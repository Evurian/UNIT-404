using System;

namespace Unit404.Integrante4.Core
{
    // Per-culture palette from odd/art-style-sheet.md (matte base + one emissive accent).
    public static class CulturePalette
    {
        static readonly string[] Bases = { "#B08A57", "#7A6A4F", "#C9CFD6", "#7B4B3A" };
        static readonly string[] Accents = { "#FF9A2E", "#2EE6C8", "#3DA5FF", "#4BE38A" };

        public static string BaseHex(int level) => Bases[Check(level)];
        public static string AccentHex(int level) => Accents[Check(level)];

        static int Check(int level)
        {
            if (level < 1 || level > 4) throw new ArgumentOutOfRangeException(nameof(level), "level must be 1-4");
            return level - 1;
        }
    }
}
