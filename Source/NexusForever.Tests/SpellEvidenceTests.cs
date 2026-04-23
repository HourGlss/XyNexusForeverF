using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Abstract.Spell.Info.Patch;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Spell.Effect;
using NexusForever.Game.Spell.Info.Patch;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Static;

namespace NexusForever.Tests;

public class SpellEvidenceTests
{
    [Fact]
    public void CastResultEnumMatchesClientSpell4CastResultEnvelope()
    {
        uint[] values = Enum.GetValues<CastResult>().Select(v => (uint)v).Order().ToArray();

        Assert.Equal(327, values.Length);
        Assert.Equal(0u, values.First());
        Assert.Equal(331u, values.Last());
        Assert.All(values, value => Assert.True(value < 512u));

        uint[] clientHoles = [61u, 62u, 110u, 111u, 324u];
        Assert.DoesNotContain(values, value => clientHoles.Contains(value));
    }

    [Theory]
    [InlineData(0u, CastResult.Ok)]
    [InlineData(5u, CastResult.SpellBad)]
    [InlineData(14u, CastResult.SpellGlobalCooldown)]
    [InlineData(30u, CastResult.CasterUnknown)]
    [InlineData(151u, CastResult.PrereqCasterCast)]
    [InlineData(281u, CastResult.PrereqTargetCast)]
    [InlineData(317u, CastResult.Queued)]
    [InlineData(330u, CastResult.RapidTransportInvalid)]
    [InlineData(331u, CastResult.ServiceTokensInsufficentFunds)]
    public void CastResultImportantClientIdsKeepStableValues(uint clientId, CastResult result)
    {
        Assert.Equal(clientId, (uint)result);
    }

    [Fact]
    public void GlobalSpellEffectManagerBuildsDelegatesForRegisteredHandlers()
    {
        var manager = new GlobalSpellEffectManager();
        manager.Initialise();

        foreach (Type type in typeof(GlobalSpellEffectManager).Assembly.GetTypes())
        {
            SpellEffectHandlerAttribute attribute = type.GetCustomAttribute<SpellEffectHandlerAttribute>();
            if (attribute == null)
                continue;

            foreach (Type interfaceType in type.GetInterfaces().Where(IsSpellEffectHandlerInterface))
            {
                Assert.NotNull(manager.GetSpellEffectDataType(attribute.SpellEffectType));

                if (interfaceType.GetGenericTypeDefinition() == typeof(ISpellEffectApplyHandler<>))
                {
                    Assert.Equal(interfaceType, manager.GetSpellEffectApplyHandlerType(attribute.SpellEffectType));
                    Assert.NotNull(manager.GetSpellEffectApplyDelegate(attribute.SpellEffectType));
                }
                else
                {
                    Assert.Equal(interfaceType, manager.GetSpellEffectRemoveHandlerType(attribute.SpellEffectType));
                    Assert.NotNull(manager.GetSpellEffectRemoveDelegate(attribute.SpellEffectType));
                }
            }
        }
    }

    [Theory]
    [InlineData(SpellEffectType.RavelSignal)]
    [InlineData(SpellEffectType.NpcExecutionDelay)]
    [InlineData(SpellEffectType.SummonCreature)]
    public void MissingHighFrequencySpellEffectsReturnNoHandler(SpellEffectType spellEffectType)
    {
        var manager = new GlobalSpellEffectManager();
        manager.Initialise();

        using ServiceProvider provider = new ServiceCollection().BuildServiceProvider();
        var invoker = new SpellEffectHandlerInvoker(
            NullLogger<SpellEffectHandlerInvoker>.Instance,
            provider,
            manager);

        ISpellTargetEffectInfo info = TestProxy.Create<ISpellTargetEffectInfo>(("get_Entry", new Spell4EffectsEntry
        {
            EffectType = spellEffectType
        }));

        Assert.Equal(SpellEffectExecutionResult.NoHandler, invoker.InvokeApplyHandler(null, null, info));
    }

    [Theory]
    [InlineData(typeof(BioShellVolatilitySpellInfoPatch), 35967u)]
    [InlineData(typeof(RicochetVolatilitySpellInfoPatch), 35741u)]
    public void EngineerSingleHitVolatilityPatchesAddHiddenResourceProxy(Type patchType, uint hiddenSpellId)
    {
        var effects = new List<Spell4EffectsEntry>();
        ISpellInfo spellInfo = TestProxy.Create<ISpellInfo>(("get_Effects", effects));
        ISpellInfoPatch patch = CreatePatch(patchType);

        patch.Patch(spellInfo);

        Spell4EffectsEntry effect = Assert.Single(effects);
        Assert.Equal(SpellEffectType.Proxy, effect.EffectType);
        Assert.Equal(SpellEffectTargetFlags.ImplicitTarget, effect.TargetFlags);
        Assert.Equal(hiddenSpellId, effect.DataBits00);
        Assert.Equal(1u, effect.DataBits04);
    }

    [Fact]
    public void VolatileInjectionPatchAddsPeriodicVolatilityProxy()
    {
        var effects = new List<Spell4EffectsEntry>();
        ISpellInfo spellInfo = TestProxy.Create<ISpellInfo>(("get_Effects", effects));
        ISpellInfoPatch patch = CreatePatch(typeof(VolatileInjectionVolatilitySpellInfoPatch));

        patch.Patch(spellInfo);

        Spell4EffectsEntry effect = Assert.Single(effects);
        Assert.Equal(SpellEffectType.Proxy, effect.EffectType);
        Assert.Equal(SpellEffectTargetFlags.Caster, effect.TargetFlags);
        Assert.Equal(42145u, effect.DataBits01);
        Assert.Equal(10000u, effect.DurationTime);
        Assert.Equal(1000u, effect.TickTime);
    }

    private static bool IsSpellEffectHandlerInterface(Type type)
    {
        return type.IsGenericType
            && (type.GetGenericTypeDefinition() == typeof(ISpellEffectApplyHandler<>)
                || type.GetGenericTypeDefinition() == typeof(ISpellEffectRemoveHandler<>));
    }

    private static ISpellInfoPatch CreatePatch(Type patchType)
    {
        ISpellInfoPatchManager patchManager = TestProxy.Create<ISpellInfoPatchManager>(("get_NextSpellEffectId", 10_000_000u));
        return (ISpellInfoPatch)Activator.CreateInstance(patchType, patchManager);
    }
}
