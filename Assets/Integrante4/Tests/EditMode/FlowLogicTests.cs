using NUnit.Framework;
using Unit404.Integrante4.Core;

namespace Unit404.Integrante4.Tests
{
    public class FlowLogicTests
    {
        [TestCase(0f, 0f)]
        [TestCase(0.45f, 0.5f)]
        [TestCase(0.9f, 1f)]
        [TestCase(1f, 1f)]
        [TestCase(-1f, 0f)]
        public void LoadProgress_NormalizesAsyncProgress(float raw, float expected)
        {
            Assert.AreEqual(expected, LoadProgress.Normalize(raw), 0.0001f);
        }

        [Test]
        public void LoadProgress_FormatsPercent()
        {
            Assert.AreEqual("50%", LoadProgress.Format(0.45f));
            Assert.AreEqual("100%", LoadProgress.Format(0.9f));
        }

        [Test]
        public void LoadProgress_CanActivateOnlyWhenLoadedAndMinTimeElapsed()
        {
            Assert.IsFalse(LoadProgress.CanActivate(0.9f, 0.5f, 1f));
            Assert.IsFalse(LoadProgress.CanActivate(0.5f, 2f, 1f));
            Assert.IsTrue(LoadProgress.CanActivate(0.9f, 1f, 1f));
        }

        [Test]
        public void PauseState_TogglesAndReportsTimeScale()
        {
            var state = new PauseState();
            Assert.IsFalse(state.Paused);
            Assert.AreEqual(1f, state.TimeScale);
            state.Toggle();
            Assert.IsTrue(state.Paused);
            Assert.AreEqual(0f, state.TimeScale);
            state.Toggle();
            Assert.IsFalse(state.Paused);
        }

        [Test]
        // ODD S34 / T36: Esc must not open the pause menu while "Sistema caído" is showing.
        public void PauseState_EscapeIsIgnoredWhileBlocked()
        {
            var state = new PauseState();
            Assert.IsFalse(state.HandleEscape(blocked: true));
            Assert.IsFalse(state.Paused);
            Assert.IsTrue(state.HandleEscape(blocked: false));
            Assert.IsTrue(state.Paused);
            Assert.IsTrue(state.HandleEscape(blocked: false));
            Assert.IsFalse(state.Paused);
        }

        [Test]
        public void PauseState_SetPausedIsIdempotent()
        {
            var state = new PauseState();
            state.SetPaused(true);
            state.SetPaused(true);
            Assert.IsTrue(state.Paused);
            state.SetPaused(false);
            Assert.IsFalse(state.Paused);
        }
    }
}
