using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
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
        public void InvokeApplyHandler(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            System.Type handlerType = globalSpellEffectManager.GetSpellEffectApplyHandlerType(info.Entry.EffectType);
            if (handlerType == null)
            {
                log.LogWarning($"Unhandled handler for spell effect {info.Entry.EffectType}!");
                return;
            }

            SpellEffectHandlerDelegate handlerDelegate = globalSpellEffectManager.GetSpellEffectApplyDelegate(info.Entry.EffectType);
            if (handlerDelegate == null)
            {
                log.LogWarning($"Unhandled handler for spell effect {info.Entry.EffectType}, missing delegate!");
                return;
            }

            InvokeHandler(spell, target, info, handlerType, handlerDelegate);
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

            SpellEffectHandlerDelegate handlerDelegate = globalSpellEffectManager.GetSpellEffectRemoveDelegate(info.Entry.EffectType);
            if (handlerDelegate == null)
            {
                log.LogWarning($"Unhandled handler for spell effect {info.Entry.EffectType}, missing delegate!");
                return;
            }

            InvokeHandler(spell, target, info, handlerType, handlerDelegate);
        }

        private void InvokeHandler(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, System.Type handlerType, SpellEffectHandlerDelegate handlerDelegate)
        {
            object handler = serviceProvider.GetKeyedService(handlerType, info.Entry.EffectType);
            if (handler == null)
            {
                log.LogWarning($"Unhandled handler for spell effect {info.Entry.EffectType}, type not registered!");
                return;
            }

            System.Type dataType = globalSpellEffectManager.GetSpellEffectDataType(info.Entry.EffectType);
            if (dataType == null)
            {
                log.LogWarning($"Unhandled data for spell effect {info.Entry.EffectType}!");
                return;
            }

            ISpellEffectData data = (ISpellEffectData)serviceProvider.GetService(dataType);
            if (data == null)
            {
                log.LogWarning($"Unhandled data for spell effect {info.Entry.EffectType}, type not registered!");
                return;
            }

            data.Populate(info.Entry);
            handlerDelegate.Invoke(handler, spell, target, info, data);
        }
    }
}
