using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Golden seal badge that slides in from the top-right corner for 3 seconds (GDD 11.6).
public class AchievementToast : MonoBehaviour
{
    public RectTransform badge;
    public Text label;
    public float visibleSeconds = 3f;
    public float slideSeconds = 0.4f;

    Vector2 shownPos;

    void Awake()
    {
        if (badge == null) return;
        shownPos = badge.anchoredPosition;
        badge.gameObject.SetActive(false);
    }

    public void Show(string achievement)
    {
        if (badge == null) return;
        StopAllCoroutines();
        StartCoroutine(Run(achievement));
    }

    IEnumerator Run(string achievement)
    {
        if (label != null) label.text = achievement;
        badge.gameObject.SetActive(true);
        Vector2 hidden = shownPos + new Vector2(600f, 0f);
        for (float t = 0f; t < slideSeconds; t += Time.unscaledDeltaTime)
        {
            badge.anchoredPosition = Vector2.Lerp(hidden, shownPos, t / slideSeconds);
            yield return null;
        }
        badge.anchoredPosition = shownPos;
        yield return new WaitForSecondsRealtime(visibleSeconds);
        badge.gameObject.SetActive(false);
    }
}
