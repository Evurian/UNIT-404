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

        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Level3And4_UseThirdPersonCameraAndCameraRelativePlayer(string sceneName)
        {
            Open(sceneName);
            Assert.AreEqual(1, CountComponents("ThirdPersonCamera"));
            Assert.AreEqual(0, CountComponents("SideCameraFollow"));
            var placeholder = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).First(m => m.GetType().Name == "PlaceholderPlayer");
            Assert.IsTrue((bool)placeholder.GetType().GetField("cameraRelative").GetValue(placeholder));
        }

        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Level3And4_HaveSidePathsWithFragments(string sceneName)
        {
            Open(sceneName);
            var paths = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name.StartsWith("SenderoLateral_")).ToArray();
            Assert.GreaterOrEqual(paths.Length, 2, "side paths");
            Assert.IsTrue(paths.All(t => Mathf.Abs(t.position.z) >= 6f), "side paths must leave the main line");
            Assert.GreaterOrEqual(GameObject.FindGameObjectsWithTag("Fragmento").Count(f => Mathf.Abs(f.transform.position.z) >= 6f), 2);
        }

        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Level_HasCultureTransitionWiredToNarrator(string sceneName)
        {
            Open(sceneName);
            Assert.AreEqual(1, CountComponents("CultureTransition"));
            var narrator = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).First(m => m.GetType().Name == "LevelNarrator");
            Assert.IsNotNull(narrator.GetType().GetField("transition").GetValue(narrator), "narrator.transition");
        }

        [TestCase("Nivel1_Mesopotamia", 1)]
        [TestCase("Nivel2_Egipto", 2)]
        [TestCase("Nivel3_Grecia", 3)]
        [TestCase("Nivel4_China", 4)]
        public void Level_UsesCulturePalette_AccentOnPickupsAndRings(string sceneName, int level)
        {
            Open(sceneName);
            ColorUtility.TryParseHtmlString(Unit404.Integrante4.Core.CulturePalette.AccentHex(level), out Color accent);
            foreach (string tag in new[] { "Fragmento", "Propulsor", "Glifo" })
            {
                var go = GameObject.FindGameObjectsWithTag(tag).First();
                var mat = go.GetComponent<Renderer>().sharedMaterial;
                Assert.IsTrue(mat.IsKeywordEnabled("_EMISSION"), tag + " emission");
                Color e = mat.GetColor("_EmissionColor");
                Assert.AreEqual(accent.r * 1.5f, e.r, 0.01f, tag + " accent r");
                Assert.AreEqual(accent.g * 1.5f, e.g, 0.01f, tag + " accent g");
                Assert.AreEqual(accent.b * 1.5f, e.b, 0.01f, tag + " accent b");
            }
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

        [TestCase("Inicio", "StartMenu")]
        [TestCase("Carga", "LoadingScreen")]
        public void FlowScenes_HaveTheirController(string sceneName, string component)
        {
            Open(sceneName);
            Assert.AreEqual(1, CountComponents(component), component);
            Assert.IsNotNull(Camera.main, "Main Camera missing");
            Assert.IsNotNull(Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>(), "EventSystem missing");
        }

        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Levels_HavePauseMenu(string sceneName)
        {
            Open(sceneName);
            Assert.AreEqual(1, CountComponents("PauseMenu"));
            Assert.IsNotNull(Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>(), "EventSystem missing");
        }

        [TestCase("Introduccion")]
        [TestCase("Despedida")]
        [TestCase("Creditos")]
        [TestCase("Cierre")]
        public void StoryScenes_HaveSkippableStoryScene(string sceneName)
        {
            Open(sceneName);
            Assert.AreEqual(1, CountComponents("StoryScene"));
        }

        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Levels_HaveNarratorWithLevelNumber(string sceneName)
        {
            Open(sceneName);
            Assert.AreEqual(1, CountComponents("LevelNarrator"));
            Assert.AreEqual(1, CountComponents("AchievementToast"));
        }

        [TestCase("Nivel1_Mesopotamia", "mesopotamia")]
        [TestCase("Nivel2_Egipto", "egipto")]
        [TestCase("Nivel3_Grecia", "grecia")]
        [TestCase("Nivel4_China", "china")]
        public void Level_BlocksAreSkinnedWithCultureKitPieces(string sceneName, string culture)
        {
            Open(sceneName);
            var kits = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)
                .Where(m => m.sharedMesh != null && m.gameObject.name.StartsWith("Kit_") && m.sharedMesh.name.StartsWith(culture + "_")).ToArray();
            Assert.GreaterOrEqual(kits.Length, 8, "kit pieces of " + culture);
            // The grey primitive stays as the collider but no longer renders.
            foreach (var k in kits)
            {
                var host = k.transform.parent.Find(k.gameObject.name.Substring("Kit_".Length));
                Assert.IsNotNull(host, k.gameObject.name + " host block");
                Assert.IsNotNull(host.GetComponent<Collider>(), host.name + " keeps its collider");
                Assert.IsFalse(host.GetComponent<Renderer>().enabled, host.name + " grey renderer off");
            }
        }

        [TestCase("Nivel2_Egipto")]
        public void KitColumns_StandUpright(string sceneName)
        {
            Open(sceneName);
            var column = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).First(r => r.gameObject.name.StartsWith("Kit_Columna_1"));
            Assert.Greater(column.bounds.size.y, column.bounds.size.x * 2f, "column height along Y");
        }

        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Portal_IsASolidLandingPad(string sceneName)
        {
            Open(sceneName);
            var portal = GameObject.Find("PortalLlegada");
            Assert.IsFalse(portal.GetComponent<Collider>().isTrigger, "Integrante 1 completes the level in OnCollisionEnter against Plataforma");
            Assert.LessOrEqual(portal.transform.localScale.y, 0.6f, "flat pad to land on");
            Assert.GreaterOrEqual(portal.transform.localScale.x, 4f, "wide enough to land on");
        }

        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Platforms_VisibleTopMatchesColliderTop_NoInvisibleLedges(string sceneName)
        {
            Open(sceneName);
            var modules = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)
                .Where(m => m.sharedMesh != null && m.gameObject.name.StartsWith("Kit_") && m.sharedMesh.name.EndsWith("_platform")).ToArray();
            Assert.Greater(modules.Length, 0, "platform modules");
            foreach (var group in modules.GroupBy(m => m.gameObject.name))
            {
                var host = group.First().transform.parent.Find(group.Key.Substring("Kit_".Length));
                if (host.rotation != Quaternion.identity) continue; // ramps
                Bounds col = host.GetComponent<Collider>().bounds;
                float top = group.Max(m => m.GetComponent<Renderer>().bounds.max.y);
                float minX = group.Min(m => m.GetComponent<Renderer>().bounds.min.x), maxX = group.Max(m => m.GetComponent<Renderer>().bounds.max.x);
                Assert.AreEqual(col.max.y, top, 0.05f, host.name + " visible top vs collider top");
                Assert.LessOrEqual(minX, col.min.x + 0.05f, host.name + " left edge covered");
                Assert.GreaterOrEqual(maxX, col.max.x - 0.05f, host.name + " right edge covered");
            }
        }

        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Level3And4_MainPathIsEightMetresWide(string sceneName)
        {
            Open(sceneName);
            var floors = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name.StartsWith("Suelo_")).ToArray();
            Assert.Greater(floors.Length, 0);
            foreach (var f in floors) Assert.GreaterOrEqual(f.localScale.z, 8f, f.name + " path depth");
        }

        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void BossArena_UsesTheApophisBasaltGround(string sceneName)
        {
            Open(sceneName);
            var arena = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)
                .Where(m => m.sharedMesh != null && m.gameObject.name.StartsWith("Kit_") && m.sharedMesh.name == "arena_platform").ToArray();
            Assert.Greater(arena.Length, 0, "arena modules");
            foreach (var m in arena) Assert.That(m.transform.position.x, Is.InRange(120f, 160f), m.name + " inside zone 4");
        }

        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Level_HasNightLook_FogBloomEmbersAndArenaAmbience(string sceneName)
        {
            Open(sceneName);
            Assert.IsTrue(RenderSettings.fog, "fog");
            var post = GameObject.Find("PostProceso");
            Assert.IsNotNull(post, "PostProceso");
            Assert.IsTrue(post.GetComponents<Component>().Any(c => c.GetType().Name == "Volume"), "global Volume");
            Assert.IsNotNull(GameObject.Find("Brasas"), "embers");
            Assert.IsNotNull(GameObject.Find("Luz_Contraluz"), "rim light");
            Assert.AreEqual(1, CountComponents("ArenaAmbience"));
        }

        // ODD S26 / T26: walking up or down a ramp is continuous, no lip at either end (collider).
        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Ramps_MeetTheFloorsWithoutALip(string sceneName)
        {
            Open(sceneName);
            var solids = Solids();
            var ramps = solids.Where(b => Mathf.Abs(Mathf.DeltaAngle(b.transform.eulerAngles.z, 0f)) > 0.5f).ToArray();
            foreach (var ramp in ramps)
            {
                Vector3 s = ramp.transform.lossyScale;
                Vector3 low = ramp.transform.position + ramp.transform.rotation * new Vector3(-s.x / 2f, s.y / 2f, 0f);
                Vector3 high = ramp.transform.position + ramp.transform.rotation * new Vector3(s.x / 2f, s.y / 2f, 0f);
                var others = solids.Where(b => b != ramp && b.transform.rotation == Quaternion.identity && b.bounds.min.z < ramp.bounds.max.z && b.bounds.max.z > ramp.bounds.min.z).ToArray();
                var floor = others.Where(b => b.bounds.min.x <= low.x + 0.05f && b.bounds.max.x >= low.x - 0.05f &&b.bounds.max.y <= low.y + 0.5f).ToArray();
                Assert.IsNotEmpty(floor, ramp.name + " has no floor at its low end");
                Assert.AreEqual(floor.Max(b => b.bounds.max.y), low.y, 0.05f, ramp.name + " low end vs floor top");
                foreach (var end in new[] { low, high })
                    foreach (var b in others.Where(b => b.bounds.min.x < end.x + 0.3f && b.bounds.max.x > end.x - 0.3f && b.bounds.min.y < end.y + 1f))
                        Assert.LessOrEqual(b.bounds.max.y, end.y + 0.05f, ramp.name + " blocked by " + b.name + " at x " + end.x.ToString("0.0"));
            }
        }

        // Solid untagged scenery colliders of the level (floors, terraces, ramps, walls), without the play-area bounds.
        static BoxCollider[] Solids()
        {
            int entorno = LayerMask.NameToLayer("Entorno");
            return Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None)
                .Where(b => !b.isTrigger && b.gameObject.layer == entorno && b.CompareTag("Untagged") && !b.name.StartsWith("Limites_")).ToArray();
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
