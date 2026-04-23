using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Chat.Format;
using NexusForever.Game.Chat.Format;
using NexusForever.Game.Static.Chat;
using NexusForever.Network;
using NexusForever.Network.Internal.Message.Chat.Shared.Format;
using NexusForever.Network.Internal.Message.Chat.Shared.Format.Model;
using NexusForever.Network.World.Chat;
using NexusForever.Network.World.Chat.Model;
using NexusForever.Network.World.Message.Model.Chat;

namespace NexusForever.Tests;

public class ChatFormatTests
{
    [Fact]
    public void NetworkWorldRegistersEveryClientChatFormatModel()
    {
        var services = new ServiceCollection();
        services.AddNetworkWorldChat();

        using ServiceProvider provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IChatFormatModelFactory>();

        foreach (ChatFormatType type in Enum.GetValues<ChatFormatType>())
            Assert.NotNull(factory.NewChatFormatModel(type));
    }

    [Fact]
    public void ClientChatFormatsRoundTripThroughPacketModels()
    {
        var services = new ServiceCollection();
        services.AddNetworkWorldChat();

        using ServiceProvider provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IChatFormatModelFactory>();

        foreach (ChatFormat format in CreateNetworkFormats())
        {
            byte[] data;
            using (var stream = new MemoryStream())
            {
                var writer = new GamePacketWriter(stream);
                format.Write(writer);
                writer.FlushBits();
                data = stream.ToArray();
            }

            using var reader = new GamePacketReader(new MemoryStream(data));
            var clientFormat = new ChatClientFormat(factory);
            clientFormat.Read(reader);

            Assert.Equal(format.Type, clientFormat.Type);
            Assert.Equal(format.StartIndex, clientFormat.StartIndex);
            Assert.Equal(format.StopIndex, clientFormat.StopIndex);
            AssertEquivalentModel(format.Model, clientFormat.Model);
        }
    }

    [Fact]
    public void GameChatFormatManagerConvertsAllRegisteredNonLocalFormats()
    {
        var services = new ServiceCollection();
        services.AddGameChatFormat();

        using ServiceProvider provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IChatFormatManager>();
        manager.Initialise();

        ChatFormat[] networkFormats = CreateNetworkFormats()
            .Where(f => f.Type != ChatFormatType.ItemGuid)
            .ToArray();

        List<ChatChannelTextFormat> internalFormats = manager.ToInternal(null, networkFormats).ToList();
        Assert.Equal(networkFormats.Length, internalFormats.Count);
        Assert.Equal(networkFormats.Select(f => f.Type), internalFormats.Select(f => f.Type));

        List<ChatFormat> roundTripFormats = manager.ToNetwork(internalFormats).ToList();
        Assert.Equal(networkFormats.Length, roundTripFormats.Count);
        Assert.Equal(networkFormats.Select(f => f.Type), roundTripFormats.Select(f => f.Type));
    }

    private static IEnumerable<ChatFormat> CreateNetworkFormats()
    {
        yield return Format(ChatFormatType.Format0, new ChatFormat0 { Unknown = true });
        yield return Format(ChatFormatType.Alien, new ChatFormatAlien { RandomTextSeed = 1001u });
        yield return Format(ChatFormatType.Roleplay, new ChatFormatRoleplay { Unknown = true });
        yield return Format(ChatFormatType.Format3, new ChatFormat3 { Unknown = false });
        yield return Format(ChatFormatType.ItemId, new ChatFormatItemId { Item2Id = 17u });
        yield return Format(ChatFormatType.QuestId, new ChatFormatQuestId { Quest2Id = 19 });
        yield return Format(ChatFormatType.ArchiveArticle, new ChatFormatArchiveArticle { ArchiveArticleId = 23 });
        yield return Format(ChatFormatType.Profanity, new ChatFormatProfanity { RandomTextSeed = 29u });
        yield return Format(ChatFormatType.ItemFull, new ChatFormatItemFull
        {
            ItemGuid = 31ul,
            Item2Id = 37u,
            MakerCharacterId = 41ul,
            CircuitData = 43ul,
            RuneData = 47u,
            ThresholdData = 53ul,
            Unknown1 = 59u,
            WorldRequirements = 61u,
            Unknown2 = 2,
            WorldRequirementItem2Id = 67u,
            ChargeAmounts = [1.25f, 2.5f],
            RuneSlotItem2Ids = [71u, 73u]
        });
        yield return Format(ChatFormatType.ItemGuid, new ChatFormatItemGuid { ItemGuid = 79ul });
        yield return Format(ChatFormatType.NavPoint, new ChatFormatNavPoint { MapZoneId = 83, X = 89.5f, Y = 97.25f });
        yield return Format(ChatFormatType.Loot, new ChatFormatLoot { LootUnitId = 101u });
    }

    private static ChatFormat Format(ChatFormatType type, IChatFormatModel model)
    {
        return new ChatFormat
        {
            Type = type,
            StartIndex = (ushort)((int)type + 1),
            StopIndex = (ushort)((int)type + 2),
            Model = model
        };
    }

    private static void AssertEquivalentModel(IChatFormatModel expected, IChatFormatModel actual)
    {
        Assert.Equal(expected.GetType(), actual.GetType());

        foreach (var property in expected.GetType().GetProperties().Where(p => p.Name != nameof(IChatFormatModel.Type)))
        {
            object expectedValue = property.GetValue(expected);
            object actualValue = property.GetValue(actual);

            if (expectedValue is System.Collections.IEnumerable expectedValues && expectedValue is not string)
            {
                Assert.Equal(expectedValues.Cast<object>(), ((System.Collections.IEnumerable)actualValue).Cast<object>());
                continue;
            }

            Assert.Equal(expectedValue, actualValue);
        }
    }
}
