using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class InsertarJugador
{
    const string PrefabPath = "Assets/Integrante1/Prefabs/UNIT-404.prefab";

    [MenuItem("UNIT-404/Insertar jugador en niveles")]
    public static void Run()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError("No se encuentra el prefab en " + PrefabPath);
            return;
        }

        foreach (string guid in AssetDatabase.FindAssets("t:Scene"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path);
            int level = LevelNumber(name);
            if (level < 1) continue;

            try
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                ProcessScene(scene, prefab, level);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[InsertarJugador] " + name + ": OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[InsertarJugador] " + name + ": " + e.Message);
            }
        }
    }

    static int LevelNumber(string sceneName)
    {
        for (int i = 1; i <= 8; i++)
            if (sceneName.StartsWith("Nivel" + i + "_")) return i;
        return 0;
    }

    static void ProcessScene(Scene scene, GameObject prefab, int level)
    {
        // Jugador
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            var start = GameObject.Find("PlayerStart");
            Vector3 pos = start != null ? start.transform.position : new Vector3(0f, 5f, 0f);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            go.transform.position = pos;
            player = go.GetComponent<PlayerController>();
        }

        // LevelSettings
        var settings = Object.FindFirstObjectByType<LevelSettings>();
        if (settings == null)
            settings = new GameObject("LevelSettings").AddComponent<LevelSettings>();
        settings.mode = level <= 2 ? FlightMode.Mode2_5D : FlightMode.Mode3D;

        // Camara
        Camera cam = Camera.main;
        if (cam == null) throw new System.Exception("No hay Main Camera");
        var follow = cam.GetComponent<CameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
        follow.target = player.transform;

        // Un solo AudioListener
        var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        for (int i = 0; i < listeners.Length; i++)
            if (listeners[i].gameObject != cam.gameObject) Object.DestroyImmediate(listeners[i]);
    }
}
