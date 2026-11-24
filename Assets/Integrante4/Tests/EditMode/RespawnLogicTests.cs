using NUnit.Framework;
using Unit404.Integrante4.Core;

namespace Unit404.Integrante4.Tests
{
    // ODD S25 / T27: a fall is a death of GDD 5.6.
    public class RespawnLogicTests
    {
        static readonly float[] Checkpoints = { 78f, 124f };

        [TestCase(10f, -1)]
        [TestCase(78f, 0)]
        [TestCase(100f, 0)]
        [TestCase(124f, 1)]
        [TestCase(180f, 1)]
        public void LastCheckpoint_IsTheFurthestOnePassed(float furthestX, int expected)
        {
            Assert.AreEqual(expected, RespawnLogic.LastCheckpoint(Checkpoints, furthestX));
        }

        [Test]
        public void LastCheckpoint_IgnoresTheOrderOfTheArray()
        {
            Assert.AreEqual(1, RespawnLogic.LastCheckpoint(new[] { 124f, 78f }, 100f));
        }

        [Test]
        public void Route_WithoutCheckpoint_RespawnsAtPlayerStart()
        {
            Assert.AreEqual(RespawnPoint.PlayerStart, RespawnLogic.Choose(30f, 120f, 160f, false, -1));
        }

        [Test]
        public void Route_AfterACheckpoint_RespawnsAtTheLastCheckpoint()
        {
            Assert.AreEqual(RespawnPoint.LastCheckpoint, RespawnLogic.Choose(100f, 120f, 160f, false, 0));
        }

        [TestCase(120f)]
        [TestCase(150f)]
        [TestCase(160f)]
        public void Arena_WhileTheBossLives_RespawnsAtTheArenaDoor(float fallX)
        {
            Assert.AreEqual(RespawnPoint.ArenaDoor, RespawnLogic.Choose(fallX, 120f, 160f, false, 1));
        }

        [Test]
        public void Arena_AfterTheBossIsDefeated_RespawnsAtTheLastCheckpoint()
        {
            Assert.AreEqual(RespawnPoint.LastCheckpoint, RespawnLogic.Choose(150f, 120f, 160f, true, 1));
        }

        [TestCase(0f, 3f)]
        [TestCase(2.5f, 3f)]
        [TestCase(12f, 12f)]
        public void Respawn_KeepsAtLeastThreeFuel(float fuel, float expected)
        {
            Assert.AreEqual(expected, RespawnLogic.RespawnFuel(fuel));
        }

        [Test]
        public void Respawn_RestoresFiveLives()
        {
            Assert.AreEqual(5, RespawnLogic.Lives);
        }
    }
}
