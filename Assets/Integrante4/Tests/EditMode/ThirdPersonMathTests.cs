using NUnit.Framework;
using Unit404.Integrante4.Core;

namespace Unit404.Integrante4.Tests
{
    public class ThirdPersonMathTests
    {
        [Test]
        public void MoveDirection_ForwardInputWithYawZero_MovesAlongPlusZ()
        {
            var d = ThirdPersonMath.MoveDirection(0f, 1f, 0f);
            Assert.AreEqual(0f, d.x, 1e-4f);
            Assert.AreEqual(1f, d.z, 1e-4f);
        }

        [Test]
        public void MoveDirection_ForwardInputWithYaw90_MovesAlongPlusX()
        {
            var d = ThirdPersonMath.MoveDirection(0f, 1f, 90f);
            Assert.AreEqual(1f, d.x, 1e-4f);
            Assert.AreEqual(0f, d.z, 1e-4f);
        }

        [Test]
        public void MoveDirection_DiagonalInput_IsNormalized()
        {
            var d = ThirdPersonMath.MoveDirection(1f, 1f, 0f);
            Assert.AreEqual(1f, (float)System.Math.Sqrt(d.x * d.x + d.z * d.z), 1e-4f);
        }

        [Test]
        public void CameraOffset_YawZero_SitsBehindAndAbove()
        {
            var o = ThirdPersonMath.CameraOffset(0f, 0f, 10f);
            Assert.AreEqual(0f, o.x, 1e-4f);
            Assert.AreEqual(-10f, o.z, 1e-4f);
        }

        [Test]
        public void ClampPitch_StaysInsideLimits()
        {
            Assert.AreEqual(ThirdPersonMath.MaxPitch, ThirdPersonMath.ClampPitch(200f));
            Assert.AreEqual(ThirdPersonMath.MinPitch, ThirdPersonMath.ClampPitch(-200f));
        }
    }
}
