using NUnit.Framework;
using Unit404.Integrante4.Core;

namespace Unit404.Integrante4.Tests
{
    public class UiAnimationTests
    {
        [TestCase(0f, 1f, 0f)]
        [TestCase(1f, 1f, 1f)]
        [TestCase(0.5f, 1f, 0.5f)]
        [TestCase(-1f, 1f, 0f)]
        [TestCase(5f, 1f, 1f)]
        public void FadeAlpha_IsSmoothAndClamped(float t, float duration, float expected)
            => Assert.AreEqual(expected, UiAnimation.FadeAlpha(t, duration), 1e-4f);

        [Test]
        public void FadeAlpha_ZeroDuration_IsFullyOpaque()
            => Assert.AreEqual(1f, UiAnimation.FadeAlpha(0f, 0f));

        [TestCase(0f, 1f)]
        [TestCase(0.5f, 1.4f)]
        [TestCase(1f, 1f)]
        [TestCase(2f, 1f)]
        public void ChipPulse_PeaksMidwayAndReturnsToOne(float t, float expected)
            => Assert.AreEqual(expected, UiAnimation.ChipPulse(t, 1f, 1.4f), 1e-4f);

        [TestCase(1, "#2EE6C8")]
        [TestCase(2, "#3DA5FF")]
        [TestCase(3, "#4BE38A")]
        [TestCase(4, "#291C12")]
        public void TransitionHex_IsNextCultureAccentOrInkAfterLastLevel(int level, string expected)
            => Assert.AreEqual(expected, CulturePalette.TransitionHex(level));
    }
}
