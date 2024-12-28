using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell
{
    public class SpellFactory : ISpellFactory
    {
        #region Dependency Injection

        private readonly IServiceProvider serviceProvider;

        public SpellFactory(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        #endregion

        /// <summary>
        /// Create a new <see cref="ISpell"/> for supplied <see cref="CastMethod"/>.
        /// </summary>
        public ISpell CreateSpell(CastMethod method)
        {
            return serviceProvider.GetKeyedService<ISpell>(method);
        }
    }
}
