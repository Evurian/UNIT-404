using System.Linq;
using NUnit.Framework;
using Unit404.Integrante4.Core;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Unit404.Integrante4.Tests
{
    // ODD S27 / T34: in-engine culture transition between levels.
    public class CinematicTests
    {
        [TestCase(2, "Transicion2_Egipto", "Nivel 2 — Egipto")]
        [TestCase(3, "Transicion3_Grecia", "Nivel 3 — Grecia")]
        [TestCase(4, "Transicion4_China", "Nivel 4 — China")]
        public void CinematicScript_NamesTitleAndSubtitles(int level, string scene, string title)
        {
            Assert.AreEqual(scene, CinematicScript.SceneName(level));
            Assert.AreEqual(title, CinematicScript.Title(level));
            Assert.GreaterOrEqual(CinematicScript.Subtitles(level).Length, 3);
            Assert.IsTrue(CinematicScript.Subtitles(level).All(l => l.StartsWith("Ada: ")));
        }

        [Test]
        public void CinematicScript_LastsFifteenToTwentySeconds()
        {
            Assert.GreaterOrEqual(CinematicScript.Duration, 15f);
            Assert.LessOrEqual(CinematicScript.Duration, 20f);
        }

        [Test]
        public void CinematicScript_TitleFadesInHoldsAndFadesOut()
        {
            Assert.AreEqual(0f, CinematicScript.TitleAlpha(0f), 0.001f);
            Assert.AreEqual(1f, CinematicScript.TitleAlpha(3f), 0.001f);
            Assert.AreEqual(0f, CinematicScript.TitleAlpha(8f), 0.001f);
        }

        [Test]
        public void CinematicScript_FadesFromAndToBlack()
        {
            Assert.AreEqual(1f, CinematicScript.FadeAlpha(0f), 0.001f);
            Assert.AreEqual(0f, CinematicScript.FadeAlpha(CinematicScript.Duration / 2f), 0.001f);
            Assert.AreEqual(1f, CinematicScript.FadeAlpha(CinematicScript.Duration), 0.001f);
        }

        [Test]
        public void CinematicScript_SubtitlesAdvanceInOrderAndCoverTheShot()
        {
            int lines = CinematicScript.Subtitles(2).Length;
            Assert.AreEqual(-1, CinematicScript.SubtitleIndex(2, 0f));
            Assert.AreEqual(0, CinematicScript.SubtitleIndex(2, 3f));
            Assert.AreEqual(lines - 1, CinematicScript.SubtitleIndex(2, CinematicScript.Duration - 1.5f));
            int last = -1;
            for (float t = 0f; t <= CinematicScript.Duration; t += 0.25f)
            {
                int i = CinematicScript.SubtitleIndex(2, t);
                Assert.GreaterOrEqual(i, last);
                last = i;
            }
        }

        [Test]
        public void CinematicPath_PassesThroughEveryKeyAndClamps()
        {
            float[] keys = { 0f, 10f, 4f, 20f };
            for (int k = 0; k < keys.Length; k++)
                Assert.AreEqual(keys[k], CinematicPath.CatmullRom(keys, k / 3f), 0.0001f);
            Assert.AreEqual(0f, CinematicPath.CatmullRom(keys, -1f), 0.0001f);
            Assert.AreEqual(20f, CinematicPath.CatmullRom(keys, 2f), 0.0001f);
            Assert.AreEqual(0.5f, CinematicPath.Ease(0.5f), 0.0001f);
            Assert.AreEqual(0f, CinematicPath.Ease(0f), 0.0001f);
            Assert.AreEqual(1f, CinematicPath.Ease(1f), 0.0001f);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void CinematicScene_FliesOverTheNextCultureWithoutGameplay(int level)
        {
            string name = CinematicScript.SceneName(level);
            EditorSceneManager.OpenScene("Assets/Integrante4/Scenes/" + name + ".unity");
            var director = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(m => m.GetType().Name == "CinematicDirector");
            System.Type type = director.GetType();
            Assert.AreEqual(level, type.GetField("level").GetValue(director));
            Assert.AreEqual(SceneFlow.Next(name), type.GetField("nextScene").GetValue(director));
            Assert.GreaterOrEqual(((Transform[])type.GetField("path").GetValue(director)).Length, 4, "camera path");
            Assert.AreEqual(((Transform[])type.GetField("path").GetValue(director)).Length,
                ((Transform[])type.GetField("lookAt").GetValue(director)).Length, "one look target per key");
            foreach (string field in new[] { "title", "subtitle", "fade" })
                Assert.IsNotNull(type.GetField(field).GetValue(director), field);
            Assert.IsNotNull(Camera.main, "camera");
            Assert.IsNotNull(GameObject.Find("Terreno"), "culture ground");
            Assert.Greater(GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(t => t.name.StartsWith("Paisaje_")), 3, "backdrops");
            Assert.Greater(GameObject.FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(t => t.name.StartsWith("Kit_") || t.GetComponent<MeshRenderer>() != null), 20, "level scenery");
            foreach (string tag in new[] { "Player", "Fragmento", "Propulsor", "Glifo", "Checkpoint", "Observador", "Jefe" })
                Assert.AreEqual(0, GameObject.FindGameObjectsWithTag(tag).Length, tag + " left in the cinematic");
            foreach (string kind in new[] { "FallScreen", "PauseMenu", "LevelNarrator", "PlaceholderPlayer" })
                Assert.AreEqual(0, Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Count(m => m.GetType().Name == kind), kind);
        }
    }
}
