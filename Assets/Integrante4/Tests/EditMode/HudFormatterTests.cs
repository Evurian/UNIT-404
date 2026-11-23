using NUnit.Framework;
using Unit404.Integrante4.Core;

namespace Unit404.Integrante4.Tests
{
    public class HudFormatterTests
    {
        [TestCase(100, "100%")]
        [TestCase(80, "80%")]
        [TestCase(0, "0%")]
        [TestCase(-20, "0%")]
        [TestCase(140, "100%")]
        public void FormatScore_ShowsPercentClampedToZeroAndHundred(int score, string expected)
        {
            Assert.AreEqual(expected, HudFormatter.FormatScore(score));
        }

        [TestCase(10f, "10 / 20")]
        [TestCase(9.4f, "9 / 20")]
        [TestCase(0f, "0 / 20")]
        [TestCase(25f, "20 / 20")]
        [TestCase(-3f, "0 / 20")]
        public void FormatFuel_ShowsWholeUnitsOverMaxFuel(float fuel, string expected)
        {
            Assert.AreEqual(expected, HudFormatter.FormatFuel(fuel));
        }

        [TestCase(10f, 0.5f)]
        [TestCase(20f, 1f)]
        [TestCase(30f, 1f)]
        [TestCase(-1f, 0f)]
        public void FuelFraction_IsFuelOverMaxClampedToUnitRange(float fuel, float expected)
        {
            Assert.AreEqual(expected, HudFormatter.FuelFraction(fuel), 0.0001f);
        }

        [Test]
        public void IsCoreActive_LightsOneCorePerRemainingLife()
        {
            Assert.IsTrue(HudFormatter.IsCoreActive(3, 0));
            Assert.IsTrue(HudFormatter.IsCoreActive(3, 2));
            Assert.IsFalse(HudFormatter.IsCoreActive(3, 3));
            Assert.IsFalse(HudFormatter.IsCoreActive(0, 0));
            Assert.IsTrue(HudFormatter.IsCoreActive(5, 4));
        }
    }
}
