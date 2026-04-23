using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using System.Numerics;

namespace NexusForever.Script.Instance.Dungeon.SanctuaryOfTheSwordmaiden
{
    [ScriptFilterOwnerId(166)]
    public class SanctuaryOfTheSwordmaidenEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;
        private IMapInstance mapInstance;

        private uint FireDoorGuid;
        private uint OtherDoorGuid;

        private enum Creature
        {
            SpiritRelicOfBody   = 28638,
            SpiritRelicOfSpirit = 28643,
            SpiritRelicOfMind   = 28644
        }

        #region Dependency Injection

        private readonly IGlobalQuestManager globalQuestManager;

        public SanctuaryOfTheSwordmaidenEventScript(
            IGlobalQuestManager globalQuestManager)
        {
            this.globalQuestManager = globalQuestManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IPublicEvent owner)
        {
            publicEvent = owner;
            publicEvent.SetPhase(PublicEventPhase.Enter);

            mapInstance = publicEvent.Map as IMapInstance;
        }

        /// <summary>
        /// Invoked when a <see cref="IGridEntity"/> is added to the map the public event is on.
        /// </summary>
        public void OnAddToMap(IGridEntity entity)
        {
            switch (entity)
            {
                case IWorldEntity worldEntity:
                    OnAddToMapWorldEntity(worldEntity);
                    break;
                case IWorldLocationVolumeGridTriggerEntity worldLocationEntity:
                    OnAddToMapWorldLocationEntity(worldLocationEntity);
                    break;
            }
        }
        private void OnAddToMapWorldEntity(IWorldEntity worldEntity)
        {
            switch ((PublicEventCreature)worldEntity.CreatureId)
            {

            }
        }
        private void OnAddToMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {

            }
        }

        /// <summary>
        /// Invoked when a <see cref="IGridEntity"/> is removed from the map the public event is on.
        /// </summary>
        public void OnRemoveFromMap(IGridEntity entity)
        {
            switch (entity)
            {
                case IWorldEntity worldEntity:
                    OnRemoveFromMapWorldEntity(worldEntity);
                    break;
                case IWorldLocationVolumeGridTriggerEntity worldLocationEntity:
                    OnRemoveFromMapWorldLocationEntity(worldLocationEntity);
                    break;
            }
        }

        private void OnRemoveFromMapWorldEntity(IWorldEntity worldEntity)
        {
            switch ((PublicEventCreature)worldEntity.CreatureId)
            {

            }
        }

        private void OnRemoveFromMapWorldLocationEntity(IWorldLocationVolumeGridTriggerEntity worldLocationEntity)
        {
            switch (worldLocationEntity.Entry.Id)
            {

            }
        }

        public void OnPublicEventPhase(uint phase)
        {
            switch ((PublicEventPhase)phase)
            {
                case PublicEventPhase.RandomPath:
                    PhaseRandomPath();
                    break;
                case PublicEventPhase.TempleOfTheLifeSpeaker:
                    PhaseTempleOfTheLifeSpeaker();
                    break;
                case PublicEventPhase.MoldwoodCorruption:
                    PhaseMoldwoodCorruption();
                    break;
                case PublicEventPhase.RaynaDarkspeaker:
                    PhaseRaynaDarkspeaker();
                    break;
                case PublicEventPhase.MoldwoodOverlordSkash:
                    PhaseMoldwoodOverlordSkash();
                    break;
                case PublicEventPhase.GoToLifeWeaverTerracePath1:
                    PhaseGoToLifeWeaverTerracePath1();
                    break;
                case PublicEventPhase.GoToLifeWeaverTerracePath2:
                    PhaseGoToLifeWeaverTerracePath2();
                    break;
                case PublicEventPhase.OnduLifeWeaver:
                    PhaseOnduLifeWeaver();
                    break;
                case PublicEventPhase.PlaceSpiritRelics:
                    PhasePlaceSpiritRelics();
                    break;
                case PublicEventPhase.SpiritmotherSelene:
                    PhaseSpiritmotherSelene();
                    break;
                case PublicEventPhase.SpiritmotherSeleneTheCorrupted:
                    PhaseSpiritmotherSeleneTheCorrupted();
                    break;
            }
        }

        private void PhaseRandomPath()
        {
            publicEvent.ActivateObjective(PublicEventObjective.CollectTorineSpiritRelics);

            BroadcastCommunicatorMessage(CommunicatorMessage.SpiritMotherSelene14);

           Random rnd = new();
           uint[] Path = [0, 1];
           int Index = rnd.Next(Path.Length);
           if (Index > 0) publicEvent.SetPhase(PublicEventPhase.TempleOfTheLifeSpeaker);
           else publicEvent.SetPhase(PublicEventPhase.MoldwoodCorruption);
        }

        private void PhaseTempleOfTheLifeSpeaker()
        {
            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(FireDoorGuid);
            door?.OpenDoor();

            publicEvent.ActivateObjective(PublicEventObjective.EnterTheTempleOfTheLifeSpeaker);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatHammerfistMoldjaw);

            var triggerEntity = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
            triggerEntity.Initialise(3421, 15f, 3421);
            triggerEntity.AddToMap(mapInstance, new Vector3(4666.132f, -812.4623f, -3318.434f));
        }

        private void PhaseMoldwoodCorruption()
        {
            IDoorEntity door = mapInstance.GetEntity<IDoorEntity>(OtherDoorGuid);
            door?.OpenDoor();

            publicEvent.ActivateObjective(PublicEventObjective.ReachTheMoldwoodCorruption);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatCorruptedEdgesmithTorian);

            Random rnd = new();
            uint[] Object = [0, 1];
            int Index1 = rnd.Next(Object.Length);
            if (Index1 > 0) publicEvent.ActivateObjective(PublicEventObjective.FreeTheSpiritsOfTheCorruptedTorineSisters);// info collected from Veteran, no info from prime levels on youtube


            var triggerEntity = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
            triggerEntity.Initialise(3420, 15f, 3420);
            triggerEntity.AddToMap(mapInstance, new Vector3(3820.886f, -808.6421f, -3322.52f));
        }

        private void PhaseMoldwoodOverlordSkash()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatMoldwoodOverlordSkash);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatSkashOrHeWillCorruptThePrisoner);
            publicEvent.ActivateObjective(PublicEventObjective.DestroyTheElderMoldwoodRavager);

            Random rnd = new();
            uint[] Object = [0, 1];
            int Index1 = rnd.Next(Object.Length);
            if (Index1 > 0) publicEvent.ActivateObjective(PublicEventObjective.DestroyTheMoldwoodCorruptors);// info collected from Veteran/normal, no info from prime levels on youtube
            int Index2 = rnd.Next(Object.Length);
            if (Index2 > 0) publicEvent.ActivateObjective(PublicEventObjective.DestroyMoldwoodSkurgeAndCrawlers);// info collected from Veteran/normal, no info from prime levels on youtube
            int Index3 = rnd.Next(Object.Length);
            if (Index3 > 0) publicEvent.ActivateObjective(PublicEventObjective.UseTheSoulSporeOnMoldwoodGorgers);// info collected from Veteran/normal, no info from prime levels on youtube
        }
        private void PhaseRaynaDarkspeaker()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatRaynaDarkspeaker);
            publicEvent.ActivateObjective(PublicEventObjective.DestroyTheFlameCrazedDemon);
            publicEvent.ActivateObjective(PublicEventObjective.DodgeTorineTotemOfFlame);
            publicEvent.ActivateObjective(PublicEventObjective.DestroyTorineTotemsOfFlame); //TODO: find and add RNGs for objects

            var triggerEntity = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
            triggerEntity.Initialise(504, 15f, 504);// get real range
            triggerEntity.AddToMap(mapInstance, new Vector3(4822.64f, -797.94f, -3319.43f));//get coordinates from sniffs
        }

        private void PhaseGoToLifeWeaverTerracePath1()
        {
            publicEvent.ActivateObjective(PublicEventObjective.EnterLifeweaverTerrace);

            var triggerEntity = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
            triggerEntity.Initialise(3419, 15f, 3419);
            triggerEntity.AddToMap(mapInstance, new Vector3(4229.305f, -810.5873f, -2882.75f)); //Get real trigger range and correct coordinates
        }

        private void PhaseGoToLifeWeaverTerracePath2()
        {
            publicEvent.ActivateObjective(PublicEventObjective.EnterLifeweaverTerrace);
            publicEvent.ActivateObjective(PublicEventObjective.KillCorruptedLifecallerKhalee);// I haven't seen the this path on prime, only veteran. I haven't seen this miniboss spawn if you come from the other path

            var triggerEntity = publicEvent.CreateEntity<ITurnstileGridTriggerEntity>();
            triggerEntity.Initialise(3419, 15f, 3419);
            triggerEntity.AddToMap(mapInstance, new Vector3(4229.305f, -810.5873f, -2882.75f)); //Get real trigger range and correct coordinates
        }
        private void PhaseOnduLifeWeaver()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatOnduLifeweaver);
            publicEvent.ActivateObjective(PublicEventObjective.KillTheLifeweaverGuardian);

            Random rnd = new();
            uint[] Object = [0, 1];
            int Index1 = rnd.Next(Object.Length);
            if (Index1 > 0) publicEvent.ActivateObjective(PublicEventObjective.KillTheCorruptedTerrorantulas);
            int Index2 = rnd.Next(Object.Length);
            if (Index2 > 0) publicEvent.ActivateObjective(PublicEventObjective.DefeatCorruptedLifeweaverPell);// Is this random?
            //TODO: add more random objects
        }

        private void PhasePlaceSpiritRelics()
        {
            publicEvent.ActivateObjective(PublicEventObjective.PlaceTheSpiritRelics);
            publicEvent.ActivateObjective(PublicEventObjective.PlaceSpiritRelicsButDoNotfall);
        }

        //TODO: maybe map script does this, but we need to add a new public event, having both events run

        private void PhaseSpiritmotherSelene()
        {
            publicEvent.ActivateObjective(PublicEventObjective.EscortSpiritmotherSelene);
        }

        private void PhaseSpiritmotherSeleneTheCorrupted()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatSpiritmotherSeleneTheCorrupted);
        }

        private void BroadcastCommunicatorMessage(CommunicatorMessage message)
        {
            ICommunicatorMessage communicatorMessage = globalQuestManager.GetCommunicatorMessage(message);
            foreach (IPlayer player in mapInstance.GetPlayers())
                communicatorMessage?.Send(player.Session);
        }

        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            if (objective.Status != PublicEventStatus.Succeeded)
                return;

            switch ((PublicEventObjective)objective.Entry.Id)
            {
                case PublicEventObjective.DefeatDeadringerShallaos:
                    publicEvent.SetPhase(PublicEventPhase.RandomPath);
                    break;
                case PublicEventObjective.EnterTheTempleOfTheLifeSpeaker:
                    publicEvent.SetPhase(PublicEventPhase.RaynaDarkspeaker);
                    break;
                case PublicEventObjective.ReachTheMoldwoodCorruption:
                    publicEvent.SetPhase(PublicEventPhase.MoldwoodOverlordSkash);
                    break;
                case PublicEventObjective.DefeatRaynaDarkspeaker:
                    publicEvent.SetPhase(PublicEventPhase.GoToLifeWeaverTerracePath1);
                    break;
                case PublicEventObjective.DefeatMoldwoodOverlordSkash:
                    publicEvent.SetPhase(PublicEventPhase.GoToLifeWeaverTerracePath2);
                    break;
                case PublicEventObjective.EnterLifeweaverTerrace:
                    publicEvent.SetPhase(PublicEventPhase.OnduLifeWeaver);
                    break;
                case PublicEventObjective.CollectTorineSpiritRelics:
                    publicEvent.SetPhase(PublicEventObjective.PlaceTheSpiritRelics);
                    break;
                case PublicEventObjective.PlaceTheSpiritRelics:
                    publicEvent.SetPhase(PublicEventPhase.SpiritmotherSelene);
                    break;
                case PublicEventObjective.EscortSpiritmotherSelene:
                    publicEvent.SetPhase(PublicEventPhase.SpiritmotherSeleneTheCorrupted);
                    break;
                case PublicEventObjective.DefeatSpiritmotherSeleneTheCorrupted:
                    publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;
            }
        }
    }
}
