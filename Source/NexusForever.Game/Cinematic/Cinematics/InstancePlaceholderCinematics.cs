using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;

namespace NexusForever.Game.Cinematic.Cinematics
{
    // These branch-imported instance scripts reference cinematics that do not yet
    // have packet-authored implementations. Keep them as no-op completions so the
    // instance flow stays safe until we can script the real playback from captures.
    public abstract class InstancePlaceholderCinematic : CinematicBase
    {
        protected override void Setup()
        {
            Duration = 0;
            InitialFlags = 0;
            InitialCancelMode = 0;
            CinematicId = 0;
        }
    }

    public class DatascapeAvatusSpawn : InstancePlaceholderCinematic, IDatascapeAvatusSpawn
    {
    }

    public class DeepSpaceExplorationOnCreate : InstancePlaceholderCinematic, IDeepSpaceExplorationOnCreate
    {
    }

    public class FragmentZeroFirstWarning : InstancePlaceholderCinematic, IFragmentZeroFirstWarning
    {
    }

    public class FragmentZeroOnCreate : InstancePlaceholderCinematic, IFragmentZeroOnCreate
    {
    }

    public class GauntletFindOutWhatHappened : InstancePlaceholderCinematic, IGauntletFindOutWhatHappened
    {
    }

    public class GauntletOnSpawnBrickBraggor : InstancePlaceholderCinematic, IGauntletOnSpawnBrickBraggor
    {
    }

    public class GeneticArchivesOpenOhmnaDoor : InstancePlaceholderCinematic, IGeneticArchivesOpenOhmnaDoor
    {
    }

    public class InfestationOnCreate : InstancePlaceholderCinematic, IInfestationOnCreate
    {
    }

    public class InitializationCoreY83OpenDoor : InstancePlaceholderCinematic, IInitializationCoreY83OpenDoor
    {
    }

    public class JourneyIntoOMNICore1OnCreate : InstancePlaceholderCinematic, IJourneyIntoOMNICore1OnCreate
    {
    }

    public class ProtostarSuperMallInTheSkyOnCreate : InstancePlaceholderCinematic, IProtostarSuperMallInTheSkyOnCreate
    {
    }

    public class RuinsOfKelVorethEnter : InstancePlaceholderCinematic, IRuinsOfKelVorethEnter
    {
    }

    public class ShadesEveOnCreate : InstancePlaceholderCinematic, IShadesEveOnCreate
    {
    }

    public class SpaceMadnessOnCreate : InstancePlaceholderCinematic, ISpaceMadnessOnCreate
    {
    }

    public class StormtalonLairStormtalonReborn : InstancePlaceholderCinematic, IStormtalonLairStormtalonReborn
    {
    }
}
