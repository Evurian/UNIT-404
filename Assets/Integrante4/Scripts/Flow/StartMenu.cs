using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public void Play() => SceneLoader.Go(SceneLoader.FirstLevel);

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
