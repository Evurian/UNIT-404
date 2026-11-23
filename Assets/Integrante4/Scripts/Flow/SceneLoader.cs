using UnityEngine;
using UnityEngine.SceneManagement;

// Routes every level change through the loading scene ("Carga").
public static class SceneLoader
{
    public const string LoadingScene = "Carga";
    public const string StartScene = "Inicio";
    public const string FirstLevel = "Nivel1_Mesopotamia";

    public static string Target { get; private set; } = FirstLevel;

    public static void Go(string sceneName)
    {
        Target = sceneName;
        Time.timeScale = 1f;
        SceneManager.LoadScene(LoadingScene);
    }
}
