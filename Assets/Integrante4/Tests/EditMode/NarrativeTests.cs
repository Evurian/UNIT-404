using System.Linq;
using NUnit.Framework;
using Unit404.Integrante4.Core;

namespace Unit404.Integrante4.Tests
{
    public class NarrativeTests
    {
        [Test]
        public void Achievements_FirstFlightOnlyOnLevelOne()
        {
            Assert.Contains("Primer vuelo", Achievements.Evaluate(1, 60, 0, 8, 0f).ToList());
            Assert.IsFalse(Achievements.Evaluate(2, 60, 0, 8, 0f).Contains("Primer vuelo"));
        }

        [Test]
        public void Achievements_PerfectScoreCollectorAndAdministrator()
        {
            var got = Achievements.Evaluate(2, 100, 8, 8, 10f).ToList();
            CollectionAssert.AreEquivalent(new[] { "Sin un rasguño", "Coleccionista", "Administrador" }, got);
        }

        [Test]
        public void Achievements_NoneWhenNothingEarned()
        {
            Assert.IsEmpty(Achievements.Evaluate(3, 80, 7, 8, 9.9f));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void AdaDialogue_HasStartCheckpointAndEndLinePerLevel(int level)
        {
            Assert.IsNotEmpty(AdaDialogue.Start(level));
            Assert.IsNotEmpty(AdaDialogue.Checkpoint(level));
            Assert.IsNotEmpty(AdaDialogue.End(level));
        }

        [Test]
        public void AdaDialogue_UnknownLevelFallsBackToGenericLine()
        {
            Assert.IsNotEmpty(AdaDialogue.Start(99));
        }

        [Test]
        public void LevelSummary_ShowsScoreGlyphsFuelAndAchievements()
        {
            string text = LevelSummary.Format(80, 6, 8, 12f, new[] { "Primer vuelo" });
            StringAssert.Contains("Puntaje: 80%", text);
            StringAssert.Contains("Glifos: 6/8", text);
            StringAssert.Contains("Combustible: 12", text);
            StringAssert.Contains("Primer vuelo", text);
        }

        [Test]
        public void StoryScripts_CarryTheFarewellPhrases()
        {
            CollectionAssert.Contains(StoryScripts.Epilogue, "El siguiente capítulo lo escribes tú");
            CollectionAssert.Contains(StoryScripts.Farewell, "Gracias por recordar");
            Assert.IsNotEmpty(StoryScripts.Intro);
            Assert.IsNotEmpty(StoryScripts.Credits);
        }
    }
}
