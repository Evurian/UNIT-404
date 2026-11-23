using Unit404.Integrante4.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

// Pause menu (Esc): freezes time, resume / restart level / back to start.
public class PauseMenu : MonoBehaviour
{
    public GameObject panel;
    readonly PauseState state = new PauseState();

    void Start() => Apply();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { state.Toggle(); Apply(); }
    }

    void OnDisable() => Time.timeScale = 1f;

    public void Resume() { state.SetPaused(false); Apply(); }
    public void Restart() => SceneLoader.Go(SceneManager.GetActiveScene().name);
    public void ToStart() => SceneLoader.Go(SceneLoader.StartScene);

    void Apply()
    {
        Time.timeScale = state.TimeScale;
        if (panel != null) panel.SetActive(state.Paused);
    }
}
