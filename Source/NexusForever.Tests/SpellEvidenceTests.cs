using System.Reflection;
using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Event;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Abstract.Spell.Info.Patch;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Abstract.Spell.Target.Implicit;
using NexusForever.Game.Abstract.Spell.Validator;
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
using NexusForever.Network.World.Combat;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Message.Static;
using NexusForever.Script;
using NexusForever.Script.Template.Collection;
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
            SpellEffectHandlerAttribute[] attributes = type.GetCustomAttributes<SpellEffectHandlerAttribute>().ToArray();
            if (attributes.Length == 0)
                continue;

            foreach (SpellEffectHandlerAttribute attribute in attributes)
            {
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
    }

    [Theory]
    [InlineData(SpellEffectType.RavelSignal)]
    [InlineData(SpellEffectType.NpcExecutionDelay)]
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
    [MemberData(nameof(ClassScoreOneSkillEffectTypes))]
    public void ClassScoreOneSkillEffectTypesHaveExpectedApplyCoverage(string className, string skillName, SpellEffectType[] spellEffectTypes)
    {
        var manager = new GlobalSpellEffectManager();
        manager.Initialise();

        Assert.NotEmpty(spellEffectTypes);
        foreach (SpellEffectType spellEffectType in spellEffectTypes)
        {
            bool hasApplyHandler = manager.GetSpellEffectApplyDelegate(spellEffectType) != null;
            bool knownMissingHandler = ClassScoreOneKnownMissingApplyHandlers.Contains(spellEffectType);

            Assert.True(
                hasApplyHandler || knownMissingHandler,
                $"{className} {skillName} depends on {spellEffectType}, but no apply handler is registered and it is not documented as a known score-1 gap.");
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
    public void ProxyUsesOriginPositionForSelfTargetWhenParentSpellHasNoExplicitTargetPosition()
    {
        var originPosition = new Position(new Vector3(12f, 0f, 34f));
        var movedPosition = new Vector3(56f, 0f, 78f);

        var parentParameters = new SpellParameters
        {
            SpellInfo = TestProxy.Create<ISpellInfo>(),
            RootSpellInfo = TestProxy.Create<ISpellInfo>(),
            OriginPosition = originPosition
        };

        IUnitEntity caster = CreateUnit(42u, movedPosition);
        ISpell parentSpell = TestProxy.Create<ISpell>(
            ("get_CastMethod", CastMethod.Normal),
            ("get_Caster", caster),
            ("get_Parameters", parentParameters));

        var data = new SpellEffectProxyData();
        data.Populate(new Spell4EffectsEntry
        {
            DataBits00 = 12345u
        });

        ISpellParameters capturedParameters = null;
        IUnitEntity proxyCaster = TestProxy.Create<IUnitEntity>(
            ("CastSpell", (Action<uint, ISpellParameters>)((_, parameters) => capturedParameters = parameters)));

        var proxy = new Proxy(caster, data, parentSpell, parentParameters);
        proxy.Evaluate();
        proxy.Cast(proxyCaster, TestProxy.Create<ISpellEventManager>());

        Assert.NotNull(capturedParameters);
        Assert.Same(originPosition, capturedParameters.TargetPosition);
        Assert.Same(originPosition, capturedParameters.OriginPosition);
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

    [Fact]
    public void ImplicitSelectorUsesTargetPositionWhenNoPositionalUnitExists()
    {
        Vector3 targetPosition = new(91f, 0f, 37f);
        IUnitEntity caster = CreateUnit(100u, Vector3.Zero);

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
            TargetPosition = new Position(targetPosition)
        });

        Assert.Same(caster, searchCheck.Searcher);
        Assert.Equal(targetPosition, searchCheck.Position);
        Assert.Equal(25f, searchCheck.Radius);
        Assert.Equal(SpellTargetMechanicFlags.IsEnemy, searchCheck.TargetMechanicFlags);
    }

    [Theory]
    [InlineData(SpellEffectType.DistributedDamage)]
    [InlineData(SpellEffectType.Transference)]
    public void DamageLikeScoreTwoEffectsUseDamageHandler(SpellEffectType spellEffectType)
    {
        var manager = new GlobalSpellEffectManager();
        manager.Initialise();

        Assert.Equal(typeof(ISpellEffectApplyHandler<ISpellEffectDamageData>), manager.GetSpellEffectApplyHandlerType(spellEffectType));
        Assert.Equal(typeof(ISpellEffectDamageData), manager.GetSpellEffectDataType(spellEffectType));
        Assert.NotNull(manager.GetSpellEffectApplyDelegate(spellEffectType));
    }

    [Theory]
    [InlineData(SpellEffectType.SummonCreature)]
    [InlineData(SpellEffectType.SummonTrap)]
    public void SummonScoreTwoEffectsCreateOwnedSummonsAtTargetPosition(SpellEffectType spellEffectType)
    {
        var position = new Vector3(12f, 13f, 14f);
        var rotation = new Vector3(1f, 2f, 3f);
        var creatureInfo = TestProxy.Create<ICreatureInfo>();

        ICreatureInfo capturedCreatureInfo = null;
        Vector3 capturedPosition = default;
        Vector3 capturedRotation = default;

        IEntitySummonFactory summonFactory = TestProxy.Create<IEntitySummonFactory>(
            ("Summon", (Func<ICreatureInfo, Vector3, Vector3, OnAddDelegate, IWorldEntity>)((info, summonPosition, summonRotation, _) =>
            {
                capturedCreatureInfo = info;
                capturedPosition = summonPosition;
                capturedRotation = summonRotation;
                return TestProxy.Create<IWorldEntity>();
            })));

        IUnitEntity caster = TestProxy.Create<IUnitEntity>(
            ("get_Map", TestProxy.Create<IBaseMap>(("GetTerrainHeight", (Func<float, float, float?>)((_, _) => null)))),
            ("get_Position", Vector3.Zero),
            ("get_Rotation", rotation),
            ("get_SummonFactory", summonFactory));
        IUnitEntity target = TestProxy.Create<IUnitEntity>(("get_Position", position));

        var context = new SpellExecutionContext();
        context.Initialise(TestProxy.Create<ISpell>(("get_Caster", caster)));

        var data = new SpellEffectSummonCreatureData();
        var entry = new Spell4EffectsEntry
        {
            EffectType = spellEffectType,
            DataBits00 = 40332u,
            DataBits02 = 1u
        };
        data.Populate(entry);

        var handler = new SpellEffectSummonCreatureHandler(
            TestProxy.Create<ICreatureInfoManager>(("GetCreatureInfo", (Func<uint, ICreatureInfo>)(creatureId => creatureId == 40332u ? creatureInfo : null))));

        Assert.Equal(SpellEffectExecutionResult.Ok, handler.Apply(context, target, TestProxy.Create<ISpellTargetEffectInfo>(("get_Entry", entry)), data));
        Assert.Same(creatureInfo, capturedCreatureInfo);
        Assert.Equal(position, capturedPosition);
        Assert.Equal(rotation, capturedRotation);
    }

    [Fact]
    public void SummonCreatureDataDecodesRawAndFloatBitDistances()
    {
        var data = new SpellEffectSummonCreatureData();
        data.Populate(new Spell4EffectsEntry
        {
            DataBits03 = 20u,
            DataBits04 = BitConverter.SingleToUInt32Bits(2f)
        });

        Assert.Equal(20f, data.MinDistance);
        Assert.Equal(2f, data.MaxDistance);
    }

    [Fact]
    public void SpellDispelFinishesMatchingSpellClassUpToLimit()
    {
        int firstDebuffFinishCount = 0;
        int secondDebuffFinishCount = 0;
        int buffFinishCount = 0;

        ISpell firstDebuff = CreateDispelTargetSpell(101u, SpellClass.DebuffDispellable, () => firstDebuffFinishCount++);
        ISpell secondDebuff = CreateDispelTargetSpell(102u, SpellClass.DebuffDispellable, () => secondDebuffFinishCount++);
        ISpell buff = CreateDispelTargetSpell(103u, SpellClass.BuffDispellable, () => buffFinishCount++);
        ISpell[] activeSpells = [firstDebuff, secondDebuff, buff];

        IUnitEntity target = TestProxy.Create<IUnitEntity>(
            ("GetSpells", (Func<Func<ISpell, bool>, IEnumerable<ISpell>>)(predicate => activeSpells.Where(predicate))));

        var context = new SpellExecutionContext();
        context.Initialise(TestProxy.Create<ISpell>());

        var entry = new Spell4EffectsEntry
        {
            EffectType  = SpellEffectType.SpellDispel,
            DataBits00  = 1u,
            DataBits01  = 1u,
            DataBits03  = (uint)SpellClass.DebuffDispellable,
            DataBits04  = 1u,
            TargetFlags = SpellEffectTargetFlags.Caster
        };

        var data = new SpellEffectDefaultData();
        data.Populate(entry);

        var handler = new SpellEffectSpellDispelHandler();

        Assert.Equal(SpellEffectExecutionResult.Ok, handler.Apply(context, target, TestProxy.Create<ISpellTargetEffectInfo>(("get_Entry", entry)), data));
        Assert.Equal(1, firstDebuffFinishCount);
        Assert.Equal(0, secondDebuffFinishCount);
        Assert.Equal(0, buffFinishCount);

        CombatLogDispel combatLog = Assert.IsType<CombatLogDispel>(Assert.Single(context.GetCombatLogs()));
        Assert.True(combatLog.BRemovesSingleInstance);
        Assert.Equal(1u, combatLog.InstancesRemoved);
        Assert.Equal(101u, combatLog.SpellRemovedId);
    }

    [Fact]
    public void ScaleEffectAppliesFloatBitScaleAndRestoresDefault()
    {
        var appliedScales = new List<float>();
        IUnitEntity target = TestProxy.Create<IUnitEntity>(
            ("get_MovementManager", TestProxy.Create<IMovementManager>(("SetScale", (Action<float>)appliedScales.Add))));

        var entry = new Spell4EffectsEntry
        {
            EffectType = SpellEffectType.Scale,
            DataBits00 = BitConverter.SingleToUInt32Bits(1.1f)
        };
        var data = new SpellEffectDefaultData();
        data.Populate(entry);

        var handler = new SpellEffectScaleHandler();

        Assert.Equal(SpellEffectExecutionResult.Ok, handler.Apply(TestProxy.Create<ISpellExecutionContext>(), target, TestProxy.Create<ISpellTargetEffectInfo>(), data));
        handler.Remove(TestProxy.Create<ISpell>(), target, TestProxy.Create<ISpellTargetEffectInfo>(), data);

        Assert.Equal(new[] { 1.1f, 1f }, appliedScales);
    }

    [Fact]
    public void RapidTapThresholdSpellsApplyBaseCooldownOnInitialExecute()
    {
        ISpellInfo spellInfo = CreateRapidTapThresholdSpellInfo(66986u, 6000u);

        ISpellInfo cooldownSpellInfo = null;
        double cooldownSeconds = 0d;
        bool emitCooldown = false;

        ISpellManager spellManager = TestProxy.Create<ISpellManager>(
            ("SetSpellCooldown", (Action<ISpellInfo, double, bool>)((info, cooldown, emit) =>
            {
                cooldownSpellInfo = info;
                cooldownSeconds = cooldown;
                emitCooldown = emit;
            })));

        IPlayer caster = TestProxy.Create<IPlayer>(("get_SpellManager", spellManager));

        IServiceProvider previousProvider = LegacyServiceProvider.Provider;
        IScriptCollection scriptCollection = TestProxy.Create<IScriptCollection>();
        LegacyServiceProvider.Provider = new ServiceCollection()
            .AddSingleton<IScriptManager>(TestProxy.Create<IScriptManager>(
                ("InitialiseOwnedCollection", (Func<object, IScriptCollection>)(_ => scriptCollection)),
                ("InitialiseOwnedScripts", (Action<IScriptCollection, uint>)((_, _) => { }))))
            .BuildServiceProvider();

        try
        {
            var spell = new TestRapidTapThresholdSpell(
                TestProxy.Create<ISpellTargetInfoCollection>(("Initialise", (Action<ISpell>)(_ => { }))),
                TestProxy.Create<IGlobalSpellManager>(("get_NextCastingId", 1u)),
                TestProxy.Create<ICastResultValidatorManager>(("GetCastResult", (Func<ISpell, CastResult>)(_ => CastResult.Ok))),
                TestProxy.Create<IDisableManager>(),
                TestProxy.Create<ISpellFactory>());

            spell.Initialise(caster, new SpellParameters
            {
                SpellInfo = spellInfo,
                RootSpellInfo = spellInfo
            });

            spell.ExecuteForTest();
        }
        finally
        {
            LegacyServiceProvider.Provider = previousProvider;
        }

        Assert.Same(spellInfo, cooldownSpellInfo);
        Assert.Equal(6d, cooldownSeconds);
        Assert.True(emitCooldown);
    }

    [Fact]
    public void FacilityModificationNowHasDefaultApplyCoverage()
    {
        var manager = new GlobalSpellEffectManager();
        manager.Initialise();

        Assert.Equal(typeof(ISpellEffectApplyHandler<ISpellEffectDefaultData>), manager.GetSpellEffectApplyHandlerType(SpellEffectType.FacilityModification));
        Assert.Equal(typeof(ISpellEffectDefaultData), manager.GetSpellEffectDataType(SpellEffectType.FacilityModification));
        Assert.NotNull(manager.GetSpellEffectApplyDelegate(SpellEffectType.FacilityModification));
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
        SpellEffectType.ForceFacing,
        SpellEffectType.ForcedAction,
        SpellEffectType.ModifyAbilityCharges,
        SpellEffectType.ModifySpell,
        SpellEffectType.ModifySpellEffect,
        SpellEffectType.ProxyChannelVariableTime,
        SpellEffectType.RavelSignal,
        SpellEffectType.SapVital,
        SpellEffectType.ShieldOverload,
    ];

    private static readonly HashSet<SpellEffectType> ClassScoreOneKnownMissingApplyHandlers =
    [
        SpellEffectType.Absorption,
        SpellEffectType.ChangePhase,
        SpellEffectType.ChangePlane,
        SpellEffectType.DelayDeath,
        SpellEffectType.DisguiseOutfit,
        SpellEffectType.ForcedAction,
        SpellEffectType.MimicDisplayName,
        SpellEffectType.MimicDisguise,
        SpellEffectType.ModifyAbilityCharges,
        SpellEffectType.ModifySpell,
        SpellEffectType.ModifySpellEffect,
        SpellEffectType.PersonalDmgHealMod,
        SpellEffectType.RavelSignal,
        SpellEffectType.SharedHealthPool,
        SpellEffectType.ShieldOverload,
        SpellEffectType.SpellEffectImmunity,
        SpellEffectType.ThreatModification,
        SpellEffectType.ThreatTransfer,
        SpellEffectType.UnitPropertyConversion,
    ];

    public static IEnumerable<object[]> ClassScoreTwoSkillEffectTypes()
    {
        return ParseClassSkillEffectRows(ClassScoreTwoSkillEffectTypeData);
    }

    public static IEnumerable<object[]> ClassScoreOneSkillEffectTypes()
    {
        return ParseClassSkillEffectRows(ClassScoreOneSkillEffectTypeData);
    }

    private static IEnumerable<object[]> ParseClassSkillEffectRows(string data)
    {
        foreach (string row in data.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
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

    private const string ClassScoreOneSkillEffectTypeData = """
Warrior|Ripsaw|ForcedMove,CCStateSet,Damage,UnitPropertyModifier,Proc,Fluff,Proxy,ShieldOverload,SpellForceRemove
Warrior|Smackdown|Damage,UnitPropertyModifier,Fluff,Proxy,ModifySpellEffect,ModifySpellCooldown,SpellForceRemove,ModifyAbilityCharges
Warrior|Expulsion|Damage,Proxy,SpellDispel
Warrior|Sentinel|Damage,UnitPropertyModifier,Proc,Fluff,ForcedAction,Proxy,SpellForceRemove
Warrior|Unstoppable Force|UnitPropertyModifier,CCStateBreak,SpellEffectImmunity
Warrior|Stance: Onslaught|VitalModifier,UnitPropertyModifier,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove,ModifyAbilityCharges,HealShields
Engineer|Unstable Anomaly|Damage,UnitPropertyModifier,Fluff,Proxy,ShieldOverload
Engineer|Shock Pulse|CCStateSet,Damage,Proc,Proxy,ModifySpellEffect,ModifySpellCooldown
Engineer|Thresher|Transference,Damage,Fluff,Proxy,ModifySpell
Engineer|Code Red|CCStateSet,Damage,Proxy,ThreatTransfer
Engineer|Personal Defense Unit|Heal,UnitPropertyModifier,Fluff,Proxy,Absorption,SummonTrap
Engineer|Recursive Matrix|Damage,UnitPropertyModifier,Proc,Fluff,Proxy,Absorption
Engineer|Shatter Impairment|UnitPropertyModifier,CCStateBreak,Absorption,SpellDispel
Engineer|Urgent Withdrawal|ForcedMove,CCStateSet,Damage,Proxy,CCStateBreak,SpellEffectImmunity
Engineer|Mode: Eradicate|UnitPropertyModifier,Fluff,Proxy,UnitPropertyConversion
Engineer|Mode: Provoke|UnitPropertyModifier,Fluff,Proxy
Esper|Mind Burst|Damage,UnitPropertyModifier,Fluff,Proxy,ModifySpellEffect,SpellForceRemove
Esper|Psychic Frenzy|Transference,Damage,Proxy,SpellForceRemove
Esper|Spectral Swarm|Fluff,Proxy,FacilityModification,SummonPet
Esper|Bolster|VitalModifier,Heal,Proxy,Absorption
Esper|Mending Banner|Heal,UnitPropertyModifier,Proxy
Esper|Mental Boon|VitalModifier,Heal,UnitPropertyModifier,Proxy
Esper|Mirage|Heal,Fluff,Proxy,SummonTrap
Esper|Phantasmal Armor|VitalModifier,UnitPropertyModifier,Proxy,Absorption,DisguiseOutfit
Esper|Reverie|Heal,UnitPropertyModifier,Proxy
Esper|Warden|Heal,UnitPropertyModifier,Fluff,Proxy,Absorption
Esper|Catharsis|Heal,Proxy,SpellDispel
Esper|Fade Out|ForcedMove,CCStateSet,Heal,UnitPropertyModifier,ForcedAction,CCStateBreak,FacilityModification,SummonPet
Esper|Geist|Proxy,FacilityModification,SummonPet
Esper|Spectral Form|Proxy,Absorption
Medic|Dematerialize|VitalModifier,Damage,Proc,Proxy,SpellDispel,ShieldOverload
Medic|Barrier|UnitPropertyModifier,Proc,Fluff,Proxy,ModifySpell,SpellForceRemove,HealShields,SharedHealthPool
Medic|Dual Shock|Transference,Damage,Heal,Fluff,Proxy,SpellForceRemove
Medic|Mending Probes|Heal,Fluff,Proxy,ModifySpellCooldown,SpellForceRemove,DelayDeath
Medic|Rejuvenator|Heal,Fluff,Proxy,SummonTrap
Medic|Antidote|VitalModifier,Heal,Proxy,SpellDispel
Medic|Calm|CCStateSet,Heal,UnitPropertyModifier,Proxy,CCStateBreak,ThreatModification,HealShields
Medic|Energize|VitalModifier,UnitPropertyModifier,Proxy,HealShields
Stalker|Analyze Weakness|VitalModifier,Damage,Heal,Proc,Proxy,ModifySpellCooldown,SpellForceRemove,HealShields,PersonalDmgHealMod
Stalker|Clone|ForcedMove,Proxy,FacilityModification,SpellForceRemove,RavelSignal,Stealth,SummonPet,MimicDisplayName,MimicDisguise
Stalker|Phlebotomize|VitalModifier,Damage,UnitPropertyModifier,Proxy,ShieldOverload,SpellForceRemove
Stalker|Ruin|VitalModifier,Damage,UnitPropertyModifier,Fluff,Proxy,SpellForceRemove,PersonalDmgHealMod
Stalker|Amplification Spike|VitalModifier,UnitPropertyModifier,Fluff,Scale,Proxy
Stalker|Frenzy|Damage,UnitPropertyModifier,Fluff,Proxy,SpellForceRemove
Stalker|Nano Dart|Transference,ModifySpellCooldown,SpellForceRemove
Stalker|Nano Field|Transference,Damage,UnitPropertyModifier,Fluff,Proxy,SpellForceRemove
Stalker|Nano Virus|VitalModifier,Transference,UnitPropertyModifier,Proc,Fluff,Proxy
Stalker|Bloodthirst|Transference,Damage,UnitPropertyModifier,Proc,Fluff,Proxy,SpellForceRemove
Stalker|Stim Drone|Heal,UnitPropertyModifier,Proxy,CCStateBreak,SpellDispel
Stalker|Tactical Retreat|ForcedMove,Proxy,CCStateBreak,SpellDispel,SpellForceRemove,Stealth
Stalker|Tether Mine|CCStateSet,Fluff,Proxy,SummonTrap
Stalker|Nano Skin: Agile|VitalModifier,UnitPropertyModifier,Proc,Proxy,SpellForceRemove
Stalker|Nano Skin: Evasive|VitalModifier,UnitPropertyModifier,Proc,Proxy,SpellForceRemove
Stalker|Nano Skin: Lethal|VitalModifier,UnitPropertyModifier,Proc,ForcedAction,Proxy,SpellForceRemove
Spellslinger|Astral Infusion|Heal,UnitPropertyModifier,Fluff,Proxy,Absorption,DelayDeath
Spellslinger|Healing Torrent|Heal,Fluff,Proxy,Absorption,ModifySpell
Spellslinger|Runes of Protection|UnitPropertyModifier,Proxy,Absorption,ModifySpellCooldown
Spellslinger|Arcane Shock|CCStateSet,Damage,Fluff,SpellDispel,ModifySpellCooldown
Spellslinger|Purify|Heal,CCStateBreak,SpellDispel
Spellslinger|Spatial Shift|CCStateSet,Fluff,Proxy,SpellEffectImmunity,SpellForceRemove
Spellslinger|Void Slip|VitalModifier,Heal,Fluff,Proxy,CCStateBreak,ChangePhase,SpellDispel,SpellEffectImmunity,SpellForceRemove,ChangePlane
""";

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

    private static ISpellInfo CreateRapidTapThresholdSpellInfo(uint spell4Id, uint spellCooldownMs)
    {
        return TestProxy.Create<ISpellInfo>(
            ("get_Entry", new Spell4Entry
            {
                Id = spell4Id,
                SpellCoolDown = spellCooldownMs,
                ThresholdTime = 0u,
                CasterInnateRequirements = [0u, 0u],
                CasterInnateRequirementValues = [0u, 0u],
                CasterInnateRequirementEval = [0u, 0u],
                InnateCostTypes = [0u, 0u],
                InnateCosts = [0u, 0u],
                InnateCostEMMIds = [0u, 0u],
                PrerequisiteIdRunners = [0u, 0u],
                SpellCoolDownIds = [0u, 0u, 0u]
            }),
            ("get_Effects", new List<Spell4EffectsEntry>()),
            ("get_Thresholds", new List<Spell4ThresholdsEntry>
            {
                new()
                {
                    OrderIndex = 0
                }
            }));
    }

    private static IUnitEntity CreateUnit(uint guid, Vector3? position = null)
    {
        return TestProxy.Create<IUnitEntity>(
            ("get_Guid", guid),
            ("get_Position", position ?? new Vector3(guid, 0f, 0f)));
    }

    private static ISpell CreateDispelTargetSpell(uint spell4Id, SpellClass spellClass, Action finish)
    {
        ISpellBaseInfo baseInfo = TestProxy.Create<ISpellBaseInfo>(("get_Entry", new Spell4BaseEntry
        {
            SpellClass = spellClass
        }));
        ISpellInfo spellInfo = TestProxy.Create<ISpellInfo>(("get_BaseInfo", baseInfo));

        return TestProxy.Create<ISpell>(
            ("get_Spell4Id", spell4Id),
            ("get_Parameters", new SpellParameters
            {
                SpellInfo = spellInfo
            }),
            ("get_IsFinished", false),
            ("Finish", finish));
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

    private sealed class TestRapidTapThresholdSpell : SpellThreshold
    {
        public override CastMethod CastMethod => CastMethod.RapidTap;

        public TestRapidTapThresholdSpell(
            ISpellTargetInfoCollection spellTargetInfoCollection,
            IGlobalSpellManager globalSpellManager,
            ICastResultValidatorManager castResultValidatorManager,
            IDisableManager disableManager,
            ISpellFactory spellFactory)
            : base(
                NullLogger<TestRapidTapThresholdSpell>.Instance,
                spellTargetInfoCollection,
                globalSpellManager,
                castResultValidatorManager,
                disableManager,
                spellFactory)
        {
        }

        public void ExecuteForTest()
        {
            Execute();
        }
    }
}
