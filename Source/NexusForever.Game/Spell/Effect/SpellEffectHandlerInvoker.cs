using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Spell.Telemetry;
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
        private readonly ISpellDiagnostics spellDiagnostics;

        public SpellEffectHandlerInvoker(
            ILogger<SpellEffectHandlerInvoker> log,
            IServiceProvider serviceProvider,
            IGlobalSpellEffectManager globalSpellEffectManager,
            ISpellDiagnostics spellDiagnostics)
        {
            this.log                      = log;

            this.serviceProvider          = serviceProvider;
            this.globalSpellEffectManager = globalSpellEffectManager;
            this.spellDiagnostics         = spellDiagnostics;
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
                spellDiagnostics.RecordEffectHandlerMissing(executionContext, target, info, "handler_missing");
                return SpellEffectExecutionResult.NoHandler;
            }

            object handler = CreateHandler(info, handlerType);
            if (handler == null)
            {
                spellDiagnostics.RecordEffectHandlerMissing(executionContext, target, info, "handler_not_registered");
                return SpellEffectExecutionResult.NoHandler;
            }

            SpellEffectHandlerApplyDelegate handlerDelegate = globalSpellEffectManager.GetSpellEffectApplyDelegate(info.Entry.EffectType);
            if (handlerDelegate == null)
            {
                log.LogWarning($"Unhandled handler for spell effect {info.Entry.EffectType}, missing delegate!");
                spellDiagnostics.RecordEffectHandlerMissing(executionContext, target, info, "handler_delegate_missing");
                return SpellEffectExecutionResult.NoHandler;
            }

            ISpellEffectData data = PopulateData(info);
            if (data == null)
            {
                spellDiagnostics.RecordEffectHandlerMissing(executionContext, target, info, "effect_data_missing");
                return SpellEffectExecutionResult.NoHandler;
            }

            SpellEffectExecutionResult result = handlerDelegate.Invoke(handler, executionContext, target, info, data);
            spellDiagnostics.RecordEffectHandlerResult(executionContext, target, info, result, handlerType.Name);
            return result;
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
