using System;

namespace Unit404.Integrante4.Core
{
    // Per-culture palette from odd/art-style-sheet.md (matte base + one emissive accent).
    public static class CulturePalette
    {
        static readonly string[] Bases = { "#B08A57", "#7A6A4F", "#C9CFD6", "#7B4B3A" };
        static readonly string[] Accents = { "#FF9A2E", "#2EE6C8", "#3DA5FF", "#4BE38A" };
        // Dark shade of each culture for the "Sistema caído" screen (ODD S36): burnt clay, Nile night, Aegean deep, jade shadow.
        static readonly string[] Falls = { "#2B1A0E", "#0D2624", "#0F1D30", "#0E2416" };
        static readonly string[] Names = { "Mesopotamia", "Egipto", "Grecia", "China" };

        public static string BaseHex(int level) => Bases[Check(level)];
        public static string AccentHex(int level) => Accents[Check(level)];
        public static string FallHex(int level) => Falls[Check(level)];
        public static string CultureName(int level) => Names[Check(level)];

        // Colour of the full-screen fade after finishing `level`: accent of the next culture, parchment ink after the last.
        public static string TransitionHex(int level)
        {
            Check(level);
            return level < 4 ? Accents[level] : "#291C12";
        }

        static int Check(int level)
        {
            if (level < 1 || level > 4) throw new ArgumentOutOfRangeException(nameof(level), "level must be 1-4");
            return level - 1;
        }
    }
}
