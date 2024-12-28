using System.Collections;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Info
{
    public class SpellBaseInfo : ISpellBaseInfo
    {
        public Spell4BaseEntry Entry { get; private set; }
        public Spell4HitResultsEntry HitResult { get; private set; }
        public Spell4TargetMechanicsEntry TargetMechanics { get; private set; }
        public Spell4TargetAngleEntry TargetAngle { get; private set; }
        public Spell4PrerequisitesEntry Prerequisites { get; private set; }
        public Spell4ValidTargetsEntry ValidTargets { get; private set; }
        public TargetGroupEntry CastGroup { get; private set; }
        public Creature2Entry PositionalAoe { get; private set; }
        public TargetGroupEntry AoeGroup { get; private set; }
        public Spell4BaseEntry PrerequisiteSpell { get; private set; }
        public Spell4SpellTypesEntry SpellType { get; private set; }
        public bool HasIcon => Entry.SpellClass is SpellClass.BuffNonDispelRightClickOk or (>= SpellClass.BuffDispellable and <= SpellClass.DebuffNonDispellable);
        public bool IsDebuff => Entry.SpellClass is SpellClass.DebuffDispellable or SpellClass.DebuffNonDispellable;
        public bool IsBuff => Entry.SpellClass is SpellClass.BuffDispellable or SpellClass.BuffNonDispellable or SpellClass.BuffNonDispelRightClickOk;
        public bool IsDispellable => Entry.SpellClass is SpellClass.BuffDispellable or SpellClass.DebuffDispellable;

        private ISpellInfo[] spellInfoStore;

        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;
        private readonly ISpellInfoManager spellInfoManager;

        public SpellBaseInfo(
            IGameTableManager gameTableManager,
            ISpellInfoManager spellInfoManager)
        {
            this.gameTableManager = gameTableManager;
            this.spellInfoManager = spellInfoManager;
        }

        #endregion

        /// <summary>
        /// Initialise the <see cref="ISpellBaseInfo"/> with the supplied <see cref="Spell4BaseEntry"/> and <see cref="ISpellInfoCache"/>.
        /// </summary>
        public void Initialise(Spell4BaseEntry spell4BaseEntry, ISpellInfoCache spellInfoCache)
        {
            if (Entry != null)
                throw new InvalidOperationException("SpellBaseInfo already initialised.");

            Entry             = spell4BaseEntry;
            HitResult         = gameTableManager.Spell4HitResults.GetEntry(Entry.Spell4HitResultId);
            TargetMechanics   = gameTableManager.Spell4TargetMechanics.GetEntry(Entry.Spell4TargetMechanicId);
            TargetAngle       = gameTableManager.Spell4TargetAngle.GetEntry(Entry.Spell4TargetAngleId);
            Prerequisites     = gameTableManager.Spell4Prerequisites.GetEntry(Entry.Spell4PrerequisiteId);
            ValidTargets      = gameTableManager.Spell4ValidTargets.GetEntry(Entry.Spell4ValidTargetId);
            CastGroup         = gameTableManager.TargetGroup.GetEntry(Entry.TargetGroupIdCastGroup);
            PositionalAoe     = gameTableManager.Creature2.GetEntry(Entry.Creature2IdPositionalAoe);
            AoeGroup          = gameTableManager.TargetGroup.GetEntry(Entry.TargetGroupIdAoeGroup);
            PrerequisiteSpell = gameTableManager.Spell4Base.GetEntry(Entry.Spell4BaseIdPrerequisiteSpell);
            SpellType         = gameTableManager.Spell4SpellTypes.GetEntry(Entry.Spell4SpellTypesIdSpellType);

            InitialiseSpellInfo(spellInfoCache);
        }

        private void InitialiseSpellInfo(ISpellInfoCache spellInfoCache)
        {
            List<Spell4Entry> spellEntries = spellInfoCache.GetSpell4Entries(Entry.Id).ToList();
            if (spellEntries.Count < 1)
                return;

            // spell don't always have sequential tiers, create from highest tier not total
            spellInfoStore = new SpellInfo[spellEntries[0].TierIndex];

            foreach (Spell4Entry spell4Entry in spellEntries)
            {
                ISpellInfo spellInfo = spellInfoManager.GetSpellInfo(spell4Entry.Id);
                spellInfo.BaseInfo = this;
                spellInfoStore[spell4Entry.TierIndex - 1] = spellInfo;
            }
        }

        /// <summary>
        /// Return <see cref="ISpellInfo"/> for the supplied spell tier.
        /// </summary>
        public ISpellInfo GetSpellInfo(byte tier)
        {
            if (tier < 1)
                tier = 1;
            return spellInfoStore[tier - 1];
        }

        public IEnumerator<ISpellInfo> GetEnumerator()
        {
            return spellInfoStore
                .Select(s => s)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
