using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Abilities;
using NexusForever.GameTable.Model;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Abilities;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Handler.Spell;

namespace NexusForever.Tests;

public class AmpHandlerTests
{
    [Fact]
    public void CommitAmpSpecSynchronisesActionSetIds()
    {
        IActionSet actionSet = CreateActionSet([10, 20], specIndex: 0);
        int grantSpellsCalls = 0;
        int refreshAmpModifierCalls = 0;
        List<IWritable> messages = [];

        ISpellManager spellManager = TestProxy.Create<ISpellManager>(
            ("get_ActiveActionSet", (byte)0),
            ("GetActionSet", (Func<byte, IActionSet>)(_ => actionSet)),
            ("GrantSpells", (Action)(() => grantSpellsCalls++)),
            ("RefreshActiveAmpModifiers", (Action)(() => refreshAmpModifierCalls++)));
        IPlayer player = TestProxy.Create<IPlayer>(("get_SpellManager", spellManager));
        IWorldSession session = TestProxy.Create<IWorldSession>(
            ("get_Player", player),
            ("EnqueueMessageEncrypted", (Action<IWritable>)(message => messages.Add(message))));

        var packet = new ClientCommitAmpSpec();
        packet.Amps.Add(20);
        packet.Amps.Add(30);

        new ClientCommitAmpSpecHandler().HandleMessage(session, packet);

        ServerAmpList ampList = Assert.Single(messages.OfType<ServerAmpList>());
        Assert.Equal([20, 30], ampList.Amps);
        Assert.Equal(1, grantSpellsCalls);
        Assert.Equal(1, refreshAmpModifierCalls);
    }

    [Fact]
    public void RequestActionSetChangesClearsCacheAndSynchronisesAmps()
    {
        IActionSet actionSet = CreateActionSet([1, 2], specIndex: 1);
        int grantSpellsCalls = 0;
        int refreshAmpModifierCalls = 0;
        List<IWritable> messages = [];

        ISpellManager spellManager = TestProxy.Create<ISpellManager>(
            ("GetActionSet", (Func<byte, IActionSet>)(_ => actionSet)),
            ("GrantSpells", (Action)(() => grantSpellsCalls++)),
            ("RefreshActiveAmpModifiers", (Action)(() => refreshAmpModifierCalls++)));
        IPlayer player = TestProxy.Create<IPlayer>(("get_SpellManager", spellManager));
        IWorldSession session = TestProxy.Create<IWorldSession>(
            ("get_Player", player),
            ("EnqueueMessageEncrypted", (Action<IWritable>)(message => messages.Add(message))));

        var packet = new ClientRequestActionSetChanges();
        typeof(ClientRequestActionSetChanges)
            .GetProperty(nameof(ClientRequestActionSetChanges.ActionSetIndex))!
            .SetValue(packet, (byte)1);
        packet.Amps.Add(2);
        packet.Amps.Add(3);

        new ClientRequestActionSetChangesHandler().HandleMessage(session, packet);

        Assert.IsType<ServerActionSetClearCache>(messages[0]);
        Assert.IsType<ServerActionSet>(messages[1]);

        ServerAmpList ampList = Assert.Single(messages.OfType<ServerAmpList>());
        Assert.Equal([2, 3], ampList.Amps);
        Assert.Equal(1, grantSpellsCalls);
        Assert.Equal(1, refreshAmpModifierCalls);
    }

    private static IActionSet CreateActionSet(IEnumerable<ushort> enabledAmpIds, byte specIndex)
    {
        var enabled = enabledAmpIds
            .Distinct()
            .ToDictionary(id => id, CreateAmp);

        return TestProxy.Create<IActionSet>(
            ("get_Index", specIndex),
            ("get_Actions", Enumerable.Empty<IActionSetShortcut>()),
            ("get_Amps", (Func<IEnumerable<IActionSetAmp>>)(() => enabled.Values.OrderBy(amp => amp.Entry.Id).ToArray())),
            ("GetAmp", (Func<ushort, IActionSetAmp>)(id => enabled.GetValueOrDefault(id))),
            ("AddAmp", (Action<ushort>)(id => enabled[id] = CreateAmp(id))),
            ("SyncAmps", (Func<IEnumerable<ushort>, bool>)(amps => Sync(enabled, amps))),
            ("BuildServerActionSet", (Func<ServerActionSet>)(() => new ServerActionSet { SpecIndex = specIndex })),
            ("BuildServerAmpList", (Func<ServerAmpList>)(() => new ServerAmpList
            {
                SpecIndex = specIndex,
                Amps = enabled.Keys.OrderBy(id => id).ToList()
            })));
    }

    private static bool Sync(Dictionary<ushort, IActionSetAmp> enabled, IEnumerable<ushort> amps)
    {
        ushort[] desired = amps
            .Distinct()
            .OrderBy(id => id)
            .ToArray();
        ushort[] current = enabled.Keys
            .OrderBy(id => id)
            .ToArray();
        if (current.SequenceEqual(desired))
            return false;

        enabled.Clear();
        foreach (ushort id in desired)
            enabled[id] = CreateAmp(id);

        return true;
    }

    private static IActionSetAmp CreateAmp(ushort id) => TestProxy.Create<IActionSetAmp>(
        ("get_Entry", new EldanAugmentationEntry
        {
            Id = id,
            PowerCost = 1
        }));
}
