using NexusForever.GameTable;
using NexusForever.GameTable.Static;

namespace NexusForever.Script.Template.Filter.Dynamic
{
    public class ScriptFilterDynamicCombatAI : IScriptFilterDynamicCombatAI
    {
        private HashSet<uint> creatureIds;

        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;

        public ScriptFilterDynamicCombatAI(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        public void Initialise(Type scriptType)
        {
            creatureIds = gameTableManager.Creature2.Entries
                .Where(e => (e.Flags & CreatureFlags.NoAI) == 0)
                .Select(e => e.Id)
                .ToHashSet();
        }

        /// <summary>
        /// Returns if <see cref="IScriptFilterSearch"/> can match dynamic filter.
        /// </summary>
        public bool Match(IScriptFilterSearch search)
        {
            if (creatureIds != null)
            {
                if (search.CreatureId == null)
                    return false;

                return creatureIds.Contains(search.CreatureId.Value);
            }

            return true;
        }
    }
}
