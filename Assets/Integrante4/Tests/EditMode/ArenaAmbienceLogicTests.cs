using NUnit.Framework;
using Unit404.Integrante4.Core;

namespace Unit404.Integrante4.Tests
{
    public class ArenaAmbienceLogicTests
    {
        [TestCase(100f, AmbienceMode.Level)]
        [TestCase(120f, AmbienceMode.Boss)]
        [TestCase(140f, AmbienceMode.Boss)]
        [TestCase(160f, AmbienceMode.Boss)]
        [TestCase(170f, AmbienceMode.Level)]
        public void Mode_DependsOnArenaBounds_WhileBossAlive(float x, AmbienceMode expected)
        {
            Assert.AreEqual(expected, ArenaAmbienceLogic.Mode(x, 120f, 160f, false));
        }

        [Test]
        public void Mode_ReturnsToLevelAmbience_AfterBossDefeated()
        {
            Assert.AreEqual(AmbienceMode.Level, ArenaAmbienceLogic.Mode(140f, 120f, 160f, true));
        }
    }
}
