using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Movement.Command.Mode;
using NexusForever.Game.Abstract.Entity.Movement.Command.Move;
using NexusForever.Game.Abstract.Entity.Movement.Command.Platform;
using NexusForever.Game.Abstract.Entity.Movement.Command.Position;
using NexusForever.Game.Abstract.Entity.Movement.Command.Rotation;
using NexusForever.Game.Abstract.Entity.Movement.Command.Scale;
using NexusForever.Game.Abstract.Entity.Movement.Command.State;
using NexusForever.Game.Abstract.Entity.Movement.Command.Time;
using NexusForever.Game.Abstract.Entity.Movement.Command.Velocity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Entity;
using NexusForever.Game.Entity.Movement.Command;
using NexusForever.Game.Entity.Movement.Command.Mode;
using NexusForever.Game.Entity.Movement.Command.Move;
using NexusForever.Game.Entity.Movement.Command.Platform;
using NexusForever.Game.Entity.Movement.Command.Position;
using NexusForever.Game.Entity.Movement.Command.Rotation;
using NexusForever.Game.Entity.Movement.Command.Scale;
using NexusForever.Game.Entity.Movement.Command.State;
using NexusForever.Game.Entity.Movement.Command.Time;
using NexusForever.Game.Entity.Movement.Command.Velocity;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Spell.Effect;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message;

namespace NexusForever.Tests;

public class DependencyInjectionSmokeTests
{
    [Fact]
    public void EntityInterfacesUsedByWorldEntityFactoryAreRegistered()
    {
        var services = new ServiceCollection();
        services.AddGameEntity();

        AssertRegistered<INonPlayerEntity>(services);
        AssertRegistered<IChestEntity>(services);
        AssertRegistered<IDestructibleEntity>(services);
        AssertRegistered<IVehicleEntity>(services);
        AssertRegistered<IDoorEntity>(services);
        AssertRegistered<IHarvestUnitEntity>(services);
        AssertRegistered<ICorpseUnitEntity>(services);
        AssertRegistered<IMountEntity>(services);
        AssertRegistered<ICollectableUnitEntity>(services);
        AssertRegistered<ITaxiEntity>(services);
        AssertRegistered<ISimpleEntity>(services);
        AssertRegistered<IPlatformEntity>(services);
        AssertRegistered<IMailboxEntity>(services);
        AssertRegistered<IAiTurretEntity>(services);
        AssertRegistered<IInstancePortalEntity>(services);
        AssertRegistered<IPlugEntity>(services);
        AssertRegistered<IResidenceEntity>(services);
        AssertRegistered<IStructuredPlugEntity>(services);
        AssertRegistered<IPinataLootEntity>(services);
        AssertRegistered<IBindPointEntity>(services);
        AssertRegistered<IPlayer>(services);
        AssertRegistered<IHiddenEntity>(services);
        AssertRegistered<ITriggerEntity>(services);
        AssertRegistered<IGhostEntity>(services);
        AssertRegistered<IPetEntity>(services);
        AssertRegistered<IEsperPetEntity>(services);
        AssertRegistered<IWorldUnitEntity>(services);
        AssertRegistered<IScannerUnitEntity>(services);
        AssertRegistered<ICameraEntity>(services);
        AssertRegistered<ITrapEntity>(services);
        AssertRegistered<IDestructibleDoorEntity>(services);
        AssertRegistered<IPickupEntity>(services);
        AssertRegistered<ISimpleCollidableEntity>(services);
        AssertRegistered<IHousingMannequinEntity>(services);
        AssertRegistered<IHousingHarvestPlugEntity>(services);
        AssertRegistered<IHousingPlantEntity>(services);
        AssertRegistered<ILockboxEntity>(services);
    }

    [Fact]
    public void MovementCommandGroupsAndCommandsAreResolvable()
    {
        var services = new ServiceCollection();
        services.AddGameEntityMovementCommand();

        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IModeCommandGroup>());
        Assert.NotNull(provider.GetRequiredService<IMoveCommandGroup>());
        Assert.NotNull(provider.GetRequiredService<IPlatformCommandGroup>());
        Assert.NotNull(provider.GetRequiredService<IPositionCommandGroup>());
        Assert.NotNull(provider.GetRequiredService<IRotationCommandGroup>());
        Assert.NotNull(provider.GetRequiredService<IScaleCommandGroup>());
        Assert.NotNull(provider.GetRequiredService<IStateCommandGroup>());
        Assert.NotNull(provider.GetRequiredService<ITimeCommandGroup>());
        Assert.NotNull(provider.GetRequiredService<IVelocityCommandGroup>());

        Assert.NotNull(provider.GetRequiredService<ModeCommand>());
        Assert.NotNull(provider.GetRequiredService<MoveCommand>());
        Assert.NotNull(provider.GetRequiredService<PlatformCommand>());
        Assert.NotNull(provider.GetRequiredService<PositionCommand>());
        Assert.NotNull(provider.GetRequiredService<RotationCommand>());
        Assert.NotNull(provider.GetRequiredService<ScaleCommand>());
        Assert.NotNull(provider.GetRequiredService<StateCommand>());
        Assert.NotNull(provider.GetRequiredService<TimeCommand>());
        Assert.NotNull(provider.GetRequiredService<VelocityCommand>());
    }

    [Fact]
    public void SpellEffectHandlersHaveKeyedRegistrations()
    {
        var services = new ServiceCollection();
        services.AddGameSpellEffect();

        foreach (Type type in typeof(GlobalSpellEffectManager).Assembly.GetTypes())
        {
            SpellEffectHandlerAttribute[] attributes = type.GetCustomAttributes<SpellEffectHandlerAttribute>().ToArray();
            if (attributes.Length == 0)
                continue;

            foreach (SpellEffectHandlerAttribute attribute in attributes)
                foreach (Type interfaceType in type.GetInterfaces().Where(IsSpellEffectHandlerInterface))
                    AssertKeyedRegistration(services, interfaceType, type, attribute.SpellEffectType);
        }
    }

    [Fact]
    public void PrerequisiteChecksHaveKeyedRegistrations()
    {
        var services = new ServiceCollection();
        services.AddGamePrerequisite();

        foreach (Type type in typeof(PrerequisiteManager).Assembly.GetTypes())
        {
            PrerequisiteCheckAttribute attribute = type.GetCustomAttribute<PrerequisiteCheckAttribute>();
            if (attribute == null)
                continue;

            AssertKeyedRegistration(services, typeof(IPrerequisiteCheck), type, attribute.Type);
        }
    }

    [Fact]
    public void WorldPacketModelsHaveKeyedRegistrationsForTheirOpcodes()
    {
        var services = new ServiceCollection();
        services.AddNetworkWorldMessage();

        foreach (Type type in typeof(NexusForever.Network.World.ServiceCollectionExtensions).Assembly.GetTypes())
        {
            MessageAttribute attribute = type.GetCustomAttribute<MessageAttribute>();
            if (attribute == null)
                continue;

            if (typeof(IReadable).IsAssignableFrom(type))
                AssertKeyedRegistration<IReadable>(services, type, attribute.Opcode);

            if (typeof(IWritable).IsAssignableFrom(type))
                AssertKeyedRegistration<IWritable>(services, type, attribute.Opcode);
        }
    }

    private static bool IsSpellEffectHandlerInterface(Type type)
    {
        return type.IsGenericType
            && (type.GetGenericTypeDefinition() == typeof(ISpellEffectApplyHandler<>)
                || type.GetGenericTypeDefinition() == typeof(ISpellEffectRemoveHandler<>));
    }

    private static void AssertRegistered<TService>(IServiceCollection services)
    {
        Assert.Contains(services, d => d.ServiceType == typeof(TService));
    }

    private static void AssertKeyedRegistration<TService>(IServiceCollection services, Type implementationType, object serviceKey)
    {
        AssertKeyedRegistration(services, typeof(TService), implementationType, serviceKey);
    }

    private static void AssertKeyedRegistration(IServiceCollection services, Type serviceType, Type implementationType, object serviceKey)
    {
        Assert.Contains(services, d =>
            d.ServiceType == serviceType
            && d.IsKeyedService
            && Equals(d.ServiceKey, serviceKey)
            && d.KeyedImplementationType == implementationType);
    }
}
