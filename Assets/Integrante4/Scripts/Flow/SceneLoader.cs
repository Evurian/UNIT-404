using Unit404.Integrante4.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

// Routes every level change through the loading scene ("Carga").
public static class SceneLoader
{
    public const string LoadingScene = SceneFlow.Loading;
    public const string StartScene = SceneFlow.Start;
    public static readonly string FirstStoryScene = SceneFlow.Next(SceneFlow.Start);

    public static string Target { get; private set; } = FirstStoryScene;

    public static void Go(string sceneName)
    {
        Target = sceneName;
        Time.timeScale = 1f;
        SceneManager.LoadScene(LoadingScene);
    }
}
