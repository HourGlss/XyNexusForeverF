using NexusForever.Game.Spell.Telemetry;

namespace NexusForever.Tests;

public class SpellDiagnosticsTests
{
    [Theory]
    [InlineData("stalker - pounce start", "stalker - pounce", SpellDiagnosticChatMarkerAction.Start)]
    [InlineData("Medic - Gamma Rays test start", "Medic - Gamma Rays", SpellDiagnosticChatMarkerAction.Start)]
    [InlineData("stalker - pounce test over", "stalker - pounce", SpellDiagnosticChatMarkerAction.End)]
    [InlineData("warrior - rampage done", "warrior - rampage", SpellDiagnosticChatMarkerAction.End)]
    public void ChatMarkerParserRecognisesSpellTestMarkers(string message, string label, SpellDiagnosticChatMarkerAction action)
    {
        Assert.True(SpellDiagnosticChatMarker.TryParse(message, out SpellDiagnosticChatMarker marker));
        Assert.Equal(label, marker.Label);
        Assert.Equal(action, marker.Action);
    }

    [Theory]
    [InlineData("")]
    [InlineData("start")]
    [InlineData("I hit the target and it seemed fine")]
    public void ChatMarkerParserIgnoresOrdinaryChat(string message)
    {
        Assert.False(SpellDiagnosticChatMarker.TryParse(message, out _));
    }
}
