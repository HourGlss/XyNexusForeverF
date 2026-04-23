using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.PetCastSpell)]
    public class SpellEffectPetCastSpellHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            if (data.DataBits01 == 0u)
                return SpellEffectExecutionResult.PreventEffect;

            IPetEntity pet = GetPet(player, data.DataBits00);
            if (pet == null)
                return SpellEffectExecutionResult.PreventEffect;

            uint primaryTargetId = executionContext.Spell.Parameters.PrimaryTargetId;
            if (primaryTargetId == 0u && player.TargetGuid.HasValue)
                primaryTargetId = player.TargetGuid.Value;

            try
            {
                pet.CastSpell(data.DataBits01, new SpellParameters
                {
                    ParentSpellInfo = executionContext.Spell.Parameters.SpellInfo,
                    RootSpellInfo = executionContext.Spell.Parameters.RootSpellInfo ?? executionContext.Spell.Parameters.SpellInfo,
                    PrimaryTargetId = primaryTargetId,
                    TargetPosition = executionContext.Spell.Parameters.TargetPosition,
                    PositionalUnitId = executionContext.Spell.Parameters.PositionalUnitId,
                    UserInitiatedSpellCast = false,
                    IsProxy = true
                });
            }
            catch (ArgumentException)
            {
                return SpellEffectExecutionResult.PreventEffect;
            }

            return SpellEffectExecutionResult.Ok;
        }

        private static IPetEntity GetPet(IPlayer player, uint summoningSpell4Id)
        {
            IEnumerable<IPetEntity> combatPets = player.SummonFactory.GetSummons<IPetEntity>()
                .Where(p => p.IsCombatPet);

            if (summoningSpell4Id == 0u)
                return combatPets.FirstOrDefault();

            uint expectedBaseSpellId = GetBaseSpellId(summoningSpell4Id);
            return combatPets.FirstOrDefault(p => PetMatches(p, summoningSpell4Id, expectedBaseSpellId));
        }

        private static bool PetMatches(IPetEntity pet, uint summoningSpell4Id, uint expectedBaseSpellId)
        {
            if (pet.SummoningSpell4Id == summoningSpell4Id)
                return true;

            if (expectedBaseSpellId == 0u)
                return false;

            return GetBaseSpellId(pet.SummoningSpell4Id) == expectedBaseSpellId;
        }

        private static uint GetBaseSpellId(uint spell4Id)
        {
            Spell4Entry entry = GameTableManager.Instance.Spell4.GetEntry(spell4Id);
            return entry?.Spell4BaseIdBaseSpell ?? 0u;
        }
    }
}
