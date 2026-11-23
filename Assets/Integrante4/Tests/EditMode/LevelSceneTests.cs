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

        static Scene Open(string sceneName)
        {
            return EditorSceneManager.OpenScene(Folder + sceneName + ".unity", OpenSceneMode.Single);
        }
    }
}
