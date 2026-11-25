using System;
using NUnit.Framework;
using Unit404.Integrante4.Core;

namespace Unit404.Integrante4.Tests
{
    public class CulturePaletteTests
    {
        [TestCase(1, "#B08A57", "#FF9A2E")]
        [TestCase(2, "#7A6A4F", "#2EE6C8")]
        [TestCase(3, "#C9CFD6", "#3DA5FF")]
        [TestCase(4, "#7B4B3A", "#4BE38A")]
        public void Level_MapsToCultureBaseAndAccent(int level, string baseHex, string accentHex)
        {
            Assert.AreEqual(baseHex, CulturePalette.BaseHex(level));
            Assert.AreEqual(accentHex, CulturePalette.AccentHex(level));
        }

        // ODD S36 / T36: the fall screen is tinted with a dark shade of each culture, not one brown for all.
        [TestCase(1, "#2B1A0E")]
        [TestCase(2, "#0D2624")]
        [TestCase(3, "#0F1D30")]
        [TestCase(4, "#0E2416")]
        public void Level_MapsToCultureFallTint(int level, string fallHex)
        {
            Assert.AreEqual(fallHex, CulturePalette.FallHex(level));
        }

        [TestCase(1, "Mesopotamia")]
        [TestCase(2, "Egipto")]
        [TestCase(3, "Grecia")]
        [TestCase(4, "China")]
        public void Level_MapsToCultureName(int level, string name)
        {
            Assert.AreEqual(name, CulturePalette.CultureName(level));
        }

        [Test]
        public void OutOfRangeLevel_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CulturePalette.BaseHex(5));
        }
    }
}
