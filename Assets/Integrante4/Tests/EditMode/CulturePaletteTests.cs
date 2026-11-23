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

        [Test]
        public void OutOfRangeLevel_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => CulturePalette.BaseHex(5));
        }
    }
}
