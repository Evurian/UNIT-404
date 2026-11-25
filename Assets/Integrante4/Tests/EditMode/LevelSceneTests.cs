using System.Linq;
using NUnit.Framework;
using UnityEditor;
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
                string hostName = k.gameObject.name.Substring("Kit_".Length);
                // blocks get a sibling skin; gameplay elements (T18) carry it as a child so it moves and hides with them
                var host = k.transform.parent.name == hostName ? k.transform.parent : k.transform.parent.Find(hostName);
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

        // ODD S23 (b, c) / T23: only the boss arena is dark. Mesopotamia keeps the teammate's red basalt (`arena` kit),
        // Egipto, Grecia and China use their own dark stone there; the route uses the lit culture material.
        [TestCase("Nivel1_Mesopotamia", "mesopotamia", "arena_platform")]
        [TestCase("Nivel2_Egipto", "egipto", "egipto_arena_platform")]
        [TestCase("Nivel3_Grecia", "grecia", "grecia_arena_platform")]
        [TestCase("Nivel4_China", "china", "china_arena_platform")]
        public void BossArena_UsesTheCultureDarkGround_RouteUsesTheDayMaterial(string sceneName, string culture, string arenaMaterial)
        {
            Open(sceneName);
            var modules = Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)
                .Where(r => r.gameObject.name.StartsWith("Kit_") && r.GetComponent<MeshFilter>().sharedMesh.name.EndsWith("_platform")).ToArray();
            var arena = modules.Where(r => r.sharedMaterial.name == arenaMaterial).ToArray();
            Assert.Greater(arena.Length, 0, "arena modules");
            foreach (var r in arena)
            {
                Assert.That(r.transform.position.x, Is.InRange(120f, 160f), r.name + " inside zone 4");
                Assert.IsTrue(r.sharedMaterial.IsKeywordEnabled("_EMISSION"), r.name + " glowing cracks");
            }
            var route = modules.Where(r => r.transform.position.x < 120f || r.transform.position.x > 160f).ToArray();
            Assert.Greater(route.Length, 0, "route modules");
            foreach (var r in route)
            {
                Assert.AreEqual(culture + "_platform", r.sharedMaterial.name, r.name + " day material");
                Assert.IsFalse(r.sharedMaterial.IsKeywordEnabled("_EMISSION"), r.name + " no glowing cracks on the route");
            }
        }

        // ODD S30 / T29: sharper route materials: 2048 maps, baked occlusion, per-pixel smoothness, anisotropic filtering.
        // Checks every route kit material the level actually renders (the dark arena keeps its own maps).
        [TestCase("Nivel1_Mesopotamia", "mesopotamia")]
        [TestCase("Nivel2_Egipto", "egipto")]
        [TestCase("Nivel3_Grecia", "grecia")]
        [TestCase("Nivel4_China", "china")]
        public void KitMaterials_AreSharp(string sceneName, string culture)
        {
            Open(sceneName);
            var pieces = new[] { "platform", "wall", "stairs", "column", "block" };
            var checkedNames = new System.Collections.Generic.HashSet<string>();
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                var mat = r.sharedMaterial;
                if (mat == null || System.Array.IndexOf(pieces, mat.name.Replace(culture + "_", "")) < 0) continue;
                if (!checkedNames.Add(mat.name)) continue;
                string name = mat.name;
                var albedo = mat.GetTexture("_BaseMap") as Texture2D;
                Assert.IsNotNull(albedo, name + " albedo");
                Assert.GreaterOrEqual(albedo.width, 2048, name + " albedo resolution");
                Assert.GreaterOrEqual(albedo.anisoLevel, 4, name + " anisotropic filtering");
                Assert.IsNotNull(mat.GetTexture("_OcclusionMap"), name + " occlusion map");
                Assert.IsTrue(mat.IsKeywordEnabled("_OCCLUSIONMAP"), name + " occlusion keyword");
                Assert.AreEqual(1f, mat.GetFloat("_SmoothnessTextureChannel"), name + " smoothness from the albedo alpha");
                Assert.IsTrue(mat.IsKeywordEnabled("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A"), name + " smoothness keyword");
            }
            Assert.GreaterOrEqual(checkedNames.Count, 2, "route kit materials found: " + string.Join(",", checkedNames));
        }

        // ODD S28 / T30: every level is dressed with its culture's props. 2.5D: behind the path on a visible decorative
        // ground, no colliders. 3D: solid props standing on walkable surfaces, clear of pickups and moving obstacles.
        static readonly System.Collections.Generic.Dictionary<string, string[]> PropKinds = new System.Collections.Generic.Dictionary<string, string[]>
        {
            { "mesopotamia", new[] { "hut", "jars", "bricks", "brazier", "lamassu", "palm" } },
            { "egipto", new[] { "obelisk", "sphinx", "pharaoh", "papyrus", "stele", "palm", "brazier" } },
            { "grecia", new[] { "amphorae", "kouros", "olive", "altar", "lintel_gr", "brazier" } },
            { "china", new[] { "lantern", "lion", "bamboo", "banner", "lintel_cn" } },
        };

        [TestCase("Nivel1_Mesopotamia", "mesopotamia", false)]
        [TestCase("Nivel2_Egipto", "egipto", false)]
        [TestCase("Nivel3_Grecia", "grecia", true)]
        [TestCase("Nivel4_China", "china", true)]
        public void Level_IsDressedWithCultureProps(string sceneName, string culture, bool is3D)
        {
            Open(sceneName);
            var names = PropKinds[culture].Select(k => culture + "_" + k).ToArray();
            var props = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(m => m.sharedMesh != null && names.Contains(m.sharedMesh.name)).ToArray();
            Assert.GreaterOrEqual(props.Select(m => m.sharedMesh.name).Distinct().Count(), 4, "prop variety");
            Assert.GreaterOrEqual(props.Length, is3D ? 12 : 24, "props");
            int[] zones = is3D ? new[] { 0, 1, 3 } : new[] { 0, 1, 2, 4 };
            foreach (int z in zones)
                Assert.GreaterOrEqual(props.Count(m => m.GetComponent<Renderer>().bounds.center.x >= z * 40f && m.GetComponent<Renderer>().bounds.center.x < z * 40f + 40f), is3D ? 2 : 3, "props in zone " + (z + 1));

            if (!is3D)
            {
                var ground = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r => r.name.StartsWith("Deco_Explanada")).Select(r => r.bounds).ToArray();
                Assert.Greater(ground.Length, 0, "decorative ground behind the path");
                // ODD S35 / T37: the arena floor keeps the path depth, no decorative apron behind it.
                Assert.IsFalse(ground.Any(g => g.center.x > 120f && g.center.x < 160f), "no decorative ground in the boss arena");
                foreach (var m in props)
                {
                    Bounds b = m.GetComponent<Renderer>().bounds;
                    Assert.AreEqual(0, m.GetComponentsInParent<Collider>().Length + m.GetComponentsInChildren<Collider>().Length, m.name + " no collider in 2.5D");
                    Assert.Greater(b.min.z, 1f, m.name + " behind the path wall");
                    Assert.IsTrue(ground.Any(g => g.min.x <= b.center.x && g.max.x >= b.center.x && g.min.z <= b.center.z && g.max.z >= b.center.z && Mathf.Abs(g.max.y - b.min.y) < 0.05f),
                        m.name + " stands on the decorative ground: " + b);
                }
                return;
            }

            var solids = Solids();
            var triggers = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c => c.isTrigger).ToArray();
            var sweeps = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(m => m.GetType().Name == "MovingObstacle").Select(m =>
            {
                Bounds s = m.GetComponent<Collider>().bounds;
                s.Encapsulate(s.center + (Vector3)m.GetType().GetField("travel").GetValue(m));
                s.Expand(0.2f);
                return s;
            }).ToArray();
            foreach (var m in props)
            {
                string hostName = m.gameObject.name.Substring("Kit_".Length);
                var host = m.transform.parent.Find(hostName);
                Assert.IsNotNull(host, m.name + " host");
                var box = host.GetComponent<BoxCollider>();
                Assert.IsNotNull(box, host.name + " solid prop");
                Assert.IsFalse(box.isTrigger, host.name + " solid prop");
                Bounds hb = box.bounds;
                Assert.IsTrue(solids.Any(s => s != box && Mathf.Abs(s.bounds.max.y - hb.min.y) < 0.05f && s.bounds.min.x <= hb.center.x && s.bounds.max.x >= hb.center.x && s.bounds.min.z < hb.max.z && s.bounds.max.z > hb.min.z),
                    host.name + " stands on a walkable surface: " + hb);
                Bounds inner = hb;
                inner.Expand(-0.05f);
                foreach (var t in triggers) Assert.IsFalse(inner.Intersects(t.bounds), host.name + " overlaps " + t.name);
                foreach (var s in sweeps) Assert.IsFalse(inner.Intersects(s), host.name + " stands in the path of a moving obstacle");
            }
        }

        // ODD S29 / T31: the 3D corridors have a rhythm of gates: a pillar or statue on each side of the 8 m path and, in
        // most of them, a guard or obstacle in the middle, with a playable lane on each side.
        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Level3And4_HaveCorridorGates(string sceneName)
        {
            Open(sceneName);
            var hosts = Solids().Where(b => System.Text.RegularExpressions.Regex.IsMatch(b.name, @"^Puerta\d+_[ID]_")).ToArray();
            var gates = hosts.GroupBy(b => b.name.Substring(0, b.name.IndexOf('_'))).ToArray();
            Assert.GreaterOrEqual(gates.Length, 4, "gates");
            var middles = GameObject.FindGameObjectsWithTag("Observador").Concat(GameObject.FindGameObjectsWithTag("Obstaculo")).Select(g => g.GetComponent<Collider>().bounds).ToArray();
            int guarded = 0;
            foreach (var g in gates)
            {
                var left = g.Single(b => b.name.Contains("_I_")).bounds;
                var right = g.Single(b => b.name.Contains("_D_")).bounds;
                Assert.AreEqual(0f, left.center.z + right.center.z, 0.1f, g.Key + " pillars mirrored across the path");
                Assert.AreEqual(left.center.x, right.center.x, 0.1f, g.Key + " pillars face each other");
                Assert.GreaterOrEqual(right.center.z, 2.5f, g.Key + " pillars on the path edges");
                Assert.IsTrue(Kit(g.First().gameObject) != null, g.Key + " pillars wear a kit piece");
                var mid = middles.Where(b => Mathf.Abs(b.center.x - left.center.x) < 1.5f && Mathf.Abs(b.center.z) < 1.5f).ToArray();
                if (mid.Length == 0) continue;
                guarded++;
                Bounds m = mid[0];
                Assert.GreaterOrEqual(right.min.z - m.max.z, 1.5f, g.Key + " playable lane on the right");
                Assert.GreaterOrEqual(m.min.z - left.max.z, 1.5f, g.Key + " playable lane on the left");
            }
            Assert.GreaterOrEqual(guarded, 3, "gates with a guard or obstacle in the middle");
        }

        static Transform Kit(GameObject host) => host.transform.parent.Find("Kit_" + host.name);

        // ODD S23 (a, b, d) / T20: lit culture day on the route; entering the arena switches to the dark look, in the culture accent.
        [TestCase("Nivel1_Mesopotamia", 1)]
        [TestCase("Nivel2_Egipto", 2)]
        [TestCase("Nivel3_Grecia", 3)]
        [TestCase("Nivel4_China", 4)]
        public void Level_RouteIsALitDay_ArenaSwitchesToTheDarkAccentLook(string sceneName, int level)
        {
            Open(sceneName);
            ColorUtility.TryParseHtmlString(Unit404.Integrante4.Core.CulturePalette.AccentHex(level), out Color accent);
            var sun = GameObject.Find("Luz_Sol").GetComponent<Light>();
            var dayVolume = GameObject.Find("PostProceso").GetComponents<Component>().First(c => c.GetType().Name == "Volume");
            var arenaVolume = GameObject.Find("PostProceso_Arena").GetComponents<Component>().First(c => c.GetType().Name == "Volume");
            float Weight(Component v) => (float)v.GetType().GetField("weight").GetValue(v);
            Assert.IsNull(GameObject.Find("Brasas"), "no embers along the route");

            // saved state = the route
            Assert.IsTrue(RenderSettings.fog, "light haze");
            Assert.Greater(Lum(RenderSettings.fogColor), 0.4f, "route fog is not dark");
            Assert.Greater(Lum(RenderSettings.ambientLight), 0.25f, "route ambient is not dark");
            Assert.GreaterOrEqual(sun.intensity, 1f, "sun");
            Assert.AreEqual(CameraClearFlags.Skybox, Camera.main.clearFlags, "culture sky");
            Assert.IsNotNull(RenderSettings.skybox, "culture sky material");
            Assert.AreEqual(1f, Weight(dayVolume), 0.001f, "day post-processing");
            Assert.AreEqual(0f, Weight(arenaVolume), 0.001f, "arena post-processing off");

            var ambience = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(m => m.GetType().Name == "ArenaAmbience");
            var apply = ambience.GetType().GetMethod("Apply");
            apply.Invoke(ambience, new object[] { Unit404.Integrante4.Core.AmbienceMode.Boss });
            Assert.Less(Lum(RenderSettings.fogColor), 0.05f, "arena fog is dark");
            Assert.Less(sun.intensity, 0.5f, "arena key is dim");
            Assert.AreEqual(CameraClearFlags.SolidColor, Camera.main.clearFlags, "arena sky is dark");
            Assert.AreEqual(1f, Weight(arenaVolume), 0.001f, "arena post-processing");
            Assert.AreEqual(0f, Weight(dayVolume), 0.001f, "day post-processing off");
            var rim = GameObject.Find("Luz_Contraluz").GetComponent<Light>();
            Assert.AreEqual(accent, rim.color, "arena rim in the culture accent");
            var embers = GameObject.Find("Brasas_Arena");
            Assert.IsNotNull(embers, "arena embers active");
            Color e = embers.GetComponent<ParticleSystem>().main.startColor.color;
            Assert.AreEqual(accent.r / accent.maxColorComponent, e.r / e.maxColorComponent, 0.02f, "embers in the accent");
            Assert.AreEqual(accent.g / accent.maxColorComponent, e.g / e.maxColorComponent, 0.02f, "embers in the accent");
            Assert.AreEqual(accent.b / accent.maxColorComponent, e.b / e.maxColorComponent, 0.02f, "embers in the accent");

            apply.Invoke(ambience, new object[] { Unit404.Integrante4.Core.AmbienceMode.Level });
            Assert.Greater(Lum(RenderSettings.fogColor), 0.4f, "back to the culture day");
            Assert.AreEqual(CameraClearFlags.Skybox, Camera.main.clearFlags, "back to the culture sky");
        }

        static float Lum(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;

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

        // ODD S40 / T41: along the 3D main line (full-width walkways) every height change is a ramp, never a step;
        // China has no gap at all and its wall walk is a solid body down to the ground.
        [TestCase("Nivel3_Grecia", true)]
        [TestCase("Nivel4_China", false)]
        public void MainLine_HasNoStepWithoutARamp(string sceneName, bool gapsAllowed)
        {
            Open(sceneName);
            var walk = Solids().Where(b => b.bounds.min.z <= -3.9f && b.bounds.max.z >= 3.9f && b.CompareTag("Untagged") && !b.name.StartsWith("Puerta")).ToArray(); // lintels are overhead
            float end = GameObject.Find("PortalLlegada").transform.position.x;
            float? prev = null;
            for (float x = 0.25f; x < end; x += 0.5f)
            {
                float? top = null;
                foreach (var b in walk)
                {
                    if (b.bounds.min.x > x || b.bounds.max.x < x) continue;
                    float t = b.bounds.max.y;
                    if (Mathf.Abs(Mathf.DeltaAngle(b.transform.eulerAngles.z, 0f)) > 0.5f)
                    {
                        Vector3 sc = b.transform.lossyScale;
                        Vector3 lo = b.transform.position + b.transform.rotation * new Vector3(-sc.x / 2f, sc.y / 2f, 0f);
                        Vector3 hi = b.transform.position + b.transform.rotation * new Vector3(sc.x / 2f, sc.y / 2f, 0f);
                        t = Mathf.Lerp(lo.y, hi.y, Mathf.InverseLerp(lo.x, hi.x, x));
                    }
                    top = top.HasValue ? Mathf.Max(top.Value, t) : t;
                }
                if (!top.HasValue)
                {
                    Assert.IsTrue(gapsAllowed, "gap in the main line at x " + x);
                    continue;
                }
                if (prev.HasValue) Assert.LessOrEqual(Mathf.Abs(top.Value - prev.Value), 0.35f, "step without a ramp at x " + x.ToString("0.0") + ": " + prev + " -> " + top);
                prev = top;
            }
            if (!gapsAllowed)
                foreach (var name in new[] { "Adarve", "Tramo_Ancho" })
                {
                    Bounds w = GameObject.Find(name).GetComponent<Collider>().bounds;
                    Assert.IsTrue(walk.Any(b => b.bounds.min.y <= 0.05f && b.bounds.max.y >= w.min.y - 0.05f && b.bounds.min.x <= w.min.x + 0.05f && b.bounds.max.x >= w.max.x - 0.05f),
                        name + " has a wall body down to the ground");
                }
        }

        // ODD S25 / T25: six invisible solid bounds keep the robot inside the level even if a bug launches it.
        [TestCase("Nivel1_Mesopotamia", false)]
        [TestCase("Nivel2_Egipto", false)]
        [TestCase("Nivel3_Grecia", true)]
        [TestCase("Nivel4_China", true)]
        public void Level_HasSixInvisibleBoundsEnclosingThePlayArea(string sceneName, bool is3D)
        {
            Open(sceneName);
            string[] names = { "Inicio", "Fin", "Izquierda", "Derecha", "Techo", "Suelo" };
            var b = names.ToDictionary(n => n, n => GameObject.Find("Limites_" + n));
            foreach (var pair in b)
            {
                Assert.IsNotNull(pair.Value, "Limites_" + pair.Key);
                var box = pair.Value.GetComponent<BoxCollider>();
                Assert.IsNotNull(box, pair.Key + " collider");
                Assert.IsFalse(box.isTrigger, pair.Key + " must be solid");
                Assert.IsNull(pair.Value.GetComponent<Renderer>(), pair.Key + " must be invisible");
                Assert.AreEqual(LayerMask.NameToLayer("Entorno"), pair.Value.layer, pair.Key + " layer");
                Assert.IsTrue(pair.Value.CompareTag("Untagged"), pair.Key + " tag");
            }
            Bounds C(string n) => b[n].GetComponent<BoxCollider>().bounds;
            var inner = new Bounds();
            inner.SetMinMax(new Vector3(C("Inicio").max.x, C("Suelo").max.y, C("Izquierda").max.z), new Vector3(C("Fin").min.x, C("Techo").min.y, C("Derecha").min.z));

            foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(c => !c.name.StartsWith("Limites_")))
            {
                Assert.GreaterOrEqual(c.bounds.min.x, inner.min.x - 0.01f, c.name + " beyond the start");
                Assert.LessOrEqual(c.bounds.max.x, inner.max.x + 0.01f, c.name + " beyond the end");
                // 2.5D: the walls hug the path; triggers (ring capsules) and the static boss placeholder may stand into them
                if (is3D || (!c.isTrigger && !c.CompareTag("Jefe")))
                {
                    Assert.GreaterOrEqual(c.bounds.min.z, inner.min.z - 0.01f, c.name + " beyond the near side");
                    Assert.LessOrEqual(c.bounds.max.z, inner.max.z + 0.01f, c.name + " beyond the far side");
                }
                Assert.GreaterOrEqual(c.bounds.min.y, inner.min.y - 0.01f, c.name + " below the floor bound");
                Assert.LessOrEqual(c.bounds.max.y, inner.max.y + 0.01f, c.name + " above the ceiling");
            }
            Assert.IsTrue(inner.Contains(GameObject.Find("PlayerStart").transform.position), "PlayerStart inside");

            float highestWalkable = Solids().Concat(GameObject.FindGameObjectsWithTag("Plataforma").Select(g => g.GetComponent<BoxCollider>())).Max(s => s.bounds.max.y);
            Assert.AreEqual(highestWalkable + 12f, inner.max.y, 0.05f, "ceiling = highest walkable surface + 12 m");
            if (!is3D)
            {
                Assert.AreEqual(-1f, inner.min.z, 0.01f, "2.5D near wall hugs the 2 m path");
                Assert.AreEqual(1f, inner.max.z, 0.01f, "2.5D far wall hugs the 2 m path");
            }
            else
            {
                Assert.LessOrEqual(inner.min.z, -12f, "3D bounds wrap the side paths");
                Assert.GreaterOrEqual(inner.max.z, 12f, "3D bounds wrap the side paths");
            }
        }

        // ODD S25 / T27: falling below the level opens "Sistema caído" and respawns at the last checkpoint or the arena door.
        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel2_Egipto")]
        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Level_HasFallZoneBelowEverySurfaceAndTheRetryScreen(string sceneName)
        {
            Open(sceneName);
            var zone = GameObject.Find("ZonaCaida");
            Assert.IsNotNull(zone, "ZonaCaida");
            var trigger = zone.GetComponent<BoxCollider>();
            Assert.IsTrue(trigger.isTrigger, "fall zone is a trigger");
            Assert.AreEqual(1, zone.GetComponents<MonoBehaviour>().Count(m => m.GetType().Name == "FallZone"), "FallZone");

            var solids = Solids();
            Bounds z = trigger.bounds;
            Assert.LessOrEqual(z.min.x, solids.Min(s => s.bounds.min.x), "covers the start");
            Assert.GreaterOrEqual(z.max.x, solids.Max(s => s.bounds.max.x), "covers the end");
            Assert.LessOrEqual(z.min.z, solids.Min(s => s.bounds.min.z), "covers the near side");
            Assert.GreaterOrEqual(z.max.z, solids.Max(s => s.bounds.max.z), "covers the far side");
            Assert.Less(z.max.y, solids.Min(s => s.bounds.min.y), "below the lowest surface");
            Assert.AreEqual(GameObject.Find("Limites_Suelo").GetComponent<BoxCollider>().bounds.max.y, z.min.y, 0.05f, "reaches down to the floor bound");

            var screen = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(m => m.GetType().Name == "FallScreen");
            var panel = (GameObject)screen.GetType().GetField("panel").GetValue(screen);
            Assert.IsFalse(panel.activeSelf, "hidden until a fall");
            var texts = panel.GetComponentsInChildren<UnityEngine.UI.Text>(true).Select(t => t.text).ToArray();
            CollectionAssert.Contains(texts, "Sistema caído");
            CollectionAssert.Contains(texts, "Reintentar desde el punto de control");
            var door = (Transform)screen.GetType().GetField("arenaDoor").GetValue(screen);
            Assert.That(door.position.x, Is.InRange(120f, 160f), "arena door at the start of zone 4");
            Assert.AreEqual(2, ((Transform[])screen.GetType().GetField("checkpoints").GetValue(screen)).Length, "both checkpoints wired");
        }

        // ODD S34 / S36 / T36: the fall screen is a full-screen card in the culture's palette, and the pause
        // menu knows it, so Esc does not open the pause panel under it.
        [TestCase("Nivel1_Mesopotamia", 1)]
        [TestCase("Nivel2_Egipto", 2)]
        [TestCase("Nivel3_Grecia", 3)]
        [TestCase("Nivel4_China", 4)]
        public void Level_FallScreenIsAFullCultureScreenAndBlocksPause(string sceneName, int level)
        {
            Open(sceneName);
            var screen = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(m => m.GetType().Name == "FallScreen");
            var panel = (GameObject)screen.GetType().GetField("panel").GetValue(screen);
            var rt = panel.GetComponent<RectTransform>();
            Assert.AreEqual(Vector2.zero, rt.anchorMin, "stretches over the whole view");
            Assert.AreEqual(Vector2.one, rt.anchorMax, "stretches over the whole view");
            var bg = panel.GetComponent<UnityEngine.UI.Image>();
            Assert.IsTrue(ColorUtility.TryParseHtmlString(Unit404.Integrante4.Core.CulturePalette.FallHex(level), out Color tint));
            Assert.AreEqual(tint.r, bg.color.r, 0.01f, "culture tint r");
            Assert.AreEqual(tint.g, bg.color.g, 0.01f, "culture tint g");
            Assert.AreEqual(tint.b, bg.color.b, 0.01f, "culture tint b");
            Assert.GreaterOrEqual(bg.color.a, 0.95f, "a screen, not a see-through overlay");
            var texts = panel.GetComponentsInChildren<UnityEngine.UI.Text>(true).Select(t => t.text).ToArray();
            string culture = Unit404.Integrante4.Core.CulturePalette.CultureName(level);
            Assert.IsTrue(texts.Any(t => string.Equals(t, culture, System.StringComparison.OrdinalIgnoreCase)), "culture name on the screen");

            var pause = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(m => m.GetType().Name == "PauseMenu");
            Assert.AreSame(screen, pause.GetType().GetField("fallScreen").GetValue(pause), "pause menu wired to the fall screen");
        }

        // ODD S19 / T15: on the 8 m path every obstacle row blocks the full width, and a side path next to an
        // obstacle carries its own row, so nothing can be skirted around.
        [TestCase("Nivel3_Grecia")]
        [TestCase("Nivel4_China")]
        public void Level3And4_ObstacleRowsBlockTheWholePathAndTheSidePaths(string sceneName)
        {
            Open(sceneName);
            var movers = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(m => m.GetType().Name == "MovingObstacle" && m.CompareTag("Obstaculo")).ToArray();
            var rows = movers.GroupBy(m => System.Text.RegularExpressions.Regex.Replace(m.name, "_[A-Z]$", "")).Select(g => new
            {
                name = g.Key,
                bounds = g.Select(m => m.GetComponent<Collider>().bounds).ToArray(),
                minX = g.Min(m => Mathf.Min(m.GetComponent<Collider>().bounds.min.x, m.GetComponent<Collider>().bounds.min.x + ((Vector3)m.GetType().GetField("travel").GetValue(m)).x)),
                maxX = g.Max(m => Mathf.Max(m.GetComponent<Collider>().bounds.max.x, m.GetComponent<Collider>().bounds.max.x + ((Vector3)m.GetType().GetField("travel").GetValue(m)).x)),
            }).ToArray();
            var main = rows.Where(r => Mathf.Abs(r.bounds.Average(b => b.center.z)) < 4f).ToArray();
            Assert.GreaterOrEqual(main.Length, 3, "obstacle rows on the main path");
            foreach (var r in main) Assert.IsTrue(Covers(r.bounds, -4f, 4f), r.name + " leaves a free lane in |z| <= 4");

            foreach (var side in Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Where(b => b.name.StartsWith("SenderoLateral_")))
            {
                Bounds s = side.bounds;
                if (!main.Any(r => r.maxX > s.min.x && r.minX < s.max.x)) continue;
                Assert.IsTrue(rows.Any(r => r.bounds.All(b => b.center.x > s.min.x && b.center.x < s.max.x) && Covers(r.bounds, s.min.z, s.max.z)),
                    side.name + " is a bypass: it runs beside an obstacle row without a row of its own");
            }
        }

        // True when the z ranges of the boxes, merged, cover [from, to].
        // ODD T17: the blocks that were stretched walls or tiled slabs get their own kit piece, flat top = collider top.
        [TestCase("Nivel1_Mesopotamia", "Terraza_1", "mesopotamia_stairs")]
        [TestCase("Nivel1_Mesopotamia", "Terraza_Orilla", "mesopotamia_stairs")]
        [TestCase("Nivel2_Egipto", "Bloque_Piedra_2", "egipto_block")]
        [TestCase("Nivel4_China", "Mastil_1", "china_mast")]
        [TestCase("Nivel4_China", "Torre_Estandarte", "china_keep")]
        public void DetailedKitPieces_SkinTheirBlocks_TopMatchesCollider(string sceneName, string block, string mesh)
        {
            Open(sceneName);
            var host = GameObject.Find(block);
            var kit = host.transform.parent.Find("Kit_" + block);
            Assert.IsNotNull(kit, "Kit_" + block);
            Assert.AreEqual(mesh, kit.GetComponent<MeshFilter>().sharedMesh.name);
            Bounds col = host.GetComponent<Collider>().bounds, vis = kit.GetComponent<Renderer>().bounds;
            Assert.AreEqual(col.max.y, vis.max.y, mesh == "china_mast" ? 0.6f : 0.05f, block + " visible top vs collider top");
            Assert.AreEqual(col.min.y, vis.min.y, 0.05f, block + " stands on the collider base");
        }

        // ODD T18: pickups, rings, glyphs, checkpoints and obstacles keep their collider and wear a culture element
        // as a child that fills the same box.
        [TestCase("Nivel1_Mesopotamia", "mesopotamia", "Estaca", "stake")]
        [TestCase("Nivel2_Egipto", "egipto", "Antorcha", "torch")]
        [TestCase("Nivel3_Grecia", "grecia", "Tronco", "log")]
        [TestCase("Nivel3_Grecia", "grecia", "ColumnaCaida", "drum")]
        [TestCase("Nivel4_China", "china", "Carreta", "cart")]
        public void GameplayElements_WearCultureArt_InsideTheirCollider(string sceneName, string culture, string obstacle, string obstacleArt)
        {
            Open(sceneName);
            var expected = new System.Collections.Generic.List<(GameObject go, string art)>();
            foreach (var pair in new[] { ("Fragmento", "crystal"), ("Propulsor", "ring"), ("Glifo", "tablet"), ("Checkpoint", "checkpoint") })
                expected.AddRange(GameObject.FindGameObjectsWithTag(pair.Item1).Select(g => (g, pair.Item2)));
            var obstacles = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Where(t => t.name.StartsWith(obstacle + "_") && t.GetComponent<Collider>() != null).Select(t => (t.gameObject, obstacleArt)).ToArray();
            Assert.Greater(obstacles.Length, 0, obstacle + " obstacles");
            expected.AddRange(obstacles);
            foreach (var (go, art) in expected)
            {
                var kit = go.transform.Find("Kit_" + go.name);
                Assert.IsNotNull(kit, go.name + " art child");
                Assert.AreEqual(culture + "_" + art, kit.GetComponent<MeshFilter>().sharedMesh.name, go.name);
                Assert.IsFalse(go.GetComponent<Renderer>().enabled, go.name + " grey primitive hidden");
                Assert.IsNotNull(go.GetComponent<Collider>(), go.name + " keeps its collider");
                Bounds col = go.GetComponent<Collider>().bounds, vis = kit.GetComponent<Renderer>().bounds;
                col.Expand(0.3f);
                Assert.IsTrue(col.Contains(vis.min) && col.Contains(vis.max), go.name + " art stays inside its collider: " + vis + " vs " + col);
            }
        }

        [TestCase("Nivel1_Mesopotamia", "mesopotamia")]
        [TestCase("Nivel2_Egipto", "egipto")]
        [TestCase("Nivel3_Grecia", "grecia")]
        [TestCase("Nivel4_China", "china")]
        public void ArrivalPad_HasACulturePortalArch_WithoutCollider(string sceneName, string culture)
        {
            Open(sceneName);
            var arch = GameObject.Find("PortalArco");
            Assert.IsNotNull(arch);
            Assert.AreEqual(culture + "_arch", arch.GetComponentInChildren<MeshFilter>().sharedMesh.name);
            Assert.AreEqual(0, arch.GetComponentsInChildren<Collider>().Length, "decoration only");
            Assert.AreEqual(GameObject.Find("PortalLlegada").transform.position.x, arch.transform.position.x, 0.01f);
        }

        // ODD S17 / T19: three modelled depth layers per culture replace the grey Fondo_* boxes; they stay outside the
        // play area (2.5D behind the path, 3D on both sides of it) and have no colliders.
        [TestCase("Nivel1_Mesopotamia", "mesopotamia", false)]
        [TestCase("Nivel2_Egipto", "egipto", false)]
        [TestCase("Nivel3_Grecia", "grecia", true)]
        [TestCase("Nivel4_China", "china", true)]
        public void Backdrops_AreThreeCultureLayers_OutsideThePlayArea(string sceneName, string culture, bool bothSides)
        {
            Open(sceneName);
            Assert.AreEqual(0, Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(t => t.name.StartsWith("Fondo_")), "grey boxes gone");
            var layers = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)
                .Where(m => m.sharedMesh != null && m.sharedMesh.name.StartsWith(culture + "_bg_")).ToArray();
            CollectionAssert.AreEquivalent(new[] { culture + "_bg_far", culture + "_bg_mid", culture + "_bg_near" },
                layers.Select(m => m.sharedMesh.name).Distinct().ToArray());
            float playEdge = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .Where(t => t.name == "Limites_Izquierda" || t.name == "Limites_Derecha").Max(t => t.GetComponent<Collider>().bounds.max.z);
            foreach (var m in layers)
            {
                Assert.AreEqual(0, m.GetComponentsInChildren<Collider>().Length, m.name + " no collider");
                Bounds b = m.GetComponent<Renderer>().bounds;
                Assert.Greater(Mathf.Min(Mathf.Abs(b.min.z), Mathf.Abs(b.max.z)), playEdge, m.name + " outside the play area: " + b);
                Assert.IsTrue(b.min.z > 0f || b.max.z < 0f, m.name + " stays on one side");
            }
            Assert.IsTrue(layers.Any(m => m.transform.position.z > 0f));
            Assert.AreEqual(bothSides, layers.Any(m => m.transform.position.z < 0f), "3D worlds have scenery on both sides");
            var levelX = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name == "PortalLlegada" || t.name == "PlayerStart").Select(t => t.position.x).ToArray();
            foreach (string layer in new[] { "near", "mid", "far" })
            {
                var strip = layers.Where(m => m.sharedMesh.name.EndsWith(layer) && m.transform.position.z > 0f).Select(m => m.GetComponent<Renderer>().bounds).ToArray();
                Assert.LessOrEqual(strip.Min(b => b.min.x), levelX.Min() - 10f, layer + " starts before the level");
                Assert.GreaterOrEqual(strip.Max(b => b.max.x), levelX.Max() + 10f, layer + " runs past the portal");
            }
        }

        // ODD S22 / T18 (L46 visual check): the 40 m hillsides of level 4 are tiled Great Wall pieces, not stretched slabs.
        [Test]
        public void Level4_Hillsides_AreTiledWallPieces()
        {
            Open("Nivel4_China");
            foreach (string side in new[] { "Ladera_Izq", "Ladera_Der" })
            {
                var host = GameObject.Find(side);
                var pieces = host.transform.parent.Cast<Transform>().Where(t => t.name == "Kit_" + side).ToArray();
                Assert.AreEqual(4, pieces.Length, side + " 10 m wall pieces");
                Assert.IsTrue(pieces.All(t => t.GetComponent<MeshFilter>().sharedMesh.name == "china_wall"), side);
                Bounds col = host.GetComponent<Collider>().bounds;
                Assert.LessOrEqual(pieces.Min(t => t.GetComponent<Renderer>().bounds.min.x), col.min.x + 0.05f);
                Assert.GreaterOrEqual(pieces.Max(t => t.GetComponent<Renderer>().bounds.max.x), col.max.x - 0.05f);
            }
        }

        // ODD S38 / T39: pollution hazards (incense, dust storm) read as particle clouds filling their trigger, not as a
        // box: the primitive renders nothing, a soft-textured, see-through particle system fills the same volume.
        [TestCase("Nivel1_Mesopotamia")]
        [TestCase("Nivel4_China")]
        public void Hazards_AreParticleClouds(string sceneName)
        {
            Open(sceneName);
            var hazards = GameObject.FindGameObjectsWithTag("Contaminacion");
            Assert.Greater(hazards.Length, 0);
            foreach (var h in hazards)
            {
                var col = h.GetComponent<Collider>();
                Assert.IsTrue(col.isTrigger, h.name + " stays a trigger");
                Assert.IsFalse(h.GetComponent<Renderer>().enabled, h.name + " primitive hidden");
                var ps = h.GetComponentInChildren<ParticleSystem>(true);
                Assert.IsNotNull(ps, h.name + " particle cloud");
                Assert.IsTrue(ps.main.loop && ps.main.prewarm, h.name + " always on");
                Assert.Less(ps.main.startColor.color.a, 0.6f, h.name + " see-through");
                var mat = ps.GetComponent<ParticleSystemRenderer>().sharedMaterial;
                Assert.IsNotNull(mat, h.name + " material");
                Assert.IsNotNull(mat.mainTexture, h.name + " soft puff texture");
                Vector3 area = Vector3.Scale(ps.shape.scale, ps.transform.lossyScale);
                Vector3 box = col.bounds.size;
                // front area (x, y): a sphere trigger is as deep as it is wide even when the 2.5D cloud is thin in z
                float covered = Mathf.Abs(area.x * area.y) / (box.x * box.y);
                Assert.That(covered, Is.InRange(0.5f, 1.5f), h.name + " cloud fills the trigger area: " + area + " vs " + box);
            }
        }

        static bool Covers(Bounds[] boxes, float from, float to)
        {
            float reach = from;
            foreach (var b in boxes.OrderBy(b => b.min.z))
            {
                if (b.min.z > reach + 0.05f) break;
                reach = Mathf.Max(reach, b.max.z);
            }
            return reach >= to - 0.05f;
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
