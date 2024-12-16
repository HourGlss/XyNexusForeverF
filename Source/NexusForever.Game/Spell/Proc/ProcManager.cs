using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Proc;
using NexusForever.Game.Static.Spell.Proc;
using NexusForever.GameTable.Model;
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
        /// Apply <see cref="IProcInfo"/> for supplied <see cref="Spell4EffectsEntry"/>.
        /// </summary>
        public void ApplyProc(Spell4EffectsEntry entry)
        {
            ProcType procType = (ProcType)entry.DataBits00;

            if (!procs.TryGetValue(procType, out Dictionary<uint, IProcInfo> procList))
            {
                procList = [];
                procs.Add(procType, procList);
            }

            if (procList.ContainsKey(entry.Id))
            {
                log.LogWarning($"Failed to apply proc {entry.Id} to owner {Owner.Guid}, it already exists!");
                return;
            }

            IProcInfo proc = procFactory.Resolve();
            proc.Initialise(Owner, entry);
            procList.Add(entry.Id, proc);

            log.LogTrace($"Applied proc {entry.Id} to owner {Owner.Guid}.");
        }

        /// <summary>
        /// Remove <see cref="IProcInfo"/> for supplied <see cref="Spell4EffectsEntry"/>.
        /// </summary>
        public void RemoveProc(Spell4EffectsEntry entry)
        {
            if (!procs.TryGetValue((ProcType)entry.DataBits00, out Dictionary<uint, IProcInfo> procList))
                return;

            procList.Remove(entry.Id);
            log.LogTrace($"Removed proc {entry.Id} from owner {Owner.Guid}.");
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
