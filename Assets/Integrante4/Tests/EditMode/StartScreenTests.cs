using System.Linq;
using NUnit.Framework;
using Unit404.Integrante4.Core;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Unit404.Integrante4.Tests
{
    // ODD S43 / S44, T43: Inicio keeps the menu on the left, aligned, over a rotation of the four cultures.
    public class StartScreenTests
    {
        [Test]
        public void SlideShow_HoldsEachSlideThenCrossFadesToTheNext()
        {
            Assert.AreEqual(0, SlideShow.Index(0f, 4, 6f, 1.5f));
            Assert.AreEqual(0f, SlideShow.Blend(3f, 6f, 1.5f), 0.0001f);
            Assert.AreEqual(0.5f, SlideShow.Blend(6.75f, 6f, 1.5f), 0.0001f);
            Assert.AreEqual(1, SlideShow.Index(7.6f, 4, 6f, 1.5f));
            Assert.AreEqual(0f, SlideShow.Blend(7.6f, 6f, 1.5f), 0.05f);
            Assert.AreEqual(0, SlideShow.Index(4 * 7.5f + 1f, 4, 6f, 1.5f), "wraps after the last culture");
        }

        [Test]
        public void Inicio_RotatesTheFourCulturesInOrder()
        {
            EditorSceneManager.OpenScene("Assets/Integrante4/Scenes/Inicio.unity");
            var cycle = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Single(m => m.GetType().Name == "BackgroundCycle");
            var slides = (Sprite[])cycle.GetType().GetField("slides").GetValue(cycle);
            CollectionAssert.AreEqual(new[] { "Fondo_Mesopotamia", "Fondo_Egipto", "Fondo_Grecia", "Fondo_China" }, slides.Select(s => s.name).ToArray());
            foreach (string field in new[] { "front", "back", "caption" })
                Assert.IsNotNull(cycle.GetType().GetField(field).GetValue(cycle), field);
            Assert.AreEqual(4, ((Image[])cycle.GetType().GetField("dots").GetValue(cycle)).Length);
        }

        [Test]
        public void Inicio_MenuSharesOneLeftEdgeWithAMargin()
        {
            EditorSceneManager.OpenScene("Assets/Integrante4/Scenes/Inicio.unity");
            string[] names = { "Titulo", "Linea", "Subtitulo", "BotonJugar", "BotonCreditos", "BotonSalir" };
            float[] lefts = names.Select(n => Left((RectTransform)GameObject.Find(n).transform)).ToArray();
            foreach (float left in lefts) Assert.AreEqual(lefts[0], left, 0.5f, "left edges differ: " + string.Join(", ", lefts));
            Assert.GreaterOrEqual(lefts[0], -960f + 100f, "margin from the screen edge");
            Assert.IsNull(GameObject.Find("Filo"), "no hard edge line across the art");
        }

        // Left edge in canvas units (1920 x 1080 reference, centre origin) for a centre-anchored rect.
        static float Left(RectTransform rt) => rt.anchoredPosition.x - rt.sizeDelta.x * rt.pivot.x;
    }
}
