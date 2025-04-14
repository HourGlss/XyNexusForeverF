using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Effect
{
    public class SpellEffectHandlerInvoker : ISpellEffectHandlerInvoker
    {
        #region Dependency Injection

        private readonly ILogger<SpellEffectHandlerInvoker> log;

        private readonly IServiceProvider serviceProvider;
        private readonly IGlobalSpellEffectManager globalSpellEffectManager;

        public SpellEffectHandlerInvoker(
            ILogger<SpellEffectHandlerInvoker> log,
            IServiceProvider serviceProvider,
            IGlobalSpellEffectManager globalSpellEffectManager)
        {
            this.log                      = log;

            this.serviceProvider          = serviceProvider;
            this.globalSpellEffectManager = globalSpellEffectManager;
        }

        #endregion

        /// <summary>
        /// Invoke the apply handler for the given spell effect.
        /// </summary>
        public SpellEffectExecutionResult InvokeApplyHandler(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            System.Type handlerType = globalSpellEffectManager.GetSpellEffectApplyHandlerType(info.Entry.EffectType);
            if (handlerType == null)
            {
                log.LogWarning($"Unhandled handler for spell effect {info.Entry.EffectType}!");
                return SpellEffectExecutionResult.NoHandler;
            }

            object handler = CreateHandler(info, handlerType);
            if (handler == null)
                return SpellEffectExecutionResult.NoHandler;

            SpellEffectHandlerApplyDelegate handlerDelegate = globalSpellEffectManager.GetSpellEffectApplyDelegate(info.Entry.EffectType);
            if (handlerDelegate == null)
            {
                log.LogWarning($"Unhandled handler for spell effect {info.Entry.EffectType}, missing delegate!");
                return SpellEffectExecutionResult.NoHandler;
            }

            ISpellEffectData data = PopulateData(info);
            if (data == null)
                return SpellEffectExecutionResult.NoHandler;

            return handlerDelegate.Invoke(handler, executionContext, target, info, data);
        }

        /// <summary>
        /// Invoke the remove handler for the given spell effect.
        /// </summary>
        public void InvokeRemoveHandler(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            System.Type handlerType = globalSpellEffectManager.GetSpellEffectRemoveHandlerType(info.Entry.EffectType);
            // not every effect has a removal handler, fine to just return without warning
            if (handlerType == null)
                return;

            object handler = CreateHandler(info, handlerType);
            if (handler == null)
                return;

            SpellEffectHandlerRemoveDelegate handlerDelegate = globalSpellEffectManager.GetSpellEffectRemoveDelegate(info.Entry.EffectType);
            if (handlerDelegate == null)
            {
                log.LogWarning($"Unhandled handler for spell effect {info.Entry.EffectType}, missing delegate!");
                return;
            }

            ISpellEffectData data = PopulateData(info);
            if (data == null)
                return;

            handlerDelegate.Invoke(handler, spell, target, info, data);
        }

        private ISpellEffectData PopulateData(ISpellTargetEffectInfo info)
        {
            System.Type dataType = globalSpellEffectManager.GetSpellEffectDataType(info.Entry.EffectType);
            if (dataType == null)
            {
                log.LogWarning($"Unhandled data for spell effect {info.Entry.EffectType}!");
                return null;
            }

            ISpellEffectData data = (ISpellEffectData)serviceProvider.GetService(dataType);
            if (data == null)
            {
                log.LogWarning($"Unhandled data for spell effect {info.Entry.EffectType}, type not registered!");
                return null;
            }

            data.Populate(info.Entry);
            return data;
        }

        private object CreateHandler(ISpellTargetEffectInfo info, System.Type handlerType)
        {
            object handler = serviceProvider.GetKeyedService(handlerType, info.Entry.EffectType);
            if (handler == null)
            {
                log.LogWarning($"Unhandled handler for spell effect {info.Entry.EffectType}, type not registered!");
                return null;
            }

            return handler;
        }
    }
}
