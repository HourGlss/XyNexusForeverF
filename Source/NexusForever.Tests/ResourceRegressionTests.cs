using System.Collections;
using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Prerequisite.Check;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;

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
}
