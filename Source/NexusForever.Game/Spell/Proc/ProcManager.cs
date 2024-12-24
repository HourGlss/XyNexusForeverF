using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Proc;
using NexusForever.Game.Static.Spell.Proc;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Proc
{
    public class ProcManager : IProcManager
    {
        public IUnitEntity Owner { get; private set; }

        private readonly Dictionary<ProcType, Dictionary<uint, IProcInfo>> procs = [];

        #region Dependency Injection

        private readonly ILogger<ProcManager> log;
        private readonly IFactory<IProcParameters> parameterFactory;
        private readonly IFactory<IProcInfo> procFactory;

        public ProcManager(
            ILogger<ProcManager> log,
            IFactory<IProcParameters> parameterFactory,
            IFactory<IProcInfo> procFactory)
        {
            this.log              = log;
            this.parameterFactory = parameterFactory;
            this.procFactory      = procFactory;
        }

        #endregion

        /// <summary>
        /// Initialise <see cref="IProcManager"/> with supplied <see cref="IUnitEntity"/> owner.
        /// </summary>
        public void Initialise(IUnitEntity owner)
        {
            if (Owner != null)
                throw new InvalidOperationException("ProcManager already initialised.");

            Owner = owner;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            foreach (IProcInfo proc in procs.Values.SelectMany(p => p.Values))
                proc.Update(lastTick);
        }

        /// <summary>
        /// Apply <see cref="IProcInfo"/> for supplied <see cref="ISpellEffectProcData"/>.
        /// </summary>
        public void ApplyProc(ISpellEffectProcData data)
        {
            if (!procs.TryGetValue(data.ProcType, out Dictionary<uint, IProcInfo> procList))
            {
                procList = [];
                procs.Add(data.ProcType, procList);
            }

            if (procList.ContainsKey(data.Entry.Id))
            {
                log.LogWarning($"Failed to apply proc {data.Entry.Id} to owner {Owner.Guid}, it already exists!");
                return;
            }

            IProcInfo proc = procFactory.Resolve();
            proc.Initialise(Owner, data);
            procList.Add(data.Entry.Id, proc);

            log.LogTrace($"Applied proc {data.Entry.Id} to owner {Owner.Guid}.");
        }

        /// <summary>
        /// Remove <see cref="IProcInfo"/> for supplied <see cref="ISpellEffectProcData"/>.
        /// </summary>
        public void RemoveProc(ISpellEffectProcData data)
        {
            if (!procs.TryGetValue(data.ProcType, out Dictionary<uint, IProcInfo> procList))
                return;

            procList.Remove(data.Entry.Id);
            log.LogTrace($"Removed proc {data.Entry.Id} from owner {Owner.Guid}.");
        }

        /// <summary>
        /// Trigger all <see cref="IProcInfo"/> of the specified <see cref="ProcType"/>.
        /// </summary>
        public void TriggerProc(ProcType type)
        {
            IProcParameters parameters = parameterFactory.Resolve();
            TriggerProc(type, parameters);
        }

        /// <summary>
        /// Trigger all <see cref="IProcInfo"/> of the specified <see cref="ProcType"/> with the supplied <see cref="IProcParameters"/>.
        /// </summary>
        public void TriggerProc(ProcType type, IProcParameters parameters)
        {
            log.LogTrace($"Triggering all procs of type {type} for owner {Owner.Guid}.");

            if (!procs.TryGetValue(type, out Dictionary<uint, IProcInfo> procList))
                return;

            foreach (IProcInfo proc in procList.Values)
                proc.Trigger(parameters);
        }
    }
}
