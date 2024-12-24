using System.Linq.Expressions;
using System.Reflection;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect
{
    public class GlobalSpellEffectManager : IGlobalSpellEffectManager
    {
        /// <summary>
        /// Id to be assigned to the next spell effect.
        /// </summary>
        public uint NextEffectId => nextEffectId++;
        private uint nextEffectId = 1;

        private readonly Dictionary<SpellEffectType, System.Type> spellEffectDataTypes = [];
        private readonly Dictionary<SpellEffectType, SpellEffectHandlerApplyDelegate> spellEffectApplyDelegates = [];
        private readonly Dictionary<SpellEffectType, SpellEffectHandlerRemoveDelegate> spellEffectRemoveDelegates = [];
        private readonly Dictionary<SpellEffectType, System.Type> spellEffectApplyTypes = [];
        private readonly Dictionary<SpellEffectType, System.Type> spellEffectRemoveTypes = [];

        public void Initialise()
        {
            // create delegates for each spell effect apply and remove handler that upcasts the handler and data object before invoke.
            foreach (System.Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var attribute = type.GetCustomAttribute<SpellEffectHandlerAttribute>();
                if (attribute == null)
                    continue;

                foreach (System.Type interfaceType in type.GetInterfaces()
                    .Where(i => i.IsGenericType))
                {
                    if (interfaceType.GetGenericTypeDefinition() != typeof(ISpellEffectApplyHandler<>)
                        && interfaceType.GetGenericTypeDefinition() != typeof(ISpellEffectRemoveHandler<>))
                        continue;

                    System.Type[] types = interfaceType.GetGenericArguments();
                    System.Type dataType = types[0];
                    spellEffectDataTypes.TryAdd(attribute.SpellEffectType, dataType);

                    InterfaceMapping map = type.GetInterfaceMap(interfaceType);
                    MethodInfo methodInfo = map.TargetMethods[0];

                    ParameterExpression handlerParameter = Expression.Parameter(typeof(object));
                    ParameterExpression entityParameter  = Expression.Parameter(typeof(IUnitEntity));
                    ParameterExpression infoParameter    = Expression.Parameter(typeof(ISpellTargetEffectInfo));
                    ParameterExpression dataParameter    = Expression.Parameter(typeof(ISpellEffectData));

                    if (interfaceType.GetGenericTypeDefinition() == typeof(ISpellEffectApplyHandler<>))
                    {
                        ParameterExpression executionContextParameter = Expression.Parameter(typeof(ISpellExecutionContext));

                        MethodCallExpression call =
                            Expression.Call(Expression.Convert(handlerParameter, type), methodInfo,
                            executionContextParameter, entityParameter, infoParameter, Expression.Convert(dataParameter, dataType));

                        Expression<SpellEffectHandlerApplyDelegate> lambda =
                            Expression.Lambda<SpellEffectHandlerApplyDelegate>(call, handlerParameter,
                            executionContextParameter, entityParameter, infoParameter, dataParameter);

                        spellEffectApplyDelegates.Add(attribute.SpellEffectType, lambda.Compile());
                        spellEffectApplyTypes.Add(attribute.SpellEffectType, interfaceType);
                    }
                    else if (interfaceType.GetGenericTypeDefinition() == typeof(ISpellEffectRemoveHandler<>))
                    {
                        ParameterExpression spellParameter = Expression.Parameter(typeof(ISpell));

                        MethodCallExpression call =
                            Expression.Call(Expression.Convert(handlerParameter, type), methodInfo,
                            spellParameter, entityParameter, infoParameter, Expression.Convert(dataParameter, dataType));

                        Expression<SpellEffectHandlerRemoveDelegate> lambda =
                            Expression.Lambda<SpellEffectHandlerRemoveDelegate>(call, handlerParameter, 
                            spellParameter, entityParameter, infoParameter, dataParameter);

                        spellEffectRemoveDelegates.Add(attribute.SpellEffectType, lambda.Compile());
                        spellEffectRemoveTypes.Add(attribute.SpellEffectType, interfaceType);
                    }
                }
            }
        }

        /// <summary>
        /// Get spell effect data <see cref="System.Type"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        public System.Type GetSpellEffectDataType(SpellEffectType type)
        {
            return spellEffectDataTypes.TryGetValue(type, out System.Type dataType) ? dataType : null;
        }

        /// <summary>
        /// Get spell effect apply handler <see cref="System.Type"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        public System.Type GetSpellEffectApplyHandlerType(SpellEffectType type)
        {
            return spellEffectApplyTypes.TryGetValue(type, out System.Type handlerType) ? handlerType : null;
        }

        /// <summary>
        /// Get spell effect remove handler <see cref="System.Type"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        public System.Type GetSpellEffectRemoveHandlerType(SpellEffectType type)
        {
            return spellEffectRemoveTypes.TryGetValue(type, out System.Type handlerType) ? handlerType : null;
        }

        /// <summary>
        /// Get spell effect apply handler <see cref="SpellEffectHandlerApplyDelegate"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        public SpellEffectHandlerApplyDelegate GetSpellEffectApplyDelegate(SpellEffectType type)
        {
            return spellEffectApplyDelegates.TryGetValue(type, out SpellEffectHandlerApplyDelegate handler) ? handler : null;
        }

        /// <summary>
        /// Get spell effect remove handler <see cref="SpellEffectHandlerRemoveDelegate"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        public SpellEffectHandlerRemoveDelegate GetSpellEffectRemoveDelegate(SpellEffectType type)
        {
            return spellEffectRemoveDelegates.TryGetValue(type, out SpellEffectHandlerRemoveDelegate handler) ? handler : null;
        }
    }
}
