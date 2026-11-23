using System.Collections;
using Unit404.Integrante4.Core;
using UnityEngine;
using UnityEngine.UI;

// Full HUD (GDD 10): glyph progress, 3 chip slots, anomaly bar, lock-on mark, minimap and Ada panel.
// GameEvents has no events for these yet, so gameplay code (or the simulator) calls the public methods.
public class HudExtrasView : MonoBehaviour
{
    [System.Serializable]
    public class ChipSlot
    {
        public Text label;
        public Image cooldownOverlay;
    }

    public Text glyphText;
    public ChipSlot[] chipSlots = new ChipSlot[3];
    public RectTransform anomalyFill;
    public GameObject lockOnMarker;
    public GameObject minimapPanel;
    public Text adaText;

    void OnEnable()
    {
        ShowGlyphs(0, 8);
        for (int i = 0; i < chipSlots.Length; i++) SetChip(i, null, 0, 0f, 0f);
        SetAnomaly(0f);
        SetLockOn(false);
        SetAdaMessage("Ada: Sin mision activa.");
    }

    public void ShowGlyphs(int collected, int total)
    {
        if (glyphText != null) glyphText.text = "Glifos " + HudExtras.FormatGlyphs(collected, total);
    }

    public void SetChip(int slot, string chipName, int fuelCost, float cooldownRemaining, float cooldownTotal)
    {
        if (slot < 0 || slot >= chipSlots.Length || chipSlots[slot] == null) return;
        ChipSlot s = chipSlots[slot];
        if (s.label != null) s.label.text = HudExtras.ChipLabel(chipName, fuelCost);
        if (s.cooldownOverlay != null) s.cooldownOverlay.fillAmount = HudExtras.CooldownFraction(cooldownRemaining, cooldownTotal);
    }

    // New chip animation: the slot label pulses once (week 14).
    public void PlayChipObtained(int slot)
    {
        if (slot < 0 || slot >= chipSlots.Length || chipSlots[slot] == null || chipSlots[slot].label == null) return;
        StartCoroutine(Pulse(chipSlots[slot].label.rectTransform));
    }

    IEnumerator Pulse(RectTransform target)
    {
        const float duration = 0.6f;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            target.localScale = Vector3.one * UiAnimation.ChipPulse(t, duration, 1.4f);
            yield return null;
        }
        target.localScale = Vector3.one;
    }

    public void SetAnomaly(float percent)
    {
        if (anomalyFill == null) return;
        Vector2 max = anomalyFill.anchorMax;
        max.x = HudExtras.AnomalyFraction(percent);
        anomalyFill.anchorMax = max;
    }

    public void SetLockOn(bool active)
    {
        if (lockOnMarker != null) lockOnMarker.SetActive(active);
    }

    public void SetAdaMessage(string message)
    {
        if (adaText != null) adaText.text = message;
    }
}
