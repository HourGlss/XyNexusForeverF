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
        private readonly Dictionary<SpellEffectType, SpellEffectHandlerDelegate> spellEffectApplyDelegates = [];
        private readonly Dictionary<SpellEffectType, SpellEffectHandlerDelegate> spellEffectRemoveDelegates = [];
        private readonly Dictionary<SpellEffectType, System.Type> spellEffectApplyTypes = [];
        private readonly Dictionary<SpellEffectType, System.Type> spellEffectRemoveTypes = [];

        public void Initialise()
        {
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
                    ParameterExpression spellParameter   = Expression.Parameter(typeof(ISpell));
                    ParameterExpression entityParameter  = Expression.Parameter(typeof(IUnitEntity));
                    ParameterExpression infoParameter    = Expression.Parameter(typeof(ISpellTargetEffectInfo));
                    ParameterExpression dataParameter    = Expression.Parameter(typeof(ISpellEffectData));

                    MethodCallExpression call =
                        Expression.Call(Expression.Convert(handlerParameter, type), methodInfo,
                        spellParameter, entityParameter, infoParameter, Expression.Convert(dataParameter, dataType));

                    Expression<SpellEffectHandlerDelegate> lambda =
                        Expression.Lambda<SpellEffectHandlerDelegate>(call, handlerParameter, spellParameter,
                        entityParameter, infoParameter, dataParameter);
                    SpellEffectHandlerDelegate handlerDelegate = lambda.Compile();

                    if (interfaceType.GetGenericTypeDefinition() == typeof(ISpellEffectApplyHandler<>))
                    {
                        spellEffectApplyDelegates.Add(attribute.SpellEffectType, handlerDelegate);
                        spellEffectApplyTypes.Add(attribute.SpellEffectType, interfaceType);
                    }
                    else if (interfaceType.GetGenericTypeDefinition() == typeof(ISpellEffectRemoveHandler<>))
                    {
                        spellEffectRemoveDelegates.Add(attribute.SpellEffectType, handlerDelegate);
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
        /// Get spell effect apply handler <see cref="SpellEffectHandlerDelegate"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        public SpellEffectHandlerDelegate GetSpellEffectApplyDelegate(SpellEffectType type)
        {
            return spellEffectApplyDelegates.TryGetValue(type, out SpellEffectHandlerDelegate handler) ? handler : null;
        }

        /// <summary>
        /// Get spell effect remove handler <see cref="SpellEffectHandlerDelegate"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        public SpellEffectHandlerDelegate GetSpellEffectRemoveDelegate(SpellEffectType type)
        {
            return spellEffectRemoveDelegates.TryGetValue(type, out SpellEffectHandlerDelegate handler) ? handler : null;
        }
    }
}
