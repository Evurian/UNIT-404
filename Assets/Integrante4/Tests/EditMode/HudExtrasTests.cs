using NUnit.Framework;
using Unit404.Integrante4.Core;
using UnityEditor;
using UnityEngine;

namespace Unit404.Integrante4.Tests
{
    public class HudExtrasTests
    {
        [TestCase(3, 8, "3 / 8")]
        [TestCase(0, 8, "0 / 8")]
        [TestCase(12, 8, "8 / 8")]
        [TestCase(-1, 8, "0 / 8")]
        public void GlyphProgress_IsCollectedOverTotalClamped(int got, int total, string expected)
        {
            Assert.AreEqual(expected, HudExtras.FormatGlyphs(got, total));
        }

        [TestCase(5f, 10f, 0.5f)]
        [TestCase(0f, 10f, 0f)]
        [TestCase(12f, 10f, 1f)]
        [TestCase(-1f, 10f, 0f)]
        [TestCase(3f, 0f, 0f)]
        public void CooldownFraction_IsRemainingOverTotalClamped(float remaining, float total, float expected)
        {
            Assert.AreEqual(expected, HudExtras.CooldownFraction(remaining, total), 0.0001f);
        }

        [Test]
        public void ChipLabel_ShowsNameAndFuelCost()
        {
            Assert.AreEqual("Salto (2)", HudExtras.ChipLabel("Salto", 2));
            Assert.AreEqual("Vacio", HudExtras.ChipLabel(null, 0));
        }

        [TestCase(0f, 0f)]
        [TestCase(50f, 0.5f)]
        [TestCase(150f, 1f)]
        [TestCase(-5f, 0f)]
        public void AnomalyFraction_IsPercentClamped(float value, float expected)
        {
            Assert.AreEqual(expected, HudExtras.AnomalyFraction(value), 0.0001f);
        }

        [Test]
        public void HudPrefab_HasChipSlotsAnomalyLockOnMinimapAndAdaPanel()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Integrante4/Prefabs/HUD.prefab");
            Assert.IsNotNull(prefab);
            var view = prefab.GetComponent("HudExtrasView");
            Assert.IsNotNull(view, "HudExtrasView missing");
            var t = view.GetType();
            var slots = (System.Array)t.GetField("chipSlots").GetValue(view);
            Assert.AreEqual(3, slots.Length);
            foreach (string field in new[] { "glyphText", "anomalyFill", "lockOnMarker", "adaText", "minimapPanel" })
                Assert.IsNotNull(t.GetField(field).GetValue(view), field);
        }
    }
}
