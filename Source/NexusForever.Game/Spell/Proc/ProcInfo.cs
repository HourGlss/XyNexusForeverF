using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Proc;
using NexusForever.Game.Static.Spell.Proc;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Spell.Proc
{
    public class ProcInfo : IProcInfo
    {
        public IUnitEntity Owner { get; private set; }
        public ISpellEffectProcData Data { get; private set; }
        public ProcType Type { get; private set; }

        private UpdateTimer triggerTimer;

        #region Dependency Injection

        private readonly ILogger<ProcInfo> log;
        private readonly IPrerequisiteManager prerequisiteManager;

        public ProcInfo(
            ILogger<ProcInfo> log,
            IPrerequisiteManager prerequisiteManager)
        {
            this.log                 = log;
            this.prerequisiteManager = prerequisiteManager;
        }

        #endregion

        /// <summary>
        /// Initialise <see cref="IProcInfo"/> with supplied <see cref="IUnitEntity"/> owner and <see cref="ISpellEffectProcData"/>.
        /// </summary>
        public void Initialise(IUnitEntity owner, ISpellEffectProcData data)
        {
            if (Owner != null)
                throw new InvalidOperationException("ProcInfo already initialised.");

            Owner        = owner;
            Data         = data;
            Type         = data.ProcType;

            triggerTimer = new UpdateTimer(TimeSpan.FromMilliseconds(Data.TriggerTime).TotalSeconds, false);
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            triggerTimer.Update(lastTick);
            if (!triggerTimer.HasElapsed)
                return;

            triggerTimer.Reset(false);

            log.LogTrace($"Triggering Proc {Data.Entry.Id} of {Type}.");

            Owner.CastSpell(Data.SpellId, new SpellParameters
            {
                UserInitiatedSpellCast = false
            });
        }

        /// <summary>
        /// Trigger the <see cref="IProcInfo"/> with supplied <see cref="IProcParameters"/>.
        /// </summary>
        public void Trigger(IProcParameters parameters)
        {
            log.LogWarning($"Attempting to trigger proc {Data.Entry.Id} of {Type}.");

            if (CanTrigger(parameters))
                triggerTimer.Reset(true);
        }

        private bool CanTrigger(IProcParameters parameters)
        {
            if (Data.TargetPrerequisiteId != 0)
            {
                // TODO: once the prerequisite system is updated to handle IUnitEntity, this needs to be updated
                if (parameters.Target is IPlayer player && !prerequisiteManager.Meets(player, Data.TargetPrerequisiteId))
                    return false;
            }

            if (Data.CasterPrerequisiteId != 0)
            {
                if (Owner is IPlayer player && !prerequisiteManager.Meets(player, Data.CasterPrerequisiteId))
                    return false;
            }

            if (triggerTimer.IsTicking)
                return false;

            return true;
        }
    }
}
