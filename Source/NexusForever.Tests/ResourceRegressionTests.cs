using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Prerequisite.Check;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.PVP;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.GameTable.Model;

namespace NexusForever.Tests;

public class ResourceRegressionTests
{
    [Fact]
    public void Unknown64UsesDiscreteEsperPsiPoints()
    {
        var handler = new PrerequisiteCheckUnknown64(NullLogger<BasePrerequisiteHandler>.Instance);
        IPlayer player = TestProxy.Create<IPlayer>(
            ("get_Class", Class.Esper),
            ("get_Resource1", 2f),
            ("get_Resource4", 0f));

        Assert.True(handler.Meets(player, PrerequisiteComparison.GreaterThan, 1u, 1u, null));
        Assert.False(handler.Meets(player, PrerequisiteComparison.GreaterThan, 2u, 1u, null));
    }

    [Fact]
    public void Unknown71UsesNormalizedVitalPercent()
    {
        var handler = new PrerequisiteCheckUnknown71(NullLogger<BasePrerequisiteHandler>.Instance);
        IPlayer player = TestProxy.Create<IPlayer>(
            ("GetVitalValue", (Func<Vital, float>)(v => v == Vital.Resource1 ? 4f : 0f)),
            ("GetPropertyValue", (Func<Property, float>)(p => p == Property.ResourceMax1 ? 5f : 0f)));
        IPlayer lowPsiPlayer = TestProxy.Create<IPlayer>(
            ("GetVitalValue", (Func<Vital, float>)(v => v == Vital.Resource1 ? 2f : 0f)),
            ("GetPropertyValue", (Func<Property, float>)(p => p == Property.ResourceMax1 ? 5f : 0f)));

        Assert.True(handler.Meets(player, PrerequisiteComparison.GreaterThan, 60u, 6u, null));
        Assert.False(handler.Meets(lowPsiPlayer, PrerequisiteComparison.GreaterThan, 60u, 6u, null));
    }

    [Fact]
    public void Unknown50ChecksActiveSpellState()
    {
        var handler = new PrerequisiteCheckUnknown50(NullLogger<PrerequisiteCheckUnknown50>.Instance);
        ISpell activeSpell = TestProxy.Create<ISpell>(("get_Spell4Id", 39407u));
        IPlayer player = TestProxy.Create<IPlayer>(
            ("HasSpell", (HasSpellPredicateDelegate)((Func<ISpell, bool> predicate, out ISpell spell) =>
            {
                if (predicate(activeSpell))
                {
                    spell = activeSpell;
                    return true;
                }

                spell = null;
                return false;
            })));

        Assert.True(handler.Meets(player, PrerequisiteComparison.Equal, 39407u, 0u, null));
        Assert.False(handler.Meets(player, PrerequisiteComparison.NotEqual, 39407u, 0u, null));
        Assert.False(handler.Meets(player, PrerequisiteComparison.Equal, 111u, 0u, null));
        Assert.True(handler.Meets(player, PrerequisiteComparison.NotEqual, 111u, 0u, null));
    }

    [Fact]
    public void Unknown50CanCheckActiveSpellStateOnTargetUnit()
    {
        var handler = new PrerequisiteCheckUnknown50(NullLogger<PrerequisiteCheckUnknown50>.Instance);
        ISpell targetSpell = TestProxy.Create<ISpell>(("get_Spell4Id", 52309u));
        IPlayer caster = TestProxy.Create<IPlayer>(
            ("HasSpell", (HasSpellPredicateDelegate)((Func<ISpell, bool> predicate, out ISpell spell) =>
            {
                spell = null;
                return false;
            })));
        IUnitEntity target = TestProxy.Create<IUnitEntity>(
            ("HasSpell", (HasSpellPredicateDelegate)((Func<ISpell, bool> predicate, out ISpell spell) =>
            {
                if (predicate(targetSpell))
                {
                    spell = targetSpell;
                    return true;
                }

                spell = null;
                return false;
            })));
        var parameters = new PrerequisiteParameters
        {
            Target = target,
            EvaluateTarget = true
        };

        Assert.True(handler.Meets(caster, PrerequisiteComparison.Equal, 52309u, 0u, parameters));
        Assert.False(handler.Meets(caster, PrerequisiteComparison.NotEqual, 52309u, 0u, parameters));
        Assert.False(handler.Meets(caster, PrerequisiteComparison.Equal, 111u, 0u, parameters));
        Assert.True(handler.Meets(caster, PrerequisiteComparison.NotEqual, 111u, 0u, parameters));
    }

    [Fact]
    public void Spell130ChecksActiveSpellStateOnTargetUnit()
    {
        var handler = new PrerequisiteCheckSpell130(NullLogger<PrerequisiteCheckSpell130>.Instance);
        ISpell targetSpell = TestProxy.Create<ISpell>(("get_Spell4Id", 41471u));
        IPlayer caster = TestProxy.Create<IPlayer>(
            ("HasSpell", (HasSpellPredicateDelegate)((Func<ISpell, bool> predicate, out ISpell spell) =>
            {
                spell = null;
                return false;
            })));
        IUnitEntity target = TestProxy.Create<IUnitEntity>(
            ("HasSpell", (HasSpellPredicateDelegate)((Func<ISpell, bool> predicate, out ISpell spell) =>
            {
                if (predicate(targetSpell))
                {
                    spell = targetSpell;
                    return true;
                }

                spell = null;
                return false;
            })));
        var parameters = new PrerequisiteParameters
        {
            Target = target,
            EvaluateTarget = true
        };

        Assert.True(handler.Meets(caster, PrerequisiteComparison.Equal, 41471u, 437u, parameters));
        Assert.False(handler.Meets(caster, PrerequisiteComparison.NotEqual, 41471u, 437u, parameters));
        Assert.False(handler.Meets(caster, PrerequisiteComparison.Equal, 111u, 437u, parameters));
        Assert.True(handler.Meets(caster, PrerequisiteComparison.NotEqual, 111u, 437u, parameters));
    }

    [Fact]
    public void EntityKindPrerequisitesCanEvaluateTargetUnit()
    {
        var creatureHandler = new PrerequisiteCheckIsCreature(NullLogger<BasePrerequisiteHandler>.Instance);
        var playerHandler = new PrerequisiteCheckIsPlayer(NullLogger<BasePrerequisiteHandler>.Instance);
        IPlayer caster = TestProxy.Create<IPlayer>(("get_Type", EntityType.Player));
        IUnitEntity creatureTarget = TestProxy.Create<IUnitEntity>(
            ("get_Type", EntityType.NonPlayer),
            ("get_CreatureId", 12664u));
        IPlayer playerTarget = TestProxy.Create<IPlayer>(("get_Type", EntityType.Player));

        var creatureParameters = new PrerequisiteParameters
        {
            Target = creatureTarget,
            EvaluateTarget = true
        };
        var playerParameters = new PrerequisiteParameters
        {
            Target = playerTarget,
            EvaluateTarget = true
        };

        Assert.True(creatureHandler.Meets(caster, PrerequisiteComparison.Equal, 12664u, 0u, creatureParameters));
        Assert.False(creatureHandler.Meets(caster, PrerequisiteComparison.Equal, 12665u, 0u, creatureParameters));
        Assert.True(playerHandler.Meets(caster, PrerequisiteComparison.Equal, 0u, 0u, playerParameters));
        Assert.True(playerHandler.Meets(caster, PrerequisiteComparison.NotEqual, 0u, 0u, creatureParameters));
    }

    [Fact]
    public void VitalAndZonePrerequisitesCanEvaluateTargetUnit()
    {
        var healthHandler = new PrerequisiteCheckHealth(NullLogger<BasePrerequisiteHandler>.Instance);
        var healthRequirementHandler = new PrerequisiteCheckHealthRequirement(NullLogger<BasePrerequisiteHandler>.Instance);
        var shieldHandler = new PrerequisiteCheckShield(NullLogger<BasePrerequisiteHandler>.Instance);
        var zoneHandler = new PrerequisiteCheckInSubZone(NullLogger<BasePrerequisiteHandler>.Instance);
        IPlayer caster = TestProxy.Create<IPlayer>();
        IUnitEntity target = TestProxy.Create<IUnitEntity>(
            ("get_Health", 25u),
            ("get_MaxHealth", 100u),
            ("get_Shield", 75u),
            ("get_MaxShieldCapacity", 100u),
            ("get_Zone", new WorldZoneEntry { Id = 140u }));
        var parameters = new PrerequisiteParameters
        {
            Target = target,
            EvaluateTarget = true
        };

        Assert.True(healthHandler.Meets(caster, PrerequisiteComparison.LessThanOrEqual, 25u, 0u, parameters));
        Assert.False(healthHandler.Meets(caster, PrerequisiteComparison.GreaterThan, 50u, 0u, parameters));
        Assert.True(healthRequirementHandler.Meets(caster, PrerequisiteComparison.GreaterThanOrEqual, 25u, 0u, parameters));
        Assert.True(shieldHandler.Meets(caster, PrerequisiteComparison.GreaterThanOrEqual, 75u, 3u, parameters));
        Assert.False(shieldHandler.Meets(caster, PrerequisiteComparison.LessThan, 75u, 3u, parameters));
        Assert.True(zoneHandler.Meets(caster, PrerequisiteComparison.Equal, 140u, 0u, parameters));
        Assert.True(zoneHandler.Meets(caster, PrerequisiteComparison.NotEqual, 141u, 0u, parameters));
    }

    [Fact]
    public void PvpFlagPrerequisiteComparesFlaggedBooleanValue()
    {
        var handler = new PrerequisiteCheckPvpFlag(NullLogger<BasePrerequisiteHandler>.Instance);
        IPlayer caster = TestProxy.Create<IPlayer>();
        IPlayer flaggedTarget = TestProxy.Create<IPlayer>(("get_PvPFlags", PvPFlag.Enabled));
        IPlayer disabledTarget = TestProxy.Create<IPlayer>(("get_PvPFlags", PvPFlag.Disabled));

        Assert.True(handler.Meets(caster, PrerequisiteComparison.Equal, 1u, 0u, new PrerequisiteParameters
        {
            Target = flaggedTarget,
            EvaluateTarget = true
        }));
        Assert.False(handler.Meets(caster, PrerequisiteComparison.Equal, 0u, 0u, new PrerequisiteParameters
        {
            Target = flaggedTarget,
            EvaluateTarget = true
        }));
        Assert.True(handler.Meets(caster, PrerequisiteComparison.Equal, 0u, 0u, new PrerequisiteParameters
        {
            Target = disabledTarget,
            EvaluateTarget = true
        }));
    }

    [Fact]
    public void FocusClassesReceiveRuntimeFocusPropertyFallbacks()
    {
        List<(Property Property, float Value)> results = InvokeTupleSequence<Property, float>(
            "GetBasePropertyFallbacks",
            Class.Esper,
            new Func<Property, float>(_ => 0f));

        Assert.Contains(results, r => r.Property == Property.BaseFocusPool && r.Value == 100f);
        Assert.Contains(results, r => r.Property == Property.BaseFocusRecoveryInCombat && r.Value == 0.011f);
        Assert.Contains(results, r => r.Property == Property.BaseFocusRecoveryOutofCombat && r.Value == 0.011f);
    }

    [Fact]
    public void NonFocusClassesDoNotReceiveFocusPropertyFallbacks()
    {
        List<(Property Property, float Value)> results = InvokeTupleSequence<Property, float>(
            "GetBasePropertyFallbacks",
            Class.Warrior,
            new Func<Property, float>(_ => 0f));

        Assert.Empty(results);
    }

    [Fact]
    public void MedicReceivesMissingFocusAndCoreDefaults()
    {
        var propertyValues = new Dictionary<Property, float>
        {
            [Property.BaseFocusPool] = 100f,
            [Property.ResourceMax1] = 4f
        };

        List<(Stat Stat, float Value)> results = InvokeTupleSequence<Stat, float>(
            "GetMissingVitalDefaults",
            Class.Medic,
            new Func<Stat, float?>(_ => null),
            new Func<Property, float>(p => propertyValues.TryGetValue(p, out float value) ? value : 0f));

        Assert.Contains(results, r => r.Stat == Stat.Focus && r.Value == 100f);
        Assert.Contains(results, r => r.Stat == Stat.Resource1 && r.Value == 4f);
    }

    private static List<(T1 Item1, T2 Item2)> InvokeTupleSequence<T1, T2>(string methodName, params object[] args)
    {
        Type helperType = typeof(PrerequisiteCheckUnknown64).Assembly.GetType("NexusForever.Game.Entity.PlayerResourceDefaults", throwOnError: true);
        MethodInfo method = helperType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);

        var results = new List<(T1 Item1, T2 Item2)>();
        foreach (object entry in (IEnumerable)method.Invoke(null, args))
        {
            Type entryType = entry.GetType();
            object item1 = entryType.GetField("Item1").GetValue(entry);
            object item2 = entryType.GetField("Item2").GetValue(entry);
            results.Add(((T1)item1, (T2)item2));
        }

        return results;
    }

    private delegate bool HasSpellPredicateDelegate(Func<ISpell, bool> predicate, out ISpell spell);
}
