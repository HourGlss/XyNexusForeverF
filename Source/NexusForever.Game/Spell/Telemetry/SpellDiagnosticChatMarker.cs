namespace NexusForever.Game.Spell.Telemetry
{
    public enum SpellDiagnosticChatMarkerAction
    {
        Start,
        End
    }

    public class SpellDiagnosticChatMarker
    {
        private static readonly string[] StartSuffixes =
        [
            " test start",
            " start"
        ];

        private static readonly string[] EndSuffixes =
        [
            " test over",
            " test end",
            " over",
            " end",
            " done"
        ];

        public string Label { get; private set; }
        public SpellDiagnosticChatMarkerAction Action { get; private set; }

        public static bool TryParse(string message, out SpellDiagnosticChatMarker marker)
        {
            marker = null;

            if (string.IsNullOrWhiteSpace(message))
                return false;

            string trimmed = message.Trim();
            foreach (string suffix in StartSuffixes)
            {
                if (!trimmed.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    continue;

                string label = trimmed[..^suffix.Length].Trim();
                if (label.Length == 0)
                    return false;

                marker = new SpellDiagnosticChatMarker
                {
                    Label  = label,
                    Action = SpellDiagnosticChatMarkerAction.Start
                };
                return true;
            }

            foreach (string suffix in EndSuffixes)
            {
                if (!trimmed.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                    continue;

                string label = trimmed[..^suffix.Length].Trim();
                if (label.Length == 0)
                    return false;

                marker = new SpellDiagnosticChatMarker
                {
                    Label  = label,
                    Action = SpellDiagnosticChatMarkerAction.End
                };
                return true;
            }

            return false;
        }
    }
}
