using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.PublicEvent
{
    public class PublicEventTemplate : IPublicEventTemplate
    {
        public PublicEventEntry Entry { get; private set; }
        public Dictionary<uint, PublicEventObjectiveEntry> Objectives { get; private set; }
        public List<PublicEventTeamEntry> Teams { get; } = [];
        public List<PublicEventCustomStatEntry> CustomStats { get; private set; }

        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;

        public PublicEventTemplate(
            IGameTableManager  gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Initialise <see cref="PublicEventTemplate"/> with <see cref="PublicEventEntry"/>.
        /// </summary>
        public void Initialise(PublicEventEntry entry)
        {
            Entry = entry;

            Objectives = gameTableManager.PublicEventObjective.Entries
                .Where(e => e.PublicEventId == entry.Id)
                .ToDictionary(e => e.Id);

            foreach (Static.PublicEvent.PublicEventTeam team in Objectives.Values
                .Select(o => o.PublicEventTeamId)
                .Distinct())
            {
                PublicEventTeamEntry teamEntry = gameTableManager.PublicEventTeam.GetEntry((uint)team);
                if (teamEntry != null)
                    Teams.Add(teamEntry);
            }

            CustomStats = gameTableManager.PublicEventCustomStat.Entries
                .Where(e => e.PublicEventId == entry.Id)
                .OrderBy(e => e.StatIndex)
                .ToList();
        }

        /// <summary>
        /// Returns if event has live stats.
        /// </summary>
        /// <remarks>
        /// This determines if live stats should be periodically sent to members of the event.
        /// </remarks>
        public bool HasLiveStats()
        {
            return Entry.PublicEventTypeEnum
                is PublicEventType.Warplot
                or PublicEventType.BattlegroundVortex
                or PublicEventType.BattlegroundHoldTheLine
                or PublicEventType.BattlegroundCannon
                or PublicEventType.BattlegroundSabotage
                or PublicEventType.Arena;
        }

        /// <summary>
        /// Returns if the public event should be disposed when the event completes.
        /// </summary>
        public bool InstantFinalise()
        {
            return Entry.PublicEventTypeEnum
                is PublicEventType.WorldEvent
                or PublicEventType.LiveEvent
                or PublicEventType.NonCombatEvent;
        }
    }
}
