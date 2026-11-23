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

    // Level 4 hands over to the epilogue until levels 5-8 (Integrante 3) are merged.
    static readonly string[] NextLevel = { "Nivel1_Mesopotamia", "Nivel2_Egipto", "Nivel3_Grecia", "Nivel4_China" };

    static readonly Color Ground = new Color(0.45f, 0.45f, 0.47f);
    static readonly Color Wall = new Color(0.58f, 0.52f, 0.46f);

    [MenuItem("UNIT-404/Integrante 4/Crear niveles 1 y 2 (bloques grises)")]
    public static void BuildAll()
    {
        BuildLevel1();
        BuildLevel2();
        BuildLevel3();
        BuildLevel4();
        FlowSceneBuilder.BuildFlowScenes();
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
        Tagged(root, "Estaca_1", "Obstaculo", PrimitiveType.Cube, new Vector3(60f, 1f, 0f), new Vector3(0.6f, 2f, 0.6f), false, new Color(0.6f, 0.25f, 0.2f));
        Tagged(root, "Estaca_2", "Obstaculo", PrimitiveType.Cube, new Vector3(112f, 1f, 0f), new Vector3(0.6f, 2f, 0.6f), false, new Color(0.6f, 0.25f, 0.2f));
        Ring(root, 64f, 6f); Ring(root, 132f, 13f);

        // 4. Cima del zigurat (x 120-160): second checkpoint and Tiamat arena
        Solid(root, "Cima", 140f, 9f, 40f, 2f);
        Tagged(root, "Checkpoint_2", "Checkpoint", PrimitiveType.Cube, new Vector3(124f, 11f, 0f), new Vector3(1f, 4f, 1f), true);
        Tagged(root, "Tiamat", "Jefe", PrimitiveType.Capsule, new Vector3(150f, 12.5f, 0f), new Vector3(3f, 3f, 3f), false);
        Pickups(root, "Fragmento", 126f, 156f, 3, 11.5f);

        // 5. Templo superior (x 160-190): arrival portal
        Solid(root, "Templo", 175f, 9f, 30f, 2f);
        Tagged(root, "PortalLlegada", "Plataforma", PrimitiveType.Cube, new Vector3(185f, 12f, 0f), new Vector3(2f, 4f, 1f), true);
        Pickups(root, "Fragmento", 164f, 182f, 3, 11.5f);

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
        Ring(root, 102f, 8f); Ring(root, 62f, 6f); Ring(root, 142f, 6f);

        // 4. Camara del Duat (x 120-160): second checkpoint and Apophis arena
        Solid(root, "CamaraDuat", 140f, -0.5f, 40f, 1f);
        Tagged(root, "Checkpoint_2", "Checkpoint", PrimitiveType.Cube, new Vector3(124f, 2f, 0f), new Vector3(1f, 4f, 1f), true);
        Tagged(root, "Apofis", "Jefe", PrimitiveType.Capsule, new Vector3(150f, 3f, 0f), new Vector3(3f, 3f, 3f), false);
        Pickups(root, "Fragmento", 126f, 156f, 3, 1.5f);
        Floor(root, "Suelo_Duat", 120f, 160f, 0f);

        // 5. Santuario (x 160-190): arrival portal
        Floor(root, "Suelo_Santuario", 160f, 190f, 0f);
        Tagged(root, "PortalLlegada", "Plataforma", PrimitiveType.Cube, new Vector3(185f, 3f, 0f), new Vector3(2f, 4f, 1f), true);
        Pickups(root, "Fragmento", 164f, 182f, 3, 1.5f);

        Glyphs(root, new[] { 8f, 22f, 46f, 70f, 94f, 112f, 136f, 168f }, new[] { 2f, 8f, 2f, 7f, 9f, 9f, 2f, 2f });
        Depth(root, new Color(0.25f, 0.3f, 0.35f), "Nilo", 190f);
        Finish(scene, new Vector3(2f, 2f, 0f), "Nivel2_Egipto");
    }

    // ---------------- Level 3: Grecia (3D flight from here on) ----------------
    public static void BuildLevel3()
    {
        Scene scene = NewLevel("Nivel3_Grecia", new Color(0.55f, 0.72f, 0.85f));
        Transform root = new GameObject("Nivel").transform;

        // 1. Puerto (x 0-40): flight tutorial over ships, free depth
        Floor(root, "Muelle", 0f, 12f, 0f);
        for (int i = 0; i < 4; i++) Solid(root, "Barco_" + (i + 1), 16f + i * 6f, 0.5f, 4f, 1f, Wall, (i % 2 == 0 ? -3f : 3f));
        Pickups(root, "Fragmento", 14f, 38f, 5, 3f);
        Ring(root, 22f, 4f); Ring(root, 30f, 5f); Ring(root, 38f, 6f);

        // 2. Agora (x 40-80): first checkpoint, first hoplites
        Floor(root, "Suelo_Agora", 40f, 80f, 0f);
        Guard(root, "Hoplita_1", 52f, 1.5f); Guard(root, "Hoplita_2", 66f, 1.5f);
        Solid(root, "Columna_Agora", 58f, 3f, 1.5f, 6f, Wall);
        Pickups(root, "Fragmento", 42f, 78f, 4, 5f);
        Tagged(root, "Checkpoint_1", "Checkpoint", PrimitiveType.Cube, new Vector3(78f, 2f, 0f), new Vector3(1f, 4f, 1f), true);

        // 3. Ladera (x 80-120): rolling rocks and falling columns
        Rampa(root, "Ladera", 100f, 0f, 40f, 8f);
        for (int i = 0; i < 3; i++) Mover(root, "Roca_" + (i + 1), "Obstaculo", PrimitiveType.Sphere, new Vector3(90f + i * 10f, 2f + i * 1.5f, 0f), Vector3.one * 2f, Vector3.right * 6f, 3f);
        for (int i = 0; i < 2; i++) Mover(root, "ColumnaCaida_" + (i + 1), "Obstaculo", PrimitiveType.Cylinder, new Vector3(96f + i * 14f, 8f, 0f), new Vector3(1f, 3f, 1f), Vector3.down * 5f, 2f);
        Wind(root, "BrisaMar", 100f, 6f);
        Pickups(root, "Fragmento", 84f, 118f, 3, 6f);

        // 4. Templo en ruinas (x 120-160): second checkpoint and Cronos arena
        Solid(root, "Ruinas", 140f, 9f, 40f, 2f);
        Tagged(root, "Checkpoint_2", "Checkpoint", PrimitiveType.Cube, new Vector3(124f, 11f, 0f), new Vector3(1f, 4f, 1f), true);
        Tagged(root, "Cronos", "Jefe", PrimitiveType.Capsule, new Vector3(150f, 12.5f, 0f), new Vector3(3f, 3f, 3f), false);
        Pickups(root, "Fragmento", 126f, 156f, 3, 11.5f);
        Ring(root, 62f, 6f); Ring(root, 110f, 8f); Ring(root, 134f, 14f);

        // 5. Altar (x 160-190): arrival portal
        Solid(root, "Altar", 175f, 9f, 30f, 2f);
        Tagged(root, "PortalLlegada", "Plataforma", PrimitiveType.Cube, new Vector3(185f, 12f, 0f), new Vector3(2f, 4f, 1f), true);
        Pickups(root, "Fragmento", 164f, 182f, 3, 11.5f);

        Glyphs(root, new[] { 10f, 26f, 46f, 70f, 94f, 112f, 136f, 168f }, new[] { 3f, 5f, 2f, 7f, 8f, 9f, 11.5f, 11.5f }, "Inscripcion");
        Depth(root, new Color(0.5f, 0.55f, 0.4f), "ColinasTaladas", 190f);
        SidePath(root, "Agora", 60f, 0f, 9f); SidePath(root, "Ladera", 100f, 4f, -9f); SidePath(root, "Templo", 140f, 9f, 9f);
        Finish(scene, new Vector3(2f, 2f, 0f), "Nivel3_Grecia", true);
    }

    // ---------------- Level 4: China (3D flight) ----------------
    public static void BuildLevel4()
    {
        Scene scene = NewLevel("Nivel4_China", new Color(0.82f, 0.74f, 0.5f));
        Transform root = new GameObject("Nivel").transform;

        // 1. Paso de montaña (x 0-40): flight between hillsides to the wall
        Solid(root, "Ladera_Izq", 20f, 6f, 40f, 12f, Ground, -9f);
        Solid(root, "Ladera_Der", 20f, 6f, 40f, 12f, Ground, 9f);
        Floor(root, "Suelo_Paso", 0f, 40f, 0f);
        Pickups(root, "Fragmento", 4f, 38f, 5, 4f);
        Ring(root, 14f, 5f); Ring(root, 26f, 7f); Ring(root, 38f, 8f);

        // 2. Primera torre (x 40-80): first checkpoint, crossbow soldiers on towers
        Floor(root, "Suelo_Torre", 40f, 80f, 0f);
        Solid(root, "Torre_1", 50f, 4f, 4f, 8f, Wall);
        Solid(root, "Torre_2", 70f, 4f, 4f, 8f, Wall);
        Guard(root, "Ballestero_1", 50f, 9f); Guard(root, "Ballestero_2", 70f, 9f);
        Pickups(root, "Fragmento", 42f, 78f, 4, 5f);
        Tagged(root, "Checkpoint_1", "Checkpoint", PrimitiveType.Cube, new Vector3(78f, 2f, 0f), new Vector3(1f, 4f, 1f), true);

        // 3. Adarve de la muralla (x 80-120): carts and dust storm
        Solid(root, "Adarve", 100f, 5f, 40f, 2f);
        for (int i = 0; i < 3; i++) Mover(root, "Carreta_" + (i + 1), "Obstaculo", PrimitiveType.Cube, new Vector3(88f + i * 12f, 7f, 0f), new Vector3(2f, 1.5f, 2f), Vector3.right * 5f, 2.5f);
        Tagged(root, "TormentaPolvo", "Contaminacion", PrimitiveType.Cube, new Vector3(108f, 8f, 0f), new Vector3(8f, 6f, 6f), true, new Color(0.8f, 0.7f, 0.3f));
        Pickups(root, "Fragmento", 84f, 118f, 3, 8f);

        // 4. Tramo ancho entre torres (x 120-160): second checkpoint and Chiyou arena
        Solid(root, "Tramo_Ancho", 140f, 5f, 40f, 2f);
        Tagged(root, "Checkpoint_2", "Checkpoint", PrimitiveType.Cube, new Vector3(124f, 8f, 0f), new Vector3(1f, 4f, 1f), true);
        Tagged(root, "Chiyou", "Jefe", PrimitiveType.Capsule, new Vector3(150f, 8.5f, 0f), new Vector3(3f, 3f, 3f), false);
        Solid(root, "Mastil_1", 130f, 9f, 0.5f, 8f, Wall);
        Pickups(root, "Fragmento", 126f, 156f, 3, 8f);
        Ring(root, 60f, 10f); Ring(root, 104f, 10f); Ring(root, 142f, 10f);

        // 5. Torre del estandarte (x 160-190): arrival portal
        Solid(root, "Torre_Estandarte", 180f, 6f, 8f, 12f, Wall);
        Tagged(root, "PortalLlegada", "Plataforma", PrimitiveType.Cube, new Vector3(180f, 13f, 0f), new Vector3(2f, 4f, 1f), true);
        Pickups(root, "Fragmento", 162f, 172f, 3, 8f);

        Glyphs(root, new[] { 10f, 30f, 48f, 72f, 96f, 114f, 138f, 166f }, new[] { 4f, 6f, 5f, 6f, 9f, 9f, 8f, 7f }, "TiraBambu");
        Depth(root, new Color(0.45f, 0.5f, 0.45f), "Montañas", 190f);
        SidePath(root, "Torre", 60f, 0f, -9f); SidePath(root, "Adarve", 100f, 5f, 9f); SidePath(root, "Tramo", 140f, 5f, -9f);
        Finish(scene, new Vector3(2f, 2f, 0f), "Nivel4_China", true);
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

    static void Finish(Scene scene, Vector3 start, string name, bool freeDepth = false)
    {
        new GameObject("PlayerStart").transform.position = start;

        var player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "JugadorTemporal";
        player.tag = "Player";
        player.layer = Layer("Player");
        player.transform.position = start;
        player.AddComponent<Rigidbody>();
        var placeholder = player.AddComponent<PlaceholderPlayer>();
        placeholder.allowDepth = freeDepth;
        if (freeDepth)
        {
            placeholder.cameraRelative = true;
            Object.DestroyImmediate(Camera.main.GetComponent<SideCameraFollow>());
            Camera.main.gameObject.AddComponent<ThirdPersonCamera>();
        }
        player.GetComponent<Renderer>().sharedMaterial = Mat(new Color(0.2f, 0.7f, 0.9f));

        ApplyPalette(name[5] - '0');

        var hud = AssetDatabase.LoadAssetAtPath<GameObject>(HudPath);
        if (hud != null) PrefabUtility.InstantiatePrefab(hud);
        new GameObject("HudEventSimulator (solo pruebas)").AddComponent<HudEventSimulator>();
        FlowSceneBuilder.AddPauseMenu();
        int level = name[5] - '0';
        FlowSceneBuilder.AddNarrator(level, level < 4 ? NextLevel[level] : "Despedida");

        EditorSceneManager.SaveScene(scene, Folder + name + ".unity");
    }

    // Culture look (odd/art-style-sheet.md): matte base on untagged scenery, emissive accent on pickups, rings, glyphs, checkpoints and the portal.
    static void ApplyPalette(int level)
    {
        ColorUtility.TryParseHtmlString(Unit404.Integrante4.Core.CulturePalette.BaseHex(level), out Color baseColor);
        ColorUtility.TryParseHtmlString(Unit404.Integrante4.Core.CulturePalette.AccentHex(level), out Color accent);
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            string tag = r.gameObject.tag;
            bool accented = tag == "Fragmento" || tag == "Propulsor" || tag == "Glifo" || tag == "Checkpoint" || r.gameObject.name == "PortalLlegada";
            if (accented)
            {
                var m = Mat(accent);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", accent * 1.5f);
                r.sharedMaterial = m;
            }
            else if (tag == "Untagged" && r.gameObject.layer == Layer("Entorno") && r.gameObject.name != "JugadorTemporal")
            {
                r.sharedMaterial = Mat(baseColor);
            }
        }
    }

    // Safe fallbacks: a missing layer maps to Default, a missing tag is skipped with a warning.
    static int Layer(string name)
    {
        int index = LayerMask.NameToLayer(name);
        if (index < 0) { Debug.LogWarning("Layer '" + name + "' not found, using Default"); return 0; }
        return index;
    }

    static void SetTag(GameObject go, string tag)
    {
        if (System.Array.IndexOf(UnityEditorInternal.InternalEditorUtility.tags, tag) < 0)
        {
            Debug.LogWarning("Tag '" + tag + "' not found, '" + go.name + "' left untagged");
            return;
        }
        go.tag = tag;
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
        go.layer = Layer(layer);
        go.GetComponent<Renderer>().sharedMaterial = Mat(color);
        return go;
    }

    static void Solid(Transform p, string name, float x, float y, float w, float h, Color? color = null, float z = 0f)
        => Make(p, name, PrimitiveType.Cube, new Vector3(x, y, z), new Vector3(w, h, 2f), color ?? Ground, "Entorno");

    static void Mover(Transform p, string name, string tag, PrimitiveType type, Vector3 pos, Vector3 scale, Vector3 travel, float speed)
    {
        var go = Tagged(p, name, tag, type, pos, scale, false, new Color(0.7f, 0.3f, 0.3f));
        var mover = go.AddComponent<MovingObstacle>();
        mover.travel = travel;
        mover.speed = speed;
    }

    // Semi-open 3D worlds (levels 3-4): side platforms off the main line, each with fragments.
    static void SidePath(Transform p, string name, float x, float y, float z)
    {
        Make(p, "SenderoLateral_" + name, PrimitiveType.Cube, new Vector3(x, y - 0.5f, z), new Vector3(14f, 1f, 6f), Wall, "Entorno");
        for (int i = 0; i < 2; i++)
        {
            var go = Make(p, "Fragmento_Lateral_" + name + "_" + (i + 1), PrimitiveType.Sphere, new Vector3(x - 3f + i * 6f, y + 1.2f, z), Vector3.one * 0.6f, new Color(0.3f, 0.9f, 0.5f), "Coleccionable");
            SetTag(go, "Fragmento");
            go.GetComponent<Collider>().isTrigger = true;
        }
    }

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
        SetTag(go, tag);
        go.GetComponent<Collider>().isTrigger = trigger;
        return go;
    }

    static void Pickups(Transform p, string tag, float x0, float x1, int count, float y)
    {
        for (int i = 0; i < count; i++)
        {
            float x = count == 1 ? x0 : Mathf.Lerp(x0, x1, i / (float)(count - 1));
            var go = Make(p, tag + "_" + x.ToString("0"), PrimitiveType.Sphere, new Vector3(x, y, 0f), Vector3.one * 0.6f, new Color(0.3f, 0.9f, 0.5f), "Coleccionable");
            SetTag(go, tag);
            go.GetComponent<Collider>().isTrigger = true;
        }
    }

    static void Ring(Transform p, float x, float y)
    {
        var go = Make(p, "AnilloPropulsor_" + x.ToString("0"), PrimitiveType.Cylinder, new Vector3(x, y, 0f), new Vector3(2.5f, 0.15f, 2.5f), new Color(1f, 0.6f, 0.1f), "Coleccionable");
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        SetTag(go, "Propulsor");
        go.GetComponent<Collider>().isTrigger = true;
    }

    static void Guard(Transform p, string name, float x, float y)
        => Tagged(p, name, "Observador", PrimitiveType.Capsule, new Vector3(x, y, 0f), new Vector3(1f, 1.5f, 1f), false, new Color(0.8f, 0.2f, 0.2f)).layer = Layer("Enemigo");

    static void Smoke(Transform p, string name, float x, float y)
        => Tagged(p, name, "Contaminacion", PrimitiveType.Sphere, new Vector3(x, y, 0f), new Vector3(3f, 3f, 1f), true, new Color(0.4f, 0.4f, 0.45f));

    static void Wind(Transform p, string name, float x, float y)
    {
        var go = Make(p, name, PrimitiveType.Cube, new Vector3(x, y, 0f), new Vector3(8f, 6f, 1f), new Color(0.6f, 0.8f, 1f), "Entorno");
        go.GetComponent<Collider>().isTrigger = true;
        go.GetComponent<Renderer>().enabled = false;
    }

    static void Glyphs(Transform p, float[] xs, float[] ys, string label = "Glifo")
    {
        for (int i = 0; i < xs.Length; i++)
        {
            var go = Make(p, label + "_" + (i + 1), PrimitiveType.Cube, new Vector3(xs[i], ys[i], 0f), new Vector3(0.7f, 1f, 0.2f), new Color(0.95f, 0.9f, 0.6f), "Coleccionable");
            SetTag(go, "Glifo");
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
