using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using Unit404.Integrante4.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Unit404.Integrante4.Tests
{
    // ODD S33 / T35: flow screens with game fonts (TextMeshPro SDF), background art and a designed layout.
    public class FlowScreensTests
    {
        const string Fonts = "Assets/Integrante4/Art/Fonts/";

        [Test]
        public void TitleFont_IsInstalledWithItsLicenceAsAnSdfAsset()
        {
            Assert.IsTrue(File.Exists(Fonts + "Metamorphous-Regular.ttf"));
            Assert.IsTrue(File.Exists(Fonts + "Metamorphous-OFL.txt"));
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Fonts + "Metamorphous SDF.asset");
            Assert.IsNotNull(asset, "Metamorphous SDF.asset");
            Assert.IsNotNull(asset.atlasTexture, "atlas");
            foreach (char c in "UNIT-404 Egipto Créditos Señal ¿Í")
                Assert.IsTrue(asset.HasCharacter(c, false, true), "missing glyph '" + c + "'");
        }

        [TestCase("Nivel2_Egipto", 2)]
        [TestCase("Transicion3_Grecia", 3)]
        [TestCase("Nivel4_China", 4)]
        [TestCase("Nivel1_Mesopotamia", 1)]
        [TestCase("Creditos", 0)]
        public void ScreenArt_CultureOfScene(string scene, int level)
        {
            Assert.AreEqual(level, ScreenArt.CultureOf(scene));
        }

        [TestCase("Nivel2_Egipto", "Nivel 2 — Egipto")]
        [TestCase("Transicion4_China", "Nivel 4 — China")]
        [TestCase("Creditos", "Créditos")]
        [TestCase("Introduccion", "Introducción")]
        [TestCase("Inicio", "Inicio")]
        public void ScreenArt_LoadingTitleReadsWell(string scene, string title)
        {
            Assert.AreEqual(title, ScreenArt.LoadingTitle(scene));
        }

        [TestCase("Inicio")]
        [TestCase("Carga")]
        [TestCase("Introduccion")]
        [TestCase("Despedida")]
        [TestCase("Creditos")]
        [TestCase("Cierre")]
        public void FlowScreen_UsesGameFontsAndBackgroundArt(string sceneName)
        {
            EditorSceneManager.OpenScene("Assets/Integrante4/Scenes/" + sceneName + ".unity");
            Assert.AreEqual(0, Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Length, "legacy Text left");
            var texts = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            Assert.IsTrue(texts.Any(t => t.font != null && t.font.name == "Metamorphous SDF"), "title in Metamorphous");
            var background = GameObject.Find("Fondo").GetComponent<Image>();
            Assert.IsNotNull(background.sprite, "background art");
            Assert.GreaterOrEqual(background.sprite.texture.width, 1920, "background resolution");
            Assert.IsNotNull(GameObject.Find("Vineta"), "vignette over the art keeps the text readable");
        }

        [Test]
        public void LoadingScreen_HasOneBackgroundPerCulture()
        {
            EditorSceneManager.OpenScene("Assets/Integrante4/Scenes/Carga.unity");
            var loading = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(m => m.GetType().Name == "LoadingScreen");
            var sprites = (Sprite[])loading.GetType().GetField("cultureBackgrounds").GetValue(loading);
            Assert.AreEqual(5, sprites.Length, "index 0 = default, 1-4 = cultures");
            Assert.IsTrue(sprites.All(s => s != null));
            Assert.AreEqual(5, sprites.Select(s => s.name).Distinct().Count());
        }
    }
}
