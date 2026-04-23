using System.Reflection;
using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Event;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Abstract.Spell.Info.Patch;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Abstract.Spell.Target.Implicit;
using NexusForever.Game.Spell;
using NexusForever.Game.Spell.Effect;
using NexusForever.Game.Spell.Effect.Data;
using NexusForever.Game.Spell.Effect.Handler;
using NexusForever.Game.Spell.Info.Patch;
using NexusForever.Game.Spell.Target.Implicit;
using NexusForever.Game.Spell.Target.Implicit.Filter;
using NexusForever.Game.Spell.Type;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Game.Static.Spell.Target;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Message.Static;
using NexusForever.Shared;

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
    [MemberData(nameof(ClassScoreTwoSkillEffectTypes))]
    public void ClassScoreTwoSkillEffectTypesHaveExpectedApplyCoverage(string className, string skillName, SpellEffectType[] spellEffectTypes)
    {
        var manager = new GlobalSpellEffectManager();
        manager.Initialise();

        Assert.NotEmpty(spellEffectTypes);
        foreach (SpellEffectType spellEffectType in spellEffectTypes)
        {
            bool hasApplyHandler = manager.GetSpellEffectApplyDelegate(spellEffectType) != null;
            bool knownMissingHandler = ClassScoreTwoKnownMissingApplyHandlers.Contains(spellEffectType);

            Assert.True(
                hasApplyHandler || knownMissingHandler,
                $"{className} {skillName} depends on {spellEffectType}, but no apply handler is registered and it is not documented as a known score-2 gap.");
        }
    }

    [Theory]
    [InlineData(0, 1u, true)]
    [InlineData(1, 1u, false)]
    [InlineData(1, 2u, true)]
    [InlineData(2, 6u, true)]
    [InlineData(0, 0u, false)]
    [InlineData(0, uint.MaxValue, true)]
    [InlineData(255, 1u, true)]
    public void MultiphasePhaseFlagsUseBitMaskSemantics(byte phase, uint phaseFlags, bool expected)
    {
        MethodInfo method = typeof(SpellMultiphase).GetMethod(
            "IsPhaseFlagMatch",
            BindingFlags.NonPublic | BindingFlags.Static,
            [typeof(byte), typeof(uint)]);

        Assert.NotNull(method);
        Assert.Equal(expected, (bool)method.Invoke(null, [phase, phaseFlags]));
    }

    [Fact]
    public void ProxyEffectWithZeroMaxExecutionsCanApplyToMultipleTargets()
    {
        var effect = new Spell4EffectsEntry
        {
            Id = 123u,
            DataBits00 = 42590u,
            DataBits04 = 0u
        };

        var data = new SpellEffectProxyData();
        data.Populate(effect);

        var executionContext = new SpellExecutionContext();
        executionContext.Initialise(TestProxy.Create<ISpell>(("get_Parameters", new SpellParameters())));

        var handler = new SpellEffectProxyHandler();
        ISpellTargetEffectInfo info = TestProxy.Create<ISpellTargetEffectInfo>(("get_Entry", effect));

        Assert.Equal(SpellEffectExecutionResult.Ok, handler.Apply(executionContext, CreateUnit(1u), info, data));
        executionContext.IncrementEffectTriggerCount(effect.Id);

        Assert.Equal(SpellEffectExecutionResult.Ok, handler.Apply(executionContext, CreateUnit(2u), info, data));
        Assert.Equal(2, executionContext.GetProxies().Count());
    }

    [Fact]
    public void ProxyCastsUseProxyTargetAsPrimaryTarget()
    {
        ISpellInfo parentSpellInfo = TestProxy.Create<ISpellInfo>();
        ISpellInfo rootSpellInfo = TestProxy.Create<ISpellInfo>();

        var parentParameters = new SpellParameters
        {
            SpellInfo = parentSpellInfo,
            RootSpellInfo = rootSpellInfo,
            PrimaryTargetId = 999u,
            TargetPosition = new Position(new Vector3(99f, 0f, 0f)),
            UserInitiatedSpellCast = true
        };

        var data = new SpellEffectProxyData();
        data.Populate(new Spell4EffectsEntry
        {
            DataBits00 = 12345u
        });

        IUnitEntity target = CreateUnit(42u);
        ISpell parentSpell = TestProxy.Create<ISpell>(
            ("get_CastMethod", CastMethod.Normal),
            ("get_Parameters", parentParameters));

        uint castSpellId = 0u;
        ISpellParameters capturedParameters = null;
        IUnitEntity caster = TestProxy.Create<IUnitEntity>(
            ("CastSpell", (Action<uint, ISpellParameters>)((spellId, parameters) =>
            {
                castSpellId = spellId;
                capturedParameters = parameters;
            })));

        var proxy = new Proxy(target, data, parentSpell, parentParameters);
        proxy.Evaluate();
        proxy.Cast(caster, TestProxy.Create<ISpellEventManager>());

        Assert.Equal(12345u, castSpellId);
        Assert.NotNull(capturedParameters);
        Assert.Same(parentSpellInfo, capturedParameters.ParentSpellInfo);
        Assert.Same(rootSpellInfo, capturedParameters.RootSpellInfo);
        Assert.Equal(target.Guid, capturedParameters.PrimaryTargetId);
        Assert.Equal(target.Position, capturedParameters.TargetPosition.Vector);
        Assert.True(capturedParameters.UserInitiatedSpellCast);
        Assert.True(capturedParameters.IsProxy);
    }

    [Fact]
    public void PetCastSpellUsesPlayerTargetWhenParentSpellHasNoPrimaryTarget()
    {
        ISpellInfo parentSpellInfo = TestProxy.Create<ISpellInfo>();
        ISpellInfo rootSpellInfo = TestProxy.Create<ISpellInfo>();
        var targetPosition = new Position(new Vector3(7f, 8f, 9f));

        var parentParameters = new SpellParameters
        {
            SpellInfo = parentSpellInfo,
            RootSpellInfo = rootSpellInfo,
            TargetPosition = targetPosition,
            PositionalUnitId = 555u
        };

        var executionContext = new SpellExecutionContext();
        executionContext.Initialise(TestProxy.Create<ISpell>(("get_Parameters", parentParameters)));

        var data = new SpellEffectDefaultData();
        data.Populate(new Spell4EffectsEntry
        {
            DataBits01 = 54321u
        });

        uint castSpellId = 0u;
        ISpellParameters capturedParameters = null;
        IPetEntity pet = TestProxy.Create<IPetEntity>(
            ("get_IsCombatPet", true),
            ("CastSpell", (Action<uint, ISpellParameters>)((spellId, parameters) =>
            {
                castSpellId = spellId;
                capturedParameters = parameters;
            })));

        IPlayer player = TestProxy.Create<IPlayer>(
            ("get_TargetGuid", 101u),
            ("get_SummonFactory", TestProxy.Create<IEntitySummonFactory>(
                ("GetSummons", (Func<IEnumerable<IPetEntity>>)(() => [pet])))));

        var handler = new SpellEffectPetCastSpellHandler();
        SpellEffectExecutionResult result = handler.Apply(executionContext, player, null, data);

        Assert.Equal(SpellEffectExecutionResult.Ok, result);
        Assert.Equal(54321u, castSpellId);
        Assert.NotNull(capturedParameters);
        Assert.Same(parentSpellInfo, capturedParameters.ParentSpellInfo);
        Assert.Same(rootSpellInfo, capturedParameters.RootSpellInfo);
        Assert.Equal(101u, capturedParameters.PrimaryTargetId);
        Assert.Same(targetPosition, capturedParameters.TargetPosition);
        Assert.Equal(555u, capturedParameters.PositionalUnitId);
        Assert.False(capturedParameters.UserInitiatedSpellCast);
        Assert.True(capturedParameters.IsProxy);
    }

    [Fact]
    public void TelegraphFilterKeepsTargetsInsideAnyTelegraph()
    {
        IUnitEntity caster = CreateUnit(100u);
        IUnitEntity targetInFirstTelegraph = CreateUnit(1u);
        IUnitEntity targetInSecondTelegraph = CreateUnit(2u);
        IUnitEntity targetOutsideBoth = CreateUnit(3u);

        ITelegraph firstTelegraph = TestProxy.Create<ITelegraph>();
        ITelegraph secondTelegraph = TestProxy.Create<ITelegraph>();

        var hitsByTelegraph = new Dictionary<ITelegraph, HashSet<uint>>
        {
            [firstTelegraph] = [targetInFirstTelegraph.Guid],
            [secondTelegraph] = [targetInSecondTelegraph.Guid]
        };

        var targets = new List<ISpellTargetImplicit>
        {
            new SpellTargetImplicit(targetInFirstTelegraph, 0f),
            new SpellTargetImplicit(targetInSecondTelegraph, 0f),
            new SpellTargetImplicit(targetOutsideBoth, 0f)
        };

        var filter = new SpellTargetImplicitTelegraphFilter(new FakeTelegraphSearchCheckFactory(hitsByTelegraph));
        filter.Initialise(new[] { firstTelegraph, secondTelegraph }, caster);
        filter.Filter(targets);

        Assert.Null(targets[0].Result);
        Assert.Null(targets[1].Result);
        Assert.Equal(SpellTargetImplicitSelectionResult.OutsideTelegraph, targets[2].Result);
    }

    [Fact]
    public void ImplicitSelectorUsesPositionalUnitPosition()
    {
        uint positionalUnitId = 222u;
        Vector3 casterPosition = new(1f, 2f, 3f);
        Vector3 positionalUnitPosition = new(10f, 11f, 12f);

        IWorldEntity positionalUnit = TestProxy.Create<IWorldEntity>(
            ("get_Guid", positionalUnitId),
            ("get_Position", positionalUnitPosition));

        IUnitEntity caster = TestProxy.Create<IUnitEntity>(
            ("get_Guid", 100u),
            ("get_Position", casterPosition),
            ("GetVisible", (Func<uint, IWorldEntity>)(guid => guid == positionalUnitId ? positionalUnit : null)));

        var targetMechanics = new Spell4TargetMechanicsEntry
        {
            Flags = SpellTargetMechanicFlags.IsEnemy
        };

        ISpellInfo spellInfo = TestProxy.Create<ISpellInfo>(
            ("get_Entry", new Spell4Entry
            {
                TargetMaxRange = 25f
            }),
            ("get_BaseInfo", TestProxy.Create<ISpellBaseInfo>(("get_TargetMechanics", targetMechanics))));

        var searchCheck = new CapturingImplicitSearchCheck();
        var selector = new SpellTargetImplicitSelector(searchCheck);
        selector.Initialise(caster, new SpellParameters
        {
            SpellInfo = spellInfo,
            PositionalUnitId = positionalUnitId
        });

        Assert.Same(caster, searchCheck.Searcher);
        Assert.Equal(positionalUnitPosition, searchCheck.Position);
        Assert.Equal(25f, searchCheck.Radius);
        Assert.Equal(SpellTargetMechanicFlags.IsEnemy, searchCheck.TargetMechanicFlags);
    }

    [Theory]
    [InlineData(typeof(BioShellVolatilitySpellInfoPatch), 35967u, uint.MaxValue)]
    [InlineData(typeof(PulseBlastSpellInfoPatch), 42148u, 2u)]
    [InlineData(typeof(RicochetVolatilitySpellInfoPatch), 35741u, 2u)]
    public void EngineerSingleHitVolatilityPatchesAddHiddenResourceProxy(Type patchType, uint hiddenSpellId, uint phaseFlags)
    {
        var effects = new List<Spell4EffectsEntry>();
        ISpellInfo spellInfo = TestProxy.Create<ISpellInfo>(("get_Effects", effects));
        ISpellInfoPatch patch = CreatePatch(patchType);

        patch.Patch(spellInfo);

        Spell4EffectsEntry effect = Assert.Single(effects);
        Assert.Equal(SpellEffectType.Proxy, effect.EffectType);
        Assert.Equal(SpellEffectTargetFlags.ImplicitTarget, effect.TargetFlags);
        Assert.Equal(phaseFlags, effect.PhaseFlags);
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

    private static readonly HashSet<SpellEffectType> ClassScoreTwoKnownMissingApplyHandlers =
    [
        SpellEffectType.Absorption,
        SpellEffectType.DelayDeath,
        SpellEffectType.DespawnUnit,
        SpellEffectType.DistributedDamage,
        SpellEffectType.FacilityModification,
        SpellEffectType.ForceFacing,
        SpellEffectType.ForcedAction,
        SpellEffectType.ModifyAbilityCharges,
        SpellEffectType.ModifySpell,
        SpellEffectType.ModifySpellEffect,
        SpellEffectType.ProxyChannelVariableTime,
        SpellEffectType.RavelSignal,
        SpellEffectType.SapVital,
        SpellEffectType.ShieldOverload,
        SpellEffectType.SummonCreature,
        SpellEffectType.SummonTrap,
        SpellEffectType.Transference,
    ];

    public static IEnumerable<object[]> ClassScoreTwoSkillEffectTypes()
    {
        foreach (string row in ClassScoreTwoSkillEffectTypeData.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] columns = row.Split('|', StringSplitOptions.TrimEntries);
            if (columns.Length != 3)
                throw new InvalidOperationException($"Invalid class skill evidence row: {row}");

            SpellEffectType[] spellEffectTypes = columns[2]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(Enum.Parse<SpellEffectType>)
                .ToArray();

            yield return [columns[0], columns[1], spellEffectTypes];
        }
    }

    private const string ClassScoreTwoSkillEffectTypeData = """
Warrior|Augmented Blade|VitalModifier,Damage,UnitPropertyModifier,Proc,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove
Warrior|Breaching Strikes|Damage,UnitPropertyModifier,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove,RavelSignal
Warrior|Leap|VitalModifier,ForcedMove,CCStateSet,Damage,UnitPropertyModifier,Fluff,Proxy,CCStateBreak,ForceFacing,SpellForceRemove
Warrior|Relentless Strikes|VitalModifier,Damage,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove
Warrior|Savage Strike|VitalModifier,CCStateSet,Damage,UnitPropertyModifier,Proxy
Warrior|Tremor|ForcedMove,CCStateSet,Damage,Fluff,Proxy
Warrior|Whirlwind|ForcedMove,CCStateSet,Damage,UnitPropertyModifier,Proxy,ModifyInterruptArmour,SpellForceRemove
Warrior|Atomic Spear|Damage,UnitPropertyModifier,Fluff,Proxy,DistributedDamage,SpellForceRemove
Warrior|Atomic Surge|CCStateSet,Damage,UnitPropertyModifier,Proxy
Warrior|Bolstering Strike|VitalModifier,Damage,UnitPropertyModifier,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove,HealShields
Warrior|Bum Rush|ForcedMove,CCStateSet,Damage,UnitPropertyModifier,Fluff,Proxy
Warrior|Jolt|Damage,UnitPropertyModifier,Fluff,Proxy,ModifySpellCooldown
Warrior|Menacing Strike|Damage,Heal,UnitPropertyModifier,Proxy
Warrior|Plasma Wall|VitalModifier,Damage,UnitPropertyModifier,Proc,Fluff,Proxy,SpellForceRemove
Warrior|Polarity Field|VitalModifier,Damage,UnitPropertyModifier,Fluff,Proxy
Warrior|Shield Burst|Damage,Fluff,Proxy,ShieldOverload,SpellForceRemove,HealShields
Warrior|Defense Grid|VitalModifier,Damage,UnitPropertyModifier,Proc,Fluff,Proxy,HealShields
Warrior|Emergency Reserves|VitalModifier,UnitPropertyModifier,Fluff,Proxy,HealShields
Warrior|Flash Bang|CCStateSet,Damage,Fluff,Proxy
Warrior|Grapple|CCStateSet,Damage,Proc,Fluff,Proxy
Warrior|Kick|CCStateSet,Damage,Proxy
Warrior|Plasma Blast|CCStateSet,Damage,Fluff,HealShields
Warrior|Power Link|VitalModifier,Damage,UnitPropertyModifier,Proc,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove
Warrior|Tether Bolt|CCStateSet,Damage
Engineer|Electrocute|Damage,Proxy,SpellForceRemove
Engineer|Energy Auger|CCStateSet,Damage,UnitPropertyModifier,Fluff,Proxy
Engineer|Mortar Strike|Damage,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove
Engineer|Pulse Blast|Damage,UnitPropertyModifier,Fluff,Proxy
Engineer|Quick Burst|Damage,UnitPropertyModifier,Fluff,Proxy
Engineer|Target Acquisition|VitalModifier,Damage,UnitPropertyModifier,Fluff,Proxy,ModifySpellCooldown
Engineer|Disruptive Module|Damage,UnitPropertyModifier,Fluff,Proxy,HealShields
Engineer|Feedback|Damage,UnitPropertyModifier,Proc,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove
Engineer|Flak Cannon|Damage,UnitPropertyModifier,Fluff,Proxy,SpellForceRemove
Engineer|Hyper Wave|CCStateSet,Damage,UnitPropertyModifier,Proxy
Engineer|Particle Ejector|Damage,UnitPropertyModifier,Proxy
Engineer|Ricochet|VitalModifier,CCStateSet,Damage,UnitPropertyModifier,Fluff,Proxy,ModifySpell,ModifySpellCooldown
Engineer|Obstruct Vision|CCStateSet,Damage,Proxy
Engineer|Personal Defense Unit|Heal,UnitPropertyModifier,Fluff,Proxy,Absorption,SummonTrap
Engineer|Repairbot|UnitPropertyModifier,Fluff,Proxy,FacilityModification,SummonPet
Engineer|Zap|CCStateSet,Damage,UnitPropertyModifier,SapVital
Esper|Blade Dance|Damage,UnitPropertyModifier,Fluff,Proxy,FacilityModification,ModifySpellCooldown,DespawnUnit
Esper|Concentrated Blade|Damage,Fluff,Proxy
Esper|Haunt|CCStateSet,Damage,UnitPropertyModifier,Proc,Proxy,ModifySpell,ModifySpellCooldown,SpellForceRemove
Esper|Illusionary Blades|Damage,Proc,Fluff,Proxy
Esper|Reap|VitalModifier,CCStateSet,Damage,Fluff,Proxy
Esper|Telekinetic Storm|CCStateSet,Damage,UnitPropertyModifier,Fluff,SummonCreature,Proxy
Esper|Telekinetic Strike|VitalModifier,CCStateSet,Damage,Fluff,Proxy,SpellForceRemove
Esper|Mind Over Body|VitalModifier,Heal,UnitPropertyModifier,Fluff,Proxy,SpellForceRemove
Esper|Pyrokinetic Flame|Damage,Heal,UnitPropertyModifier,Proc,Fluff,Proxy,ModifySpell
Esper|Soothe|VitalModifier,Heal,Fluff,Proxy
Esper|Crush|CCStateSet,Damage,UnitPropertyModifier,Proxy
Esper|Fixation|VitalModifier,Proxy,ModifySpellCooldown
Esper|Incapacitate|CCStateSet,Damage,UnitPropertyModifier,Proc,Proxy
Esper|Meditate|VitalModifier,Heal,UnitPropertyModifier
Esper|Projected Spirit|ForcedMove,Heal,Proxy,CCStateBreak,ModifySpellCooldown
Esper|Restraint|VitalModifier,CCStateSet,Damage,Fluff,Proxy
Esper|Shockwave|ForcedMove,CCStateSet,Damage,CCStateBreak
Medic|Annihilation|Damage,Proc,Fluff,Proxy,ModifySpellCooldown
Medic|Atomize|Damage,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove
Medic|Collider|Damage,Proxy,ModifySpellCooldown
Medic|Devastator Probes|Damage,Proxy,ModifySpellCooldown,SpellForceRemove
Medic|Discharge|Damage,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove
Medic|Fissure|Damage,UnitPropertyModifier,Proc,Fluff,Proxy,SpellForceRemove
Medic|Gamma Rays|VitalModifier,Damage,Fluff,Proxy,SpellForceRemove
Medic|Nullifier|Damage,Fluff,Proxy
Medic|Quantum Cascade|Damage,UnitPropertyModifier,Proc,Fluff,Proxy,SpellForceRemoveChanneled,ModifySpellCooldown,SpellForceRemove
Medic|Crisis Wave|VitalModifier,Heal,UnitPropertyModifier,Proc,Proxy,ModifySpell,ModifySpellCooldown,SpellForceRemove
Medic|Emission|Heal,Fluff,Proxy,ModifySpell,ModifySpellCooldown,SpellForceRemove
Medic|Extricate|ForcedMove,Heal,UnitPropertyModifier,CCStateBreak,HealShields
Medic|Flash|Heal,Proxy,HealShields
Medic|Shield Surge|Damage,UnitPropertyModifier,Fluff,Proxy,HealShields
Medic|Triage|Heal,UnitPropertyModifier,HealShields
Medic|Empowering Probes|Damage,Heal,UnitPropertyModifier,Proc,Fluff,Proxy
Medic|Field Probes|Damage,Heal,UnitPropertyModifier,Proc,Fluff,Proxy,SpellForceRemove
Medic|Magnetic Lockdown|VitalModifier,CCStateSet,Damage,Proxy,ModifySpell
Medic|Paralytic Surge|CCStateSet,Damage
Medic|Protection Probes|UnitPropertyModifier,Fluff,Proxy,HealShields
Medic|Recharge|VitalModifier,ModifySpellCooldown
Medic|Restrictor|ForcedMove,CCStateSet,UnitPropertyModifier,Fluff,Proxy
Medic|Urgency|ForcedMove,CCStateSet,Damage,Heal,UnitPropertyModifier,Proxy,CCStateBreak,SpellForceRemove
Stalker|Concussive Kicks|ForcedMove,CCStateSet,Damage,ModifySpellCooldown
Stalker|Cripple|CCStateSet,Damage,UnitPropertyModifier,Proxy,SpellForceRemove
Stalker|Impale|Damage,Proxy
Stalker|Neutralize|Damage,Proc,Fluff,Proxy,ModifySpell,ModifySpellCooldown,SpellForceRemove
Stalker|Punish|VitalModifier,Damage,UnitPropertyModifier,Proxy,ModifySpellCooldown,SpellForceRemove
Stalker|Shred|Damage,Proxy,DistributedDamage
Stalker|Decimate|VitalModifier,Damage,UnitPropertyModifier,Fluff,Proxy,ModifySpellCooldown
Stalker|Razor Disk|Damage,UnitPropertyModifier,Proxy
Stalker|Razor Storm|VitalModifier,CCStateSet,Damage,UnitPropertyModifier,Proxy,SpellForceRemove
Stalker|Steadfast|UnitPropertyModifier,Proxy
Stalker|Whiplash|VitalModifier,Damage,UnitPropertyModifier,Proxy,SpellForceRemove
Stalker|Collapse|VitalModifier,CCStateSet,Damage,Proxy,SpellForceRemove
Stalker|Pounce|VitalModifier,ForcedMove,CCStateSet,Transference,Damage,UnitPropertyModifier,Fluff,Proxy,SpellForceRemove
Stalker|Preparation|VitalModifier,Heal,UnitPropertyModifier,ForcedAction,Proxy,SpellForceRemove
Stalker|Reaver|CCStateSet,Damage,UnitPropertyModifier,Proxy,SpellForceRemove
Stalker|Stagger|CCStateSet,Damage,Proxy,ModifySpellCooldown,SpellForceRemove
Spellslinger|Arcane Missiles|VitalModifier,Damage,UnitPropertyModifier,Proc,Fluff,Proxy,DistributedDamage
Spellslinger|Assassinate|Damage,Fluff,Proxy,ModifyAbilityCharges
Spellslinger|Charged Shot|CCStateSet,Damage,Fluff,Proxy,ModifySpellCooldown
Spellslinger|Chill|CCStateSet,Damage,Proxy
Spellslinger|Flame Burst|Damage,UnitPropertyModifier,Proc,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove
Spellslinger|Ignite|Damage,Proxy
Spellslinger|Quick Draw|VitalModifier,Damage,UnitPropertyModifier,Fluff,Proxy,SpellForceRemove
Spellslinger|Rapid Fire|VitalModifier,Damage,UnitPropertyModifier,Proc,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove,ProxyChannelVariableTime
Spellslinger|True Shot|Damage,Proc,ModifySpell
Spellslinger|Wild Barrage|CCStateSet,Damage,Proxy
Spellslinger|Dual Fire|VitalModifier,Damage,Heal,Proxy
Spellslinger|Healing Salve|Heal,UnitPropertyModifier,Proc,Proxy
Spellslinger|Regenerative Pulse|VitalModifier,Heal,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove,DelayDeath
Spellslinger|Runic Healing|Heal,UnitPropertyModifier,Proxy,ModifySpell,ModifySpellEffect
Spellslinger|Sustain|Heal,Fluff,ModifySpellCooldown
Spellslinger|Vitality Burst|Heal,UnitPropertyModifier,Fluff,Proxy,ModifySpellCooldown
Spellslinger|Voidspring|Heal,UnitPropertyModifier,Fluff,Proxy
Spellslinger|Affinity|Heal,Proc,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove
Spellslinger|Flash Freeze|CCStateSet,Damage,Proxy
Spellslinger|Gate|VitalModifier,ForcedMove,CCStateSet,UnitPropertyModifier,Fluff,SummonCreature,Proxy,ModifySpellCooldown
Spellslinger|Gather Focus|UnitPropertyModifier
Spellslinger|Phase Shift|CCStateSet,Heal,UnitPropertyModifier,Proc,CCStateBreak,ModifyInterruptArmour,ModifySpellCooldown
Spellslinger|Void Pact|UnitPropertyModifier,Proc,Fluff,Proxy,ModifySpellCooldown
Spellslinger|Spell Surge|Fluff,Proxy,SpellForceRemoveChanneled,SpellForceRemove
""";

    private static ISpellInfoPatch CreatePatch(Type patchType)
    {
        ISpellInfoPatchManager patchManager = TestProxy.Create<ISpellInfoPatchManager>(("get_NextSpellEffectId", 10_000_000u));
        return (ISpellInfoPatch)Activator.CreateInstance(patchType, patchManager);
    }

    private static IUnitEntity CreateUnit(uint guid)
    {
        return TestProxy.Create<IUnitEntity>(
            ("get_Guid", guid),
            ("get_Position", new Vector3(guid, 0f, 0f)));
    }

    private sealed class FakeTelegraphSearchCheckFactory : IFactory<ISearchCheckTelegraph>
    {
        private readonly IReadOnlyDictionary<ITelegraph, HashSet<uint>> hitsByTelegraph;

        public FakeTelegraphSearchCheckFactory(IReadOnlyDictionary<ITelegraph, HashSet<uint>> hitsByTelegraph)
        {
            this.hitsByTelegraph = hitsByTelegraph;
        }

        public ISearchCheckTelegraph Resolve()
        {
            return new FakeTelegraphSearchCheck(hitsByTelegraph);
        }
    }

    private sealed class FakeTelegraphSearchCheck : ISearchCheckTelegraph
    {
        private readonly IReadOnlyDictionary<ITelegraph, HashSet<uint>> hitsByTelegraph;
        private ITelegraph telegraph;

        public FakeTelegraphSearchCheck(IReadOnlyDictionary<ITelegraph, HashSet<uint>> hitsByTelegraph)
        {
            this.hitsByTelegraph = hitsByTelegraph;
        }

        public void Initialise(ITelegraph telegraph, IUnitEntity caster)
        {
            this.telegraph = telegraph;
        }

        public bool CheckEntity(IUnitEntity entity)
        {
            return hitsByTelegraph.TryGetValue(telegraph, out HashSet<uint> targetGuids)
                && targetGuids.Contains(entity.Guid);
        }
    }

    private sealed class CapturingImplicitSearchCheck : ISearchCheckSpellTargetImplicit
    {
        public IUnitEntity Searcher { get; private set; }
        public Vector3 Position { get; private set; }
        public float Radius { get; private set; }
        public SpellTargetMechanicFlags TargetMechanicFlags { get; private set; }

        public void Initialise(IUnitEntity searcher, Vector3 position, float radius, SpellTargetMechanicFlags targetMechanicFlags)
        {
            Searcher = searcher;
            Position = position;
            Radius = radius;
            TargetMechanicFlags = targetMechanicFlags;
        }

        public bool CheckEntity(IUnitEntity entity)
        {
            return true;
        }
    }
}
