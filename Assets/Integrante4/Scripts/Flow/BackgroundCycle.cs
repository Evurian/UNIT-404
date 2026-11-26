using TMPro;
using Unit404.Integrante4.Core;
using UnityEngine;
using UnityEngine.UI;

// Inicio background (ODD S44, T43): Mesopotamia -> Egipto -> Grecia -> China, cross-fading, with the culture named
// and a dot per culture lit in its accent colour.
public class BackgroundCycle : MonoBehaviour
{
    public Image front;
    public Image back;
    public Sprite[] slides;
    public string[] names;
    public Color[] accents;
    public TMP_Text caption;
    public Image[] dots;
    public float hold = 6f;
    public float fade = 1.5f;

    static readonly Color DotOff = new Color(0.86f, 0.77f, 0.58f, 0.35f);

    void Update() => Show(Time.unscaledTime);

    public void Show(float t)
    {
        if (slides == null || slides.Length == 0 || front == null || back == null) return;
        int n = slides.Length, i = SlideShow.Index(t, n, hold, fade), next = (i + 1) % n;
        float blend = SlideShow.Blend(t, hold, fade);
        front.sprite = slides[i];
        back.sprite = slides[next];
        back.color = new Color(1f, 1f, 1f, blend);
        int shown = blend > 0.5f ? next : i;
        if (caption != null && names != null && shown < names.Length)
        {
            caption.text = names[shown].ToUpperInvariant();
            if (accents != null && shown < accents.Length) caption.color = accents[shown];
        }
        if (dots != null)
            for (int d = 0; d < dots.Length; d++)
                dots[d].color = d == shown && accents != null && d < accents.Length ? accents[d] : DotOff;
    }
}
