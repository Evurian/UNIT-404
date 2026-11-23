using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Generates the grey-box scenes of levels 1 and 2 (final art arrives in week 14).
// Everything gameplay-related sits on Z = 0; only decorative depth layers use Z > 0.
public static class LevelSceneBuilder
{
    const string Folder = "Assets/Integrante4/Scenes/";
    const string HudPath = "Assets/Integrante4/Prefabs/HUD.prefab";

    static readonly Color Ground = new Color(0.45f, 0.45f, 0.47f);
    static readonly Color Wall = new Color(0.58f, 0.52f, 0.46f);

    [MenuItem("UNIT-404/Integrante 4/Crear niveles 1 y 2 (bloques grises)")]
    public static void BuildAll()
    {
        BuildLevel1();
        BuildLevel2();
        AssetDatabase.SaveAssets();
    }

    // ---------------- Level 1: Mesopotamia ----------------
    public static void BuildLevel1()
    {
        Scene scene = NewLevel("Nivel1_Mesopotamia", new Color(0.78f, 0.62f, 0.38f));
        Transform root = new GameObject("Nivel").transform;

        // 1. Orillas del rio (x 0-40): first thrusts, landings, Propulsor rings
        Floor(root, "Suelo_Orilla", 0f, 40f, 0f);
        Pickups(root, "Fragmento", 4f, 36f, 5, 1.5f);
        Ring(root, 12f, 5f); Ring(root, 20f, 7f); Ring(root, 28f, 6f); Ring(root, 35f, 9f);
        Solid(root, "Terraza_Orilla", 24f, 3f, 6f, 1f);

        // 2. Calles de la ciudad (x 40-80): first checkpoint and first guards
        Floor(root, "Suelo_Calle", 40f, 80f, 0f);
        Solid(root, "Muro_Adobe_1", 52f, 2f, 2f, 4f, Wall);
        Solid(root, "Muro_Adobe_2", 66f, 2f, 2f, 4f, Wall);
        Rampa(root, "Rampa_Calle", 44f, 0f, 8f, 3f);
        Guard(root, "Guardia_1", 58f, 1.5f); Guard(root, "Guardia_2", 72f, 1.5f);
        Pickups(root, "Fragmento", 42f, 78f, 4, 5f);
        Tagged(root, "Checkpoint_1", "Checkpoint", PrimitiveType.Cube, new Vector3(78f, 2f, 0f), new Vector3(1f, 4f, 1f), true);

        // 3. Escalinata del zigurat (x 80-120): terraces with side wind
        for (int i = 0; i < 5; i++) Solid(root, "Terraza_" + (i + 1), 84f + i * 7f, 1f + i * 2f, 6f, 2f);
        Floor(root, "Suelo_Escalinata", 80f, 120f, 0f);
        Wind(root, "Viento_1", 92f, 7f); Wind(root, "Viento_2", 106f, 7f);
        Smoke(root, "Incienso_1", 96f, 4f); Smoke(root, "Incienso_2", 110f, 8f);
        Pickups(root, "Fragmento", 86f, 118f, 3, 8f);
        Ring(root, 100f, 9f);

        // 4. Cima del zigurat (x 120-160): second checkpoint and Tiamat arena
        Solid(root, "Cima", 140f, 9f, 40f, 2f);
        Tagged(root, "Checkpoint_2", "Checkpoint", PrimitiveType.Cube, new Vector3(124f, 11f, 0f), new Vector3(1f, 4f, 1f), true);
        Tagged(root, "Tiamat", "Jefe", PrimitiveType.Capsule, new Vector3(150f, 12.5f, 0f), new Vector3(3f, 3f, 3f), false);
        Pickups(root, "Fragmento", 126f, 156f, 3, 11.5f);

        // 5. Templo superior (x 160-190): arrival portal
        Solid(root, "Templo", 175f, 9f, 30f, 2f);
        Tagged(root, "PortalLlegada", "Plataforma", PrimitiveType.Cube, new Vector3(185f, 12f, 0f), new Vector3(2f, 4f, 1f), true);

        Glyphs(root, new[] { 8f, 22f, 46f, 70f, 94f, 112f, 136f, 168f }, new[] { 2f, 8f, 2f, 7f, 10f, 9f, 11.5f, 11.5f });
        Depth(root, new Color(0.5f, 0.42f, 0.3f), "CamposSalados", 190f);
        Finish(scene, new Vector3(2f, 2f, 0f), "Nivel1_Mesopotamia");
    }

    // ---------------- Level 2: Egipto ----------------
    public static void BuildLevel2()
    {
        Scene scene = NewLevel("Nivel2_Egipto", new Color(0.3f, 0.26f, 0.2f));
        Transform root = new GameObject("Nivel").transform;

        // 1. Muelle del Nilo (x 0-40)
        Floor(root, "Suelo_Muelle", 0f, 40f, 0f);
        Pickups(root, "Fragmento", 4f, 36f, 5, 1.5f);
        Ring(root, 14f, 5f); Ring(root, 24f, 7f); Ring(root, 33f, 6f); Ring(root, 38f, 9f);
        Solid(root, "Bloque_Piedra_1", 20f, 1.5f, 3f, 3f, Wall);

        // 2. Patio del templo (x 40-80): first checkpoint, priests and archers
        Floor(root, "Suelo_Patio", 40f, 80f, 0f);
        Solid(root, "Bloque_Piedra_2", 55f, 2f, 3f, 4f, Wall);
        Solid(root, "Bloque_Piedra_3", 68f, 1.5f, 3f, 3f, Wall);
        Guard(root, "Sacerdote_1", 50f, 1.5f); Guard(root, "Arquero_1", 62f, 5f); Guard(root, "Sacerdote_2", 74f, 1.5f);
        Pickups(root, "Fragmento", 42f, 78f, 4, 5f);
        Tagged(root, "Checkpoint_1", "Checkpoint", PrimitiveType.Cube, new Vector3(78f, 2f, 0f), new Vector3(1f, 4f, 1f), true);

        // 3. Sala de columnas (x 80-120): dark stretch lit by torches
        Floor(root, "Suelo_Sala", 80f, 120f, 0f);
        for (int i = 0; i < 6; i++) Solid(root, "Columna_" + (i + 1), 86f + i * 6f, 4f, 1.5f, 8f, Wall);
        for (int i = 0; i < 4; i++) Tagged(root, "Antorcha_" + (i + 1), "Obstaculo", PrimitiveType.Cube, new Vector3(89f + i * 9f, 3f, 0f), new Vector3(0.6f, 1.2f, 0.6f), false, new Color(0.9f, 0.45f, 0.1f));
        Pickups(root, "Fragmento", 84f, 118f, 3, 8f);
        Ring(root, 102f, 8f);

        // 4. Camara del Duat (x 120-160): second checkpoint and Apophis arena
        Solid(root, "CamaraDuat", 140f, -0.5f, 40f, 1f);
        Tagged(root, "Checkpoint_2", "Checkpoint", PrimitiveType.Cube, new Vector3(124f, 2f, 0f), new Vector3(1f, 4f, 1f), true);
        Tagged(root, "Apofis", "Jefe", PrimitiveType.Capsule, new Vector3(150f, 3f, 0f), new Vector3(3f, 3f, 3f), false);
        Pickups(root, "Fragmento", 126f, 156f, 3, 1.5f);
        Floor(root, "Suelo_Duat", 120f, 160f, 0f);

        // 5. Santuario (x 160-190): arrival portal
        Floor(root, "Suelo_Santuario", 160f, 190f, 0f);
        Tagged(root, "PortalLlegada", "Plataforma", PrimitiveType.Cube, new Vector3(185f, 3f, 0f), new Vector3(2f, 4f, 1f), true);

        Glyphs(root, new[] { 8f, 22f, 46f, 70f, 94f, 112f, 136f, 168f }, new[] { 2f, 8f, 2f, 7f, 9f, 9f, 2f, 2f });
        Depth(root, new Color(0.25f, 0.3f, 0.35f), "Nilo", 190f);
        Finish(scene, new Vector3(2f, 2f, 0f), "Nivel2_Egipto");
    }

    // ---------------- helpers ----------------
    static Scene NewLevel(string name, Color sky)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var cam = Camera.main;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = sky;
        cam.transform.position = new Vector3(0f, 4f, -16f);
        cam.transform.rotation = Quaternion.identity;
        cam.gameObject.AddComponent<SideCameraFollow>();
        return scene;
    }

    static void Finish(Scene scene, Vector3 start, string name)
    {
        new GameObject("PlayerStart").transform.position = start;

        var player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "JugadorTemporal";
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Player");
        player.transform.position = start;
        player.AddComponent<Rigidbody>();
        player.AddComponent<PlaceholderPlayer>();
        player.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.2f, 0.7f, 0.9f));

        var hud = AssetDatabase.LoadAssetAtPath<GameObject>(HudPath);
        if (hud != null) PrefabUtility.InstantiatePrefab(hud);
        new GameObject("HudEventSimulator (solo pruebas)").AddComponent<HudEventSimulator>();

        EditorSceneManager.SaveScene(scene, Folder + name + ".unity");
    }

    static Material Mat(Color c)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        m.color = c;
        return m;
    }

    static GameObject Make(Transform parent, string name, PrimitiveType type, Vector3 pos, Vector3 scale, Color color, string layer)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.layer = LayerMask.NameToLayer(layer);
        go.GetComponent<Renderer>().sharedMaterial = Mat(color);
        return go;
    }

    static void Solid(Transform p, string name, float x, float y, float w, float h, Color? color = null)
        => Make(p, name, PrimitiveType.Cube, new Vector3(x, y, 0f), new Vector3(w, h, 2f), color ?? Ground, "Entorno");

    static void Floor(Transform p, string name, float x0, float x1, float y)
        => Solid(p, name, (x0 + x1) / 2f, y - 0.5f, x1 - x0, 1f);

    static void Rampa(Transform p, string name, float x, float y, float w, float h)
    {
        var go = Make(p, name, PrimitiveType.Cube, new Vector3(x, y + h / 2f, 0f), new Vector3(w, 0.5f, 2f), Wall, "Entorno");
        go.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(h, w) * Mathf.Rad2Deg);
    }

    static GameObject Tagged(Transform p, string name, string tag, PrimitiveType type, Vector3 pos, Vector3 scale, bool trigger, Color? color = null)
    {
        var go = Make(p, name, type, pos, scale, color ?? new Color(0.9f, 0.85f, 0.4f), "Entorno");
        go.tag = tag;
        go.GetComponent<Collider>().isTrigger = trigger;
        return go;
    }

    static void Pickups(Transform p, string tag, float x0, float x1, int count, float y)
    {
        for (int i = 0; i < count; i++)
        {
            float x = count == 1 ? x0 : Mathf.Lerp(x0, x1, i / (float)(count - 1));
            var go = Make(p, tag + "_" + x.ToString("0"), PrimitiveType.Sphere, new Vector3(x, y, 0f), Vector3.one * 0.6f, new Color(0.3f, 0.9f, 0.5f), "Coleccionable");
            go.tag = tag;
            go.GetComponent<Collider>().isTrigger = true;
        }
    }

    static void Ring(Transform p, float x, float y)
    {
        var go = Make(p, "AnilloPropulsor_" + x.ToString("0"), PrimitiveType.Cylinder, new Vector3(x, y, 0f), new Vector3(2.5f, 0.15f, 2.5f), new Color(1f, 0.6f, 0.1f), "Coleccionable");
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        go.tag = "Propulsor";
        go.GetComponent<Collider>().isTrigger = true;
    }

    static void Guard(Transform p, string name, float x, float y)
        => Tagged(p, name, "Observador", PrimitiveType.Capsule, new Vector3(x, y, 0f), new Vector3(1f, 1.5f, 1f), false, new Color(0.8f, 0.2f, 0.2f)).layer = LayerMask.NameToLayer("Enemigo");

    static void Smoke(Transform p, string name, float x, float y)
        => Tagged(p, name, "Contaminacion", PrimitiveType.Sphere, new Vector3(x, y, 0f), new Vector3(3f, 3f, 1f), true, new Color(0.4f, 0.4f, 0.45f));

    static void Wind(Transform p, string name, float x, float y)
    {
        var go = Make(p, name, PrimitiveType.Cube, new Vector3(x, y, 0f), new Vector3(8f, 6f, 1f), new Color(0.6f, 0.8f, 1f), "Entorno");
        go.GetComponent<Collider>().isTrigger = true;
        go.GetComponent<Renderer>().enabled = false;
    }

    static void Glyphs(Transform p, float[] xs, float[] ys)
    {
        for (int i = 0; i < xs.Length; i++)
        {
            var go = Make(p, "Glifo_" + (i + 1), PrimitiveType.Cube, new Vector3(xs[i], ys[i], 0f), new Vector3(0.7f, 1f, 0.2f), new Color(0.95f, 0.9f, 0.6f), "Coleccionable");
            go.tag = "Glifo";
            go.GetComponent<Collider>().isTrigger = true;
        }
    }

    // Decorative depth layers at Z > 0 (no collider, no gameplay tag).
    static void Depth(Transform p, Color color, string name, float length)
    {
        for (int i = 0; i < 3; i++)
        {
            var go = Make(p, "Fondo_" + name + "_" + (i + 1), PrimitiveType.Cube, new Vector3(length / 2f, 3f + i * 2f, 12f + i * 10f), new Vector3(length * 1.2f, 6f + i * 3f, 1f), Color.Lerp(color, Color.gray, i * 0.25f), "Default");
            Object.DestroyImmediate(go.GetComponent<Collider>());
        }
    }
}
