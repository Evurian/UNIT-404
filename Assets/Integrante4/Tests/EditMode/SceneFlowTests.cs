using System.IO;
using System.Linq;
using NUnit.Framework;
using Unit404.Integrante4.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Unit404.Integrante4.Tests
{
    // ODD S31 / T33: my scenes are wired in one flow on integrante-4.
    public class SceneFlowTests
    {
        const string Folder = "Assets/Integrante4/Scenes/";

        [TestCase("Inicio", "Introduccion")]
        [TestCase("Introduccion", "Nivel1_Mesopotamia")]
        [TestCase("Nivel1_Mesopotamia", "Nivel2_Egipto")]
        [TestCase("Nivel2_Egipto", "Nivel3_Grecia")]
        [TestCase("Nivel3_Grecia", "Nivel4_China")]
        [TestCase("Nivel4_China", "Despedida")]
        [TestCase("Despedida", "Creditos")]
        [TestCase("Creditos", "Cierre")]
        [TestCase("Cierre", "Inicio")]
        public void SceneFlow_NextFollowsTheStory(string current, string expected)
        {
            Assert.AreEqual(expected, SceneFlow.Next(current));
        }

        [Test]
        public void SceneFlow_UnknownSceneGoesBackToStart()
        {
            Assert.AreEqual(SceneFlow.Start, SceneFlow.Next("SampleScene"));
        }

        [Test]
        public void BuildSettings_StartAtInicioAndHoldEveryFlowSceneInOrder()
        {
            string[] enabled = EditorBuildSettings.scenes.Where(s => s.enabled)
                .Select(s => Path.GetFileNameWithoutExtension(s.path)).ToArray();
            CollectionAssert.AreEqual(SceneFlow.BuildOrder, enabled);
            Assert.AreEqual("Inicio", enabled[0]);
            foreach (var s in EditorBuildSettings.scenes.Where(s => s.enabled))
                Assert.IsTrue(File.Exists(s.path), s.path + " missing");
        }

        [TestCase("Introduccion")]
        [TestCase("Despedida")]
        [TestCase("Creditos")]
        [TestCase("Cierre")]
        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Scene_NextSceneMatchesTheFlow(string sceneName)
        {
            EditorSceneManager.OpenScene(Folder + sceneName + ".unity");
            var owner = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .First(m => m.GetType().Name == "StoryScene" || m.GetType().Name == "LevelNarrator");
            Assert.AreEqual(SceneFlow.Next(sceneName), owner.GetType().GetField("nextScene").GetValue(owner));
        }
    }
}
