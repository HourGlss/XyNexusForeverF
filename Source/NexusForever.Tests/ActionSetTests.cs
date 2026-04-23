using System.Reflection;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Spell;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Tests;

public class ActionSetTests
{
    [Fact]
    public void ShortcutAddRemoveMaintainsSlotInvariants()
    {
        var actionSet = new ActionSet(0, TestProxy.Create<IPlayer>(("get_CharacterId", 1234ul)));

        actionSet.AddShortcut((UILocation)0, ShortcutType.SpellbookItem, 42u, 2);

        Assert.Equal(42u, actionSet.GetShortcut((UILocation)0).ObjectId);
        Assert.Throws<InvalidOperationException>(() =>
            actionSet.AddShortcut((UILocation)0, ShortcutType.SpellbookItem, 43u, 2));

        actionSet.RemoveShortcut((UILocation)0);

        Assert.Null(actionSet.GetShortcut((UILocation)0));
    }

    [Fact]
    public void ShortcutTierCannotExceedClientActionSetTierLimit()
    {
        var actionSet = new ActionSet(0, TestProxy.Create<IPlayer>(("get_CharacterId", 1234ul)));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            actionSet.AddShortcut((UILocation)0, ShortcutType.SpellbookItem, 42u, (byte)(ActionSet.MaxTier + 1)));
    }
}

internal class TestProxy : DispatchProxy
{
    private Dictionary<string, object> values;

    public static T Create<T>(params (string Name, object Value)[] values) where T : class
    {
        object proxy = Create<T, TestProxy>();
        ((TestProxy)proxy).values = values.ToDictionary(v => v.Name, v => v.Value);
        return (T)proxy;
    }

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        if (targetMethod != null && values.TryGetValue(targetMethod.Name, out object value))
        {
            if (value is Delegate handler)
                return handler.DynamicInvoke(args);

            return value;
        }

        Type returnType = targetMethod?.ReturnType ?? typeof(void);
        if (returnType == typeof(void))
            return null;

        return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
    }
}
