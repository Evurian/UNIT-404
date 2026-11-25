using UnityEngine;

// Slow push-in and drift on a screen's background art (T35), so menus feel alive without distracting.
public class ScreenMotion : MonoBehaviour
{
    public float zoom = 0.06f;
    public float drift = 18f;
    public float period = 24f;

    RectTransform rect;
    Vector2 origin;

    void Awake()
    {
        rect = (RectTransform)transform;
        origin = rect.anchoredPosition;
    }

    void Update()
    {
        float phase = Mathf.PingPong(Time.unscaledTime / period, 1f);
        float eased = phase * phase * (3f - 2f * phase);
        rect.localScale = Vector3.one * (1f + zoom * eased);
        rect.anchoredPosition = origin + new Vector2(-drift * eased, 0f);
    }
}
