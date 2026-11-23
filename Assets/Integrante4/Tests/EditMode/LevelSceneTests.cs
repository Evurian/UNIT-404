using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unit404.Integrante4.Tests
{
    public class LevelSceneTests
    {
        const string Folder = "Assets/Integrante4/Scenes/";

        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        public void Scene_HasIntegrationContractObjects(string sceneName)
        {
            Scene scene = Open(sceneName);
            Assert.IsNotNull(GameObject.Find("PlayerStart"), "PlayerStart missing");
            Assert.IsNotNull(Camera.main, "Main Camera missing");
        }

        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        public void Scene_HasEightGlyphsTwoCheckpointsAndOnePortal(string sceneName)
        {
            Open(sceneName);
            Assert.AreEqual(8, GameObject.FindGameObjectsWithTag("Glifo").Length);
            Assert.AreEqual(2, GameObject.FindGameObjectsWithTag("Checkpoint").Length);
            Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Plataforma").Count(g => g.name == "PortalLlegada"));
        }

        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        public void Scene_GameplayObjectsLieOnZeroPlane(string sceneName)
        {
            Open(sceneName);
            string[] tags = { "Fragmento", "Propulsor", "Obstaculo", "Contaminacion", "Observador", "Jefe", "Glifo", "Checkpoint", "Plataforma" };
            foreach (string tag in tags)
                foreach (GameObject go in GameObject.FindGameObjectsWithTag(tag))
                    Assert.AreEqual(0f, go.transform.position.z, 0.0001f, go.name + " (" + tag + ") is off Z = 0");
            Assert.AreEqual(0f, GameObject.Find("PlayerStart").transform.position.z, 0.0001f);
        }

        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        public void Scene_HasCollectiblesRingsAndGuards(string sceneName)
        {
            Open(sceneName);
            Assert.GreaterOrEqual(GameObject.FindGameObjectsWithTag("Fragmento").Length, 10);
            Assert.GreaterOrEqual(GameObject.FindGameObjectsWithTag("Propulsor").Length, 4);
            Assert.GreaterOrEqual(GameObject.FindGameObjectsWithTag("Observador").Length, 2);
            Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Jefe").Length);
        }

        [TestCase("Nivel3_Grecia", "Inscripcion")]
        [TestCase("Nivel4_China", "TiraBambu")]
        public void Level3And4_HaveContractObjectsGlyphsCheckpointsPortalAndBoss(string sceneName, string glyphPrefix)
        {
            Open(sceneName);
            Assert.IsNotNull(GameObject.Find("PlayerStart"), "PlayerStart missing");
            Assert.IsNotNull(Camera.main, "Main Camera missing");
            var glyphs = GameObject.FindGameObjectsWithTag("Glifo");
            Assert.AreEqual(8, glyphs.Length);
            Assert.IsTrue(glyphs.All(g => g.name.StartsWith(glyphPrefix)));
            Assert.AreEqual(2, GameObject.FindGameObjectsWithTag("Checkpoint").Length);
            Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Plataforma").Count(g => g.name == "PortalLlegada"));
            Assert.AreEqual(1, GameObject.FindGameObjectsWithTag("Jefe").Length);
        }

        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Level3And4_HaveMovingObstaclesAndGuardsAndAllowDepth(string sceneName)
        {
            Open(sceneName);
            Assert.GreaterOrEqual(CountComponents("MovingObstacle"), 3, "moving obstacles");
            Assert.GreaterOrEqual(GameObject.FindGameObjectsWithTag("Observador").Length, 2);
            Assert.GreaterOrEqual(GameObject.FindGameObjectsWithTag("Fragmento").Length, 10);
            var placeholder = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).First(m => m.GetType().Name == "PlaceholderPlayer");
            Assert.IsTrue((bool)placeholder.GetType().GetField("allowDepth").GetValue(placeholder), "3D flight must allow depth");
        }

        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        public void Level1And2_HaveNoMovingObstacles(string sceneName)
        {
            Open(sceneName);
            Assert.AreEqual(0, CountComponents("MovingObstacle"));
        }

        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void AllLevels_AreComplete_ObstaclesFragmentsRingsInEveryZone(string sceneName)
        {
            Open(sceneName);
            Assert.GreaterOrEqual(GameObject.FindGameObjectsWithTag("Obstaculo").Length, 2, "obstacles");
            Assert.GreaterOrEqual(GameObject.FindGameObjectsWithTag("Propulsor").Length, 6, "rings");
            float[] edges = { 0f, 40f, 80f, 120f, 160f, 200f };
            var fragments = GameObject.FindGameObjectsWithTag("Fragmento");
            for (int zone = 0; zone < 5; zone++)
            {
                int inZone = fragments.Count(f => f.transform.position.x >= edges[zone] && f.transform.position.x < edges[zone + 1]);
                Assert.GreaterOrEqual(inZone, 2, "fragments in zone " + (zone + 1));
            }
        }

        static int CountComponents(string typeName)
        {
            return Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Count(m => m != null && m.GetType().Name == typeName);
        }

        static Scene Open(string sceneName)
        {
            return EditorSceneManager.OpenScene(Folder + sceneName + ".unity", OpenSceneMode.Single);
        }
    }
}
