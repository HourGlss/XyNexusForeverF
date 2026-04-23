using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Pet;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Spell;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Model.Abilities;
using NexusForever.Network.World.Message.Model.Pet;
using NexusForever.Shared;
using NexusForever.Shared.Game;
using NLog;

namespace NexusForever.Game.Entity
{
    public class PetEntity : UnitEntity, IPetEntity
    {
        private const float FollowDistance = 3f;
        private const float FollowMinRecalculateDistance = 5f;
        private const ushort PrimaryPetActionBarShortcutSetId = 299;
        private const ushort PetMiniActionBarShortcutSetId = 499;
        private const uint AllPetStances = 0x1Fu;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public override EntityType Type => EntityType.Pet;

        public uint OwnerGuid { get; private set; }
        public bool IsCombatPet { get; private set; }
        public uint SummoningSpell4Id { get; private set; }
        public PetStance Stance { get; private set; }

        private ShortcutSet miniPetShortcutSet = ShortcutSet.PetMiniBar0;

        private readonly UpdateTimer followTimer = new(1d);

        #region Dependency Injection

        public PetEntity(IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory,
            IStatUpdateManager<IUnitEntity> statUpdateManager,
            ISpellFactory spellFactory)
            : base(movementManager, entitySummonFactory, statUpdateManager, spellFactory)
        {
            statUpdateManager.Initialise(this);
        }

        #endregion

        public void Initialise(IPlayer owner, ICreatureInfo creatureInfo)
        {
            OwnerGuid = owner.Guid;
            IsCombatPet = false;
            Stance = PetStance.Stay;
            Initialise(creatureInfo);

            SetBaseProperty(Property.BaseHealth, 800.0f);

            SetStat(Static.Entity.Stat.Health, 800u);
            SetStat(Static.Entity.Stat.Level, 3u);
            SetStat(Static.Entity.Stat.Sheathed, 0u);
        }

        public void InitialiseCombat(IPlayer owner, ICreatureInfo creatureInfo, uint summoningSpell4Id)
        {
            OwnerGuid             = owner.Guid;
            SummonerGuid          = owner.Guid;
            IsCombatPet           = true;
            Stance                = PetStance.Stay;
            SummoningSpell4Id     = summoningSpell4Id;

            Initialise(creatureInfo);

            Faction1 = owner.Faction1;
            Faction2 = owner.Faction2;
            UpdateCombatStats(owner);
        }

        private void UpdateCombatStats(IPlayer owner)
        {
            uint petHealth = Math.Max(1u, (uint)Math.Round(owner.Health * 0.4f));

            MaxHealth = petHealth;
            Health    = petHealth;
            Level     = owner.Level;
            Sheathed  = false;

            SetBaseProperty(Property.AssaultRating, owner.GetPropertyValue(Property.AssaultRating) * 0.4f);
            SetBaseProperty(Property.SupportRating, owner.GetPropertyValue(Property.SupportRating) * 0.4f);
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new PetEntityModel
            {
                CreatureId  = CreatureId,
                OwnerId     = OwnerGuid,
                Name        = ""
            };
        }

        public override void OnAddToMap(IBaseMap map, uint guid, Vector3 vector)
        {
            base.OnAddToMap(map, guid, vector);

            IPlayer owner = map.GetEntity<IPlayer>(OwnerGuid);
            if (owner == null)
            {
                // this shouldn't happen, log it anyway
                log.Error($"{(IsCombatPet ? "CombatPet" : "VanityPet")} {Guid} has lost it's owner {OwnerGuid}!");
                RemoveFromMap();
                return;
            }

            if (IsCombatPet)
            {
                SendCombatPetSpawn(owner);
                followTimer.Reset();
                return;
            }

            owner.VanityPetGuid = Guid;

            owner.EnqueueToVisible(new ServerPathScientistSetUnitScanParameters
            {
                UnitId = Guid,
                ScanRewardFlags  = 0,
                IsScannable  = true
            }, true);
        }

        public override void OnEnqueueRemoveFromMap()
        {
            followTimer.Reset(false);
        }

        protected override void OnRemoveFromMap()
        {
            IPlayer owner = Map?.GetEntity<IPlayer>(OwnerGuid);
            if (owner != null)
            {
                if (IsCombatPet)
                    SendCombatPetDespawn(owner);
                else if (owner.VanityPetGuid == Guid)
                    owner.VanityPetGuid = null;
            }

            base.OnRemoveFromMap();

            OwnerGuid = 0u;
            SummoningSpell4Id = 0u;
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);
            Follow(lastTick);
        }

        private void Follow(double lastTick)
        {
            followTimer.Update(lastTick);
            if (!followTimer.HasElapsed)
                return;

            IPlayer owner = GetVisible<IPlayer>(OwnerGuid);
            if (owner == null)
            {
                // this shouldn't happen, log it anyway
                log.Error($"VanityPet {Guid} has lost it's owner {OwnerGuid}!");
                RemoveFromMap();
                return;
            }

            // only recalculate the path to owner if distance is significant
            float distance = owner.Position.GetDistance(Position);
            if (distance < FollowMinRecalculateDistance)
                return;

            MovementManager.Follow(owner, FollowDistance);

            followTimer.Reset();
        }

        public void SetStance(PetStance stance)
        {
            Stance = stance;

            IPlayer owner = Map?.GetEntity<IPlayer>(OwnerGuid);
            owner?.Session.EnqueueMessageEncrypted(new ServerPetStanceChanged
            {
                PetUnitId = Guid,
                Stance    = stance
            });
        }

        private void SendCombatPetSpawn(IPlayer owner)
        {
            int petCount = owner.SummonFactory.GetSummons<IPetEntity>().Count(p => p.IsCombatPet);
            miniPetShortcutSet = GetMiniPetShortcutSet(petCount);

            if (petCount == 1)
            {
                owner.Session.EnqueueMessageEncrypted(new ServerShowActionBar
                {
                    ShortcutSet            = ShortcutSet.PrimaryPetBar,
                    ActionBarShortcutSetId = PrimaryPetActionBarShortcutSetId,
                    AssociatedUnitId       = Guid
                });
            }

            owner.Session.EnqueueMessageEncrypted(new ServerShowActionBar
            {
                ShortcutSet            = miniPetShortcutSet,
                ActionBarShortcutSetId = PetMiniActionBarShortcutSetId,
                AssociatedUnitId       = Guid
            });

            owner.Session.EnqueueMessageEncrypted(new ServerPetSpawned
            {
                PetUnitId        = Guid,
                SummoningSpell4Id = SummoningSpell4Id,
                ValidStances     = AllPetStances,
                Stance           = (uint)Stance
            });
        }

        private void SendCombatPetDespawn(IPlayer owner)
        {
            int petCount = owner.SummonFactory.GetSummons<IPetEntity>().Count(p => p.IsCombatPet);

            owner.Session.EnqueueMessageEncrypted(new ServerPetDespawned
            {
                PetUnitId = Guid
            });

            owner.Session.EnqueueMessageEncrypted(new ServerShowActionBar
            {
                ShortcutSet = miniPetShortcutSet
            });

            if (petCount == 1)
            {
                owner.Session.EnqueueMessageEncrypted(new ServerShowActionBar
                {
                    ShortcutSet = ShortcutSet.PrimaryPetBar
                });
            }
        }

        private static ShortcutSet GetMiniPetShortcutSet(int petCount)
        {
            int miniBarOffset = Math.Clamp(petCount - 1, 0, 4);
            return (ShortcutSet)((int)ShortcutSet.PetMiniBar0 + miniBarOffset);
        }
    }
}
