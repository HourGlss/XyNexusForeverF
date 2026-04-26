using NexusForever.WorldServer.Network.Message.Handler.Character;

namespace NexusForever.Tests;

public class CharacterSlotTests
{
    [Theory]
    [InlineData(null, 0, 6u)]
    [InlineData(2f, 0, 6u)]
    [InlineData(2f, 2, 4u)]
    [InlineData(6f, 6, 0u)]
    [InlineData(12f, 5, 7u)]
    public void RemainingCharacterSlotsUseSixSlotMinimum(float? rewardCharacterSlots, int characterCount, uint remainingSlots)
    {
        Assert.Equal(remainingSlots, CharacterListManager.CalculateRemainingCharacterSlots(rewardCharacterSlots, characterCount));
    }
}
