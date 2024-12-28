using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Info
{
    public class SpellInfoManager : ISpellInfoManager
    {
        private readonly Dictionary<uint, ISpellInfo> spellInfoStore = [];
        private readonly Dictionary<uint, ISpellBaseInfo> spellBaseInfoStore = [];

        #region Dependency Injection

        private readonly ILogger<SpellInfoManager> log;

        private readonly IGameTableManager gameTableManager;
        private readonly IFactory<ISpellInfo> spellInfoFactory;
        private readonly IFactory<ISpellBaseInfo> spellBaseInfoFactory;
        private readonly IFactory<ISpellInfoCache> spellInfoCacheFactory;
        private readonly ISpellInfoPatchManager spellInfoPatchManager;

        public SpellInfoManager(
            ILogger<SpellInfoManager> log,
            IGameTableManager gameTableManager,
            IFactory<ISpellInfo> spellInfoFactory,
            IFactory<ISpellBaseInfo> spellBaseInfoFactory,
            IFactory<ISpellInfoCache> spellInfoCacheFactory,
            ISpellInfoPatchManager spellInfoPatchManager)
        {
            this.log                   = log;

            this.gameTableManager      = gameTableManager;
            this.spellInfoFactory      = spellInfoFactory;
            this.spellBaseInfoFactory  = spellBaseInfoFactory;
            this.spellInfoCacheFactory = spellInfoCacheFactory;
            this.spellInfoPatchManager = spellInfoPatchManager;
        }

        #endregion

        public void Initialise()
        {
            Stopwatch sw = Stopwatch.StartNew();
            log.LogInformation("Generating spell info...");

            ISpellInfoCache spellInfoCache = spellInfoCacheFactory.Resolve();
            spellInfoCache.Initialise();

            InitialiseSpellInfo(spellInfoCache);
            InitialiseSpellBaseInfo(spellInfoCache);

            foreach (ISpellInfo spellInfo in spellInfoStore.Values)
                spellInfoPatchManager.Patch(spellInfo);
            foreach (ISpellBaseInfo spellBaseInfo in spellBaseInfoStore.Values)
                spellInfoPatchManager.Patch(spellBaseInfo);

            log.LogInformation($"Cached {spellBaseInfoStore.Count} base spells and {spellInfoStore.Count} spells in {sw.ElapsedMilliseconds}ms.");
        }

        private void InitialiseSpellInfo(ISpellInfoCache spellInfoCache)
        {
            foreach (Spell4Entry entry in gameTableManager.Spell4.Entries)
            {
                ISpellInfo spellInfo = spellInfoFactory.Resolve();
                spellInfo.Initialise(entry, spellInfoCache);
                spellInfoStore.Add(entry.Id, spellInfo);
            }

            foreach (ISpellInfo spellInfo in spellInfoStore.Values)
                spellInfo.InitialiseLate();
        }

        private void InitialiseSpellBaseInfo(ISpellInfoCache spellInfoCache)
        {
            foreach (Spell4BaseEntry entry in gameTableManager.Spell4Base.Entries)
            {
                ISpellBaseInfo spellBaseInfo = spellBaseInfoFactory.Resolve();
                spellBaseInfo.Initialise(entry, spellInfoCache);
                spellBaseInfoStore.Add(entry.Id, spellBaseInfo);
            }
        }

        /// <summary>
        /// Return <see cref="ISpellInfo"/> with the supplied spell4Id.
        /// </summary>
        public ISpellInfo GetSpellInfo(uint spell4Id)
        {
            return spellInfoStore.TryGetValue(spell4Id, out ISpellInfo spellInfo) ? spellInfo : null;
        }

        /// <summary>
        /// Return <see cref="ISpellBaseInfo"/> with the supplied spell4BaseId.
        /// </summary>
        public ISpellBaseInfo GetSpellBaseInfo(uint spell4BaseId)
        {
            return spellBaseInfoStore.TryGetValue(spell4BaseId, out ISpellBaseInfo spellBaseInfo) ? spellBaseInfo : null;
        }
    }
}
