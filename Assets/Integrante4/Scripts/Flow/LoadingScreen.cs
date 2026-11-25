using System.Collections;
using Unit404.Integrante4.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

// Loading scene: LoadSceneAsync with a progress bar, activating the target once ready.
public class LoadingScreen : MonoBehaviour
{
    public RectTransform barFill;
    public TMP_Text percentText;
    public TMP_Text titleText;
    public Image background;
    public Sprite[] cultureBackgrounds; // 0 = default, 1-4 = the culture of the scene being loaded (T35)
    public float minSeconds = 1.5f;

    IEnumerator Start()
    {
        string target = SceneLoader.Target;
        if (titleText != null) titleText.text = ScreenArt.LoadingTitle(target);
        int culture = ScreenArt.CultureOf(target);
        if (background != null && cultureBackgrounds != null && culture < cultureBackgrounds.Length && cultureBackgrounds[culture] != null)
            background.sprite = cultureBackgrounds[culture];

        AsyncOperation op = SceneManager.LoadSceneAsync(target);
        op.allowSceneActivation = false;
        float elapsed = 0f;
        while (!LoadProgress.CanActivate(op.progress, elapsed, minSeconds))
        {
            elapsed += Time.unscaledDeltaTime;
            Show(op.progress);
            yield return null;
        }
        Show(LoadProgress.ReadyThreshold);
        op.allowSceneActivation = true;
    }

    void Show(float raw)
    {
        if (percentText != null) percentText.text = LoadProgress.Format(raw);
        if (barFill != null)
        {
            Vector2 max = barFill.anchorMax;
            max.x = LoadProgress.Normalize(raw);
            barFill.anchorMax = max;
        }
    }
}
