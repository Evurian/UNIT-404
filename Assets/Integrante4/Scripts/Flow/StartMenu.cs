using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public void Play() => SceneLoader.Go("Introduccion");
    public void ShowCredits() => SceneLoader.Go("Creditos");

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
