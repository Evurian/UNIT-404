namespace Unit404.Integrante4.Core
{
    // Pure logic for the full HUD (week 13): glyph progress, chip slots, anomaly bar.
    public static class HudExtras
    {
        public const float MaxAnomaly = 100f;

        public static string FormatGlyphs(int collected, int total)
        {
            int c = collected < 0 ? 0 : collected > total ? total : collected;
            return c + " / " + total;
        }

        public static float CooldownFraction(float remaining, float total)
        {
            if (total <= 0f) return 0f;
            float f = remaining / total;
            return f < 0f ? 0f : f > 1f ? 1f : f;
        }

        public static string ChipLabel(string name, int fuelCost)
            => string.IsNullOrEmpty(name) ? "Vacio" : name + " (" + fuelCost + ")";

        public static float AnomalyFraction(float value)
        {
            float f = value / MaxAnomaly;
            return f < 0f ? 0f : f > 1f ? 1f : f;
        }
    }
}
