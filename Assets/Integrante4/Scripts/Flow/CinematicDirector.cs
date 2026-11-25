using Unit404.Integrante4.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Culture transition (ODD S27, T34): flies the camera over the next culture's landscape, shows the title card and
// Ada's subtitles, and loads the next level in the background so it starts as soon as the shot ends.
// Space, Enter, Esc or a click skips.
public class CinematicDirector : MonoBehaviour
{
    public int level = 2;
    public string nextScene;
    public Transform[] path;
    public Transform[] lookAt;
    public CanvasGroup title;
    public Text subtitle;
    public Image fade;

    float t;
    bool leaving;
    float leaveAt;
    AsyncOperation load;
    float[] px, py, pz, lx, ly, lz;

    void Start()
    {
        Time.timeScale = 1f;
        px = Axis(path, 0); py = Axis(path, 1); pz = Axis(path, 2);
        lx = Axis(lookAt, 0); ly = Axis(lookAt, 1); lz = Axis(lookAt, 2);
        if (Application.CanStreamedLevelBeLoaded(nextScene))
        {
            load = SceneManager.LoadSceneAsync(nextScene);
            load.allowSceneActivation = false;
        }
        Apply(0f);
    }

    void Update()
    {
        t += Time.unscaledDeltaTime;
        if (!leaving && (t >= CinematicScript.Duration || SkipPressed())) Leave();
        Apply(t);
        if (leaving && t >= leaveAt) Finish();
    }

    // Skipping jumps to the closing fade instead of cutting.
    void Leave()
    {
        leaving = true;
        float closing = CinematicScript.Duration - 0.8f;
        if (t < closing) t = closing;
        leaveAt = CinematicScript.Duration;
    }

    void Finish()
    {
        enabled = false;
        if (load != null) load.allowSceneActivation = true;
        else SceneLoader.Go(nextScene);
    }

    void Apply(float time)
    {
        float u = CinematicPath.Ease(time / CinematicScript.Duration);
        Camera cam = Camera.main;
        if (cam != null && path != null && path.Length > 0)
        {
            var pos = new Vector3(CinematicPath.CatmullRom(px, u), CinematicPath.CatmullRom(py, u), CinematicPath.CatmullRom(pz, u));
            var target = new Vector3(CinematicPath.CatmullRom(lx, u), CinematicPath.CatmullRom(ly, u), CinematicPath.CatmullRom(lz, u));
            cam.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(target - pos, Vector3.up));
        }
        if (title != null) title.alpha = CinematicScript.TitleAlpha(time);
        if (subtitle != null)
        {
            int i = CinematicScript.SubtitleIndex(level, time);
            subtitle.text = i < 0 ? "" : CinematicScript.Subtitles(level)[i];
        }
        if (fade != null)
        {
            Color c = fade.color;
            fade.color = new Color(c.r, c.g, c.b, CinematicScript.FadeAlpha(time));
        }
    }

    static float[] Axis(Transform[] keys, int axis)
    {
        if (keys == null || keys.Length == 0) return new[] { 0f };
        var values = new float[keys.Length];
        for (int i = 0; i < keys.Length; i++) values[i] = keys[i].position[axis];
        return values;
    }

    static bool SkipPressed() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)
        || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0);
}
