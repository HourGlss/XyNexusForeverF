using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Validator;
using NexusForever.Game.Spell.Telemetry;
using NexusForever.Network.World.Message.Static;

namespace NexusForever.Game.Spell.Validator
{
    public class CastResultValidatorManager : ICastResultValidatorManager
    {
        #region Dependency Injection

        private readonly IEnumerable<ICastResultValidator> castResultValidators;
        private readonly ISpellDiagnostics spellDiagnostics;

        public CastResultValidatorManager(
            IEnumerable<ICastResultValidator> castResultValidators,
            ISpellDiagnostics spellDiagnostics)
        {
            this.castResultValidators = castResultValidators;
            this.spellDiagnostics     = spellDiagnostics;
        }

        #endregion

        public CastResult GetCastResult(ISpell spell)
        {
            foreach (ICastResultValidator castResultValidator in castResultValidators)
            {
                CastResult result = castResultValidator.GetCastResult(spell);
                if (result != CastResult.Ok)
                {
                    spellDiagnostics.RecordValidatorFailure(spell, castResultValidator.GetType().Name, result);
                    return result;
                }
            }

            return CastResult.Ok;
        }
    }
}
