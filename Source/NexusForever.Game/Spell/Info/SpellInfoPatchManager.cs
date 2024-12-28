using System.Reflection;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Abstract.Spell.Info.Patch;

namespace NexusForever.Game.Spell.Info
{
    public class SpellInfoPatchManager : ISpellInfoPatchManager
    {
        public uint NextSpellEffectId => nextSpellEffectId++;
        private uint nextSpellEffectId = 10_000_000;

        private readonly Dictionary<uint, System.Type> spellBaseInfoBaseIdPatches = [];
        private readonly Dictionary<uint, System.Type> spellInfoBaseIdPatches = [];

        #region Dependency Injection

        private readonly IServiceProvider serviceProvider;

        public SpellInfoPatchManager(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        #endregion

        public void Initialise()
        {
            foreach (System.Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                InitialiseSpellInfoBaseIdPatches(type);
            }
        }

        private void InitialiseSpellInfoBaseIdPatches(System.Type type)
        {
            var attribute = type.GetCustomAttribute<SpellInfoPatchSpellBaseIdAttribute>();
            if (attribute == null)
                return;

            if (type.IsAssignableTo(typeof(ISpellInfoPatch)))
                spellInfoBaseIdPatches[attribute.BaseSpellId] = type;
            else if (type.IsAssignableTo(typeof(ISpellBaseInfoPatch)))
                spellBaseInfoBaseIdPatches[attribute.BaseSpellId] = type;
        }

        /// <summary>
        /// Patch the supplied <see cref="ISpellBaseInfo"/> with any defined matching patches.
        /// </summary>
        public void Patch(ISpellBaseInfo spellBaseInfo)
        {
            ISpellBaseInfoPatch patch = GetSpellBaseInfoPatch(spellBaseInfo.Entry.Id, spellBaseInfoBaseIdPatches);
            patch?.Patch(spellBaseInfo);
        }

        private ISpellBaseInfoPatch GetSpellBaseInfoPatch(uint id, Dictionary<uint, System.Type> patches)
        {
            if (!patches.TryGetValue(id, out System.Type type))
                return null;

            return (ISpellBaseInfoPatch)serviceProvider.GetService(type);
        }

        /// <summary>
        /// Patch the supplied <see cref="ISpellInfo"/> with any defined matching patches.
        /// </summary>
        public void Patch(ISpellInfo spellInfo)
        {
            ISpellInfoPatch patch = GetSpellInfoPatch(spellInfo.BaseInfo.Entry.Id, spellInfoBaseIdPatches);
            patch?.Patch(spellInfo);
        }

        private ISpellInfoPatch GetSpellInfoPatch(uint id, Dictionary<uint, System.Type> patches)
        {
            if (!patches.TryGetValue(id, out System.Type type))
                return null;

            return (ISpellInfoPatch)serviceProvider.GetService(type);
        }
    }
}
