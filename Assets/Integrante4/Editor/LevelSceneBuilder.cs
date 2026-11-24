using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
        pathDepth = 2f;
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
        Walk(root, "Cima", 140f, 9f, 40f, 2f);
        Tagged(root, "Checkpoint_2", "Checkpoint", PrimitiveType.Cube, new Vector3(124f, 11f, 0f), new Vector3(1f, 4f, 1f), true);
        Tagged(root, "Tiamat", "Jefe", PrimitiveType.Capsule, new Vector3(150f, 12.5f, 0f), new Vector3(3f, 3f, 3f), false);
        Pickups(root, "Fragmento", 126f, 156f, 3, 11.5f);

        // 5. Templo superior (x 160-190): arrival portal
        Walk(root, "Templo", 175f, 9f, 30f, 2f);
        Portal(root, 185f, 10f);
        Pickups(root, "Fragmento", 164f, 182f, 3, 11.5f);

        Glyphs(root, new[] { 8f, 22f, 46f, 70f, 94f, 112f, 136f, 168f }, new[] { 2f, 8f, 2f, 7f, 10f, 9f, 11.5f, 11.5f });
        Depth(root, new Color(0.5f, 0.42f, 0.3f), "CamposSalados", 190f);
        Finish(scene, new Vector3(2f, 2f, 0f), "Nivel1_Mesopotamia");
    }

    // ---------------- Level 2: Egipto ----------------
    public static void BuildLevel2()
    {
        pathDepth = 2f;
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
        Walk(root, "CamaraDuat", 140f, -0.5f, 40f, 1f);
        Tagged(root, "Checkpoint_2", "Checkpoint", PrimitiveType.Cube, new Vector3(124f, 2f, 0f), new Vector3(1f, 4f, 1f), true);
        Tagged(root, "Apofis", "Jefe", PrimitiveType.Capsule, new Vector3(150f, 3f, 0f), new Vector3(3f, 3f, 3f), false);
        Pickups(root, "Fragmento", 126f, 156f, 3, 1.5f);
        Floor(root, "Suelo_Duat", 120f, 160f, 0f);

        // 5. Santuario (x 160-190): arrival portal
        Floor(root, "Suelo_Santuario", 160f, 190f, 0f);
        Portal(root, 185f, 0f);
        Pickups(root, "Fragmento", 164f, 182f, 3, 1.5f);

        Glyphs(root, new[] { 8f, 22f, 46f, 70f, 94f, 112f, 136f, 168f }, new[] { 2f, 8f, 2f, 7f, 9f, 9f, 2f, 2f });
        Depth(root, new Color(0.25f, 0.3f, 0.35f), "Nilo", 190f);
        Finish(scene, new Vector3(2f, 2f, 0f), "Nivel2_Egipto");
    }

    // ---------------- Level 3: Grecia (3D flight from here on) ----------------
    public static void BuildLevel3()
    {
        pathDepth = 8f;
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
        Rampa(root, "Ladera", 100f, 0f, 40f, 10f); // ends level with the Ruinas floor (top 10), no step (ODD S26)
        for (int i = 0; i < 3; i++) Mover(root, "Roca_" + (i + 1), "Obstaculo", PrimitiveType.Sphere, new Vector3(90f + i * 10f, 2f + i * 1.5f, 0f), Vector3.one * 2f, Vector3.right * 6f, 3f);
        for (int i = 0; i < 2; i++) Mover(root, "ColumnaCaida_" + (i + 1), "Obstaculo", PrimitiveType.Cylinder, new Vector3(96f + i * 14f, 8f, 0f), new Vector3(1f, 3f, 1f), Vector3.down * 5f, 2f);
        Wind(root, "BrisaMar", 100f, 6f);
        foreach (float fx in new[] { 84f, 101f, 118f }) Pickups(root, "Fragmento", fx, fx, 1, RampTop(fx, 100f, 0f, 40f, 10f) + 1.5f);

        // 4. Templo en ruinas (x 120-160): second checkpoint and Cronos arena
        Walk(root, "Ruinas", 140f, 9f, 40f, 2f);
        Tagged(root, "Checkpoint_2", "Checkpoint", PrimitiveType.Cube, new Vector3(124f, 11f, 0f), new Vector3(1f, 4f, 1f), true);
        Tagged(root, "Cronos", "Jefe", PrimitiveType.Capsule, new Vector3(150f, 12.5f, 0f), new Vector3(3f, 3f, 3f), false);
        Pickups(root, "Fragmento", 126f, 156f, 3, 11.5f);
        Ring(root, 62f, 6f); Ring(root, 110f, 10.5f); Ring(root, 134f, 14f);

        // 5. Altar (x 160-190): arrival portal
        Walk(root, "Altar", 175f, 9f, 30f, 2f);
        Portal(root, 185f, 10f);
        Pickups(root, "Fragmento", 164f, 182f, 3, 11.5f);

        Glyphs(root, new[] { 10f, 26f, 46f, 70f, 94f, 112f, 136f, 168f }, new[] { 3f, 5f, 2f, 7f, 8f, 9f, 11.5f, 11.5f }, "Inscripcion");
        Depth(root, new Color(0.5f, 0.55f, 0.4f), "ColinasTaladas", 190f);
        SidePath(root, "Agora", 60f, 0f, 9f); SidePath(root, "Ladera", 100f, 4f, -9f); SidePath(root, "Templo", 140f, 9f, 9f);
        Finish(scene, new Vector3(2f, 2f, 0f), "Nivel3_Grecia", true);
    }

    // ---------------- Level 4: China (3D flight) ----------------
    public static void BuildLevel4()
    {
        pathDepth = 8f;
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
        Walk(root, "Adarve", 100f, 5f, 40f, 2f);
        for (int i = 0; i < 3; i++) Mover(root, "Carreta_" + (i + 1), "Obstaculo", PrimitiveType.Cube, new Vector3(88f + i * 12f, 7f, 0f), new Vector3(2f, 1.5f, 2f), Vector3.right * 5f, 2.5f);
        Tagged(root, "TormentaPolvo", "Contaminacion", PrimitiveType.Cube, new Vector3(108f, 8f, 0f), new Vector3(8f, 6f, 6f), true, new Color(0.8f, 0.7f, 0.3f));
        Pickups(root, "Fragmento", 84f, 118f, 3, 8f);

        // 4. Tramo ancho entre torres (x 120-160): second checkpoint and Chiyou arena
        Walk(root, "Tramo_Ancho", 140f, 5f, 40f, 2f);
        Tagged(root, "Checkpoint_2", "Checkpoint", PrimitiveType.Cube, new Vector3(124f, 8f, 0f), new Vector3(1f, 4f, 1f), true);
        Tagged(root, "Chiyou", "Jefe", PrimitiveType.Capsule, new Vector3(150f, 8.5f, 0f), new Vector3(3f, 3f, 3f), false);
        Solid(root, "Mastil_1", 130f, 9f, 0.5f, 8f, Wall);
        Pickups(root, "Fragmento", 126f, 156f, 3, 8f);
        Ring(root, 60f, 10f); Ring(root, 104f, 10f); Ring(root, 142f, 10f);

        // 5. Torre del estandarte (x 160-190): arrival portal
        Solid(root, "Torre_Estandarte", 180f, 6f, 8f, 12f, Wall, 0f, pathDepth);
        Portal(root, 180f, 12f);
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

        PlayBounds(freeDepth);
        ApplyKit(name[5] - '0');
        ApplyPalette(name[5] - '0');
        NightLook(name[5] - '0', freeDepth ? 8f : 2f);

        var hud = AssetDatabase.LoadAssetAtPath<GameObject>(HudPath);
        if (hud != null) PrefabUtility.InstantiatePrefab(hud);
        new GameObject("HudEventSimulator (solo pruebas)").AddComponent<HudEventSimulator>();
        FlowSceneBuilder.AddPauseMenu();
        int level = name[5] - '0';
        FlowSceneBuilder.AddNarrator(level, level < 4 ? NextLevel[level] : "Despedida");

        EditorSceneManager.SaveScene(scene, Folder + name + ".unity");
    }

    // ---- Play-area bounds (ODD S25, T25): six invisible solid boxes around everything the level holds ----
    const float CeilingClearance = 12f, BoundThickness = 2f;
    // Gap between the lowest surface and the floor bound: the fall zone (T27) fills it.
    public const float FallGap = 6f;

    static Bounds levelBounds;
    static float floorBoundTop;

    static void PlayBounds(bool freeDepth)
    {
        Physics.SyncTransforms();
        var root = GameObject.Find("Nivel").transform;
        int entorno = Layer("Entorno");
        bool any = false;
        float walkTop = float.MinValue;
        foreach (var c in root.GetComponentsInChildren<Collider>())
        {
            if (!any) { levelBounds = c.bounds; any = true; } else levelBounds.Encapsulate(c.bounds);
            if (!c.isTrigger && c.gameObject.layer == entorno && (c.CompareTag("Untagged") || c.CompareTag("Plataforma")))
                walkTop = Mathf.Max(walkTop, c.bounds.max.y);
        }
        // 2.5D: the side walls hug the 2 m path; 3D: they wrap the main path and the side paths with 1 m to spare.
        // The 2.5D boss placeholder (3 m capsule, static) may stand into the walls.
        float zHalf = freeDepth ? Mathf.Max(-levelBounds.min.z, levelBounds.max.z) + 1f : pathDepth / 2f;
        float x0 = levelBounds.min.x, x1 = levelBounds.max.x, top = walkTop + CeilingClearance;
        floorBoundTop = levelBounds.min.y - FallGap;
        float t = BoundThickness;

        var parent = new GameObject("Limites").transform;
        Bound(parent, "Inicio", new Vector3(x0 - t, floorBoundTop - t, -zHalf - t), new Vector3(x0, top + t, zHalf + t));
        Bound(parent, "Fin", new Vector3(x1, floorBoundTop - t, -zHalf - t), new Vector3(x1 + t, top + t, zHalf + t));
        Bound(parent, "Izquierda", new Vector3(x0 - t, floorBoundTop - t, -zHalf - t), new Vector3(x1 + t, top + t, -zHalf));
        Bound(parent, "Derecha", new Vector3(x0 - t, floorBoundTop - t, zHalf), new Vector3(x1 + t, top + t, zHalf + t));
        Bound(parent, "Techo", new Vector3(x0 - t, top, -zHalf - t), new Vector3(x1 + t, top + t, zHalf + t));
        Bound(parent, "Suelo", new Vector3(x0 - t, floorBoundTop - t, -zHalf - t), new Vector3(x1 + t, floorBoundTop, zHalf + t));
    }

    static void Bound(Transform parent, string name, Vector3 min, Vector3 max)
    {
        var go = new GameObject("Limites_" + name);
        go.transform.SetParent(parent, false);
        go.transform.position = (min + max) / 2f;
        go.layer = Layer("Entorno");
        go.AddComponent<BoxCollider>().size = max - min;
    }

    // ---- Night look like the teammate's Apophis (ODD S20, S23, T20): dark sky and fog, cool key, accent rim, bloom, embers ----
    const string VolumeFolder = "Assets/Integrante4/Art/Volumes";
    const float LevelLength = 190f;

    static void NightLook(int level, float depth)
    {
        ColorUtility.TryParseHtmlString(Unit404.Integrante4.Core.CulturePalette.AccentHex(level), out Color accent);
        Color sky = Color.Lerp(new Color(0.008f, 0.009f, 0.014f), accent, 0.05f);

        var cam = Camera.main;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = sky;
        var camData = cam.GetComponent<UniversalAdditionalCameraData>() ?? cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
        camData.renderPostProcessing = true;

        var keyGo = GameObject.Find("Directional Light");
        if (keyGo != null)
        {
            keyGo.name = "Luz_Luna";
            var key = keyGo.GetComponent<Light>();
            key.color = new Color(0.55f, 0.62f, 0.8f);
            key.intensity = 0.35f;
            keyGo.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
        }
        var rim = new GameObject("Luz_Contraluz").AddComponent<Light>();
        rim.type = LightType.Directional;
        rim.color = accent;
        rim.intensity = 1f;
        rim.transform.rotation = Quaternion.Euler(18f, 180f, 0f); // from behind the scenery towards the camera

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.Lerp(new Color(0.03f, 0.03f, 0.04f), accent, 0.06f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = sky;
        RenderSettings.fogDensity = 0.012f;

        var volume = new GameObject("PostProceso").AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = NightProfile(level);

        Embers("Brasas", new Vector3(LevelLength / 2f, 2f, 0f), new Vector2(LevelLength, depth + 6f), accent, 45f, true);
        float arenaY = 0f; // top of the arena ground, so the boss embers rise from the floor the player stands on
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            if (r.gameObject.name.StartsWith("Kit_") && r.bounds.center.x > ArenaStartX && r.bounds.center.x < ArenaEndX) arenaY = Mathf.Max(arenaY, r.bounds.max.y);
        var arenaEmbers = Embers("Brasas_Arena", new Vector3((ArenaStartX + ArenaEndX) / 2f, arenaY, 0f), new Vector2(ArenaEndX - ArenaStartX, depth + 6f), new Color(1f, 0.35f, 0.06f), 120f, false);

        var ambience = new GameObject("AmbienteArena").AddComponent<ArenaAmbience>();
        ambience.arenaStartX = ArenaStartX;
        ambience.arenaEndX = ArenaEndX;
        ambience.rimLight = rim;
        ambience.sceneCamera = cam;
        ambience.arenaEmbers = arenaEmbers;
        ambience.levelRim = accent;
        ambience.levelFog = sky;
    }

    static VolumeProfile NightProfile(int level)
    {
        if (!AssetDatabase.IsValidFolder(VolumeFolder)) AssetDatabase.CreateFolder("Assets/Integrante4/Art", "Volumes");
        string path = VolumeFolder + "/Nivel" + level + "_Noche.asset";
        AssetDatabase.DeleteAsset(path);
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, path);
        var bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(0.9f);
        bloom.intensity.Override(1.4f);
        bloom.scatter.Override(0.7f);
        profile.Add<Tonemapping>(true).mode.Override(TonemappingMode.ACES);
        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.38f);
        var color = profile.Add<ColorAdjustments>(true);
        color.postExposure.Override(0.3f);
        color.contrast.Override(15f);
        foreach (var c in profile.components) AssetDatabase.AddObjectToAsset(c, profile);
        EditorUtility.SetDirty(profile);
        return profile;
    }

    static GameObject Embers(string name, Vector3 centre, Vector2 area, Color color, float rate, bool active)
    {
        var go = new GameObject(name);
        go.transform.position = centre;
        go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // emit upwards
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 7f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
        main.startColor = color * 3f; // HDR so the bloom makes them glow
        main.maxParticles = 1500;
        main.prewarm = true;
        var emission = ps.emission;
        emission.rateOverTime = rate;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(area.x, area.y, 0.5f);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.4f;
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = EmberMaterial();
        go.SetActive(active);
        return go;
    }

    static Material EmberMaterial()
    {
        string path = VolumeFolder + "/Brasa.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;
        mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit"));
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    // Culture look (odd/art-style-sheet.md): matte base on untagged scenery, emissive accent on pickups, rings, glyphs, checkpoints and the portal.
    static void ApplyPalette(int level)
    {
        ColorUtility.TryParseHtmlString(Unit404.Integrante4.Core.CulturePalette.BaseHex(level), out Color baseColor);
        ColorUtility.TryParseHtmlString(Unit404.Integrante4.Core.CulturePalette.AccentHex(level), out Color accent);
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            string tag = r.gameObject.tag;
            bool accented = tag == "Fragmento" || tag == "Propulsor" || tag == "Glifo" || tag == "Checkpoint" || r.gameObject.name == "PortalLlegada" || r.gameObject.name == "PortalFaro";
            if (accented)
            {
                var m = Mat(accent);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", accent * 1.5f);
                r.sharedMaterial = m;
            }
            else if (tag == "Untagged" && r.gameObject.layer == Layer("Entorno") && r.gameObject.name != "JugadorTemporal" && !InKit(r.transform))
            {
                r.sharedMaterial = Mat(baseColor);
            }
        }
    }

    // ---- Blender kit skin (odd T10): the grey block stays as the collider, a culture kit piece renders instead ----
    const string ArtFolder = "Assets/Integrante4/Art/";
    static readonly string[] Cultures = { "Mesopotamia", "Egipto", "Grecia", "China" };

    // Nominal size (Unity axes: width, height, depth) of each Blender kit piece, see .claude/skills/unit404-niveles-blender/scripts/kits.py
    static Vector3 NominalSize(string piece)
    {
        switch (piece)
        {
            case "column": return new Vector3(1.4f, 6f, 1.4f);
            case "wall": return new Vector3(10f, 5f, 1f);
            case "tower": return new Vector3(5.6f, 8f, 5.6f);
            case "ship": return new Vector3(6f, 1.6f, 2f);
            default: return new Vector3(4f, 1f, 4f); // platform module (tiled, never stretched)
        }
    }

    // Zone 4 of every level (x 120-160) is the boss arena: it uses the teammate's Apophis basalt ground (ODD S23).
    public const float ArenaStartX = 120f, ArenaEndX = 160f;

    static string KitPiece(string blockName)
    {
        if (blockName.StartsWith("Columna_")) return "column";
        if (blockName.StartsWith("Muro") || blockName.StartsWith("Bloque") || blockName.StartsWith("Mastil")) return "wall";
        if (blockName.StartsWith("Torre_") && !blockName.StartsWith("Torre_Estandarte")) return "tower";
        if (blockName.StartsWith("Barco")) return "ship";
        return "platform";
    }

    static bool InKit(Transform t)
    {
        for (; t != null; t = t.parent) if (t.name.StartsWith("Kit_")) return true;
        return false;
    }

    // Material built from the maps baked in Blender (albedo, tangent normal, emission); flat palette colour if they are missing.
    static Material KitMaterial(string folder, string piece, Color fallback)
    {
        string dir = ArtFolder + folder + "/";
        string matDir = dir + "Materials";
        if (!AssetDatabase.IsValidFolder(matDir)) AssetDatabase.CreateFolder(ArtFolder + folder, "Materials");
        string path = matDir + "/" + piece + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            AssetDatabase.CreateAsset(mat, path);
        }
        var albedo = LoadMap(dir + piece + "_albedo.png", false);
        var normal = LoadMap(dir + piece + "_normal.png", true);
        var emission = LoadMap(dir + piece + "_emission.png", false);
        mat.color = albedo != null ? Color.white : fallback;
        mat.SetTexture("_BaseMap", albedo);
        mat.SetFloat("_Smoothness", 0.15f);
        if (normal != null) { mat.SetTexture("_BumpMap", normal); mat.EnableKeyword("_NORMALMAP"); }
        if (emission != null)
        {
            mat.SetTexture("_EmissionMap", emission);
            mat.SetColor("_EmissionColor", Color.white * 3f); // HDR: the URP bloom picks the glowing cracks up
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Texture2D LoadMap(string path, bool isNormal)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return null;
        var type = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        if (importer.textureType != type) { importer.textureType = type; importer.SaveAndReimport(); }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static void ApplyKit(int level)
    {
        string culture = Cultures[level - 1];
        string prefix = culture.ToLowerInvariant();
        ColorUtility.TryParseHtmlString(Unit404.Integrante4.Core.CulturePalette.BaseHex(level), out Color baseColor);
        int entorno = Layer("Entorno");

        var blocks = new System.Collections.Generic.List<Transform>();
        foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            var go = r.gameObject;
            if (!r.enabled || go.tag != "Untagged" || go.layer != entorno || go.name == "JugadorTemporal") continue;
            if (go.GetComponent<BoxCollider>() == null || InKit(go.transform)) continue;
            blocks.Add(go.transform);
        }

        foreach (var block in blocks)
        {
            string piece = KitPiece(block.name);
            bool arena = piece == "platform" && block.position.x > ArenaStartX && block.position.x < ArenaEndX;
            string folder = arena ? "Arena" : culture;
            string asset = (arena ? "arena" : prefix) + "_" + piece;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArtFolder + folder + "/" + asset + ".fbx");
            if (prefab == null) { Debug.LogWarning("Kit piece missing: " + asset); continue; }
            var material = KitMaterial(folder, asset, baseColor * 0.3f);

            Vector3 size = block.localScale, nominal = NominalSize(piece);
            if (piece == "platform")
            {
                // Tile 4 m modules so the stone texture keeps its scale; the top slab covers the block footprint exactly (ODD S18).
                int nx = Mathf.Max(1, Mathf.RoundToInt(size.x / nominal.x)), nz = Mathf.Max(1, Mathf.RoundToInt(size.z / nominal.z));
                var scale = new Vector3(size.x / (nx * nominal.x), size.y / nominal.y, size.z / (nz * nominal.z));
                for (int ix = 0; ix < nx; ix++)
                    for (int iz = 0; iz < nz; iz++)
                    {
                        var local = new Vector3((ix + 0.5f) / nx - 0.5f, -0.5f, (iz + 0.5f) / nz - 0.5f);
                        Place(prefab, block, material, scale, block.position + block.rotation * Vector3.Scale(local, size));
                    }
            }
            else
            {
                var scale = piece == "ship" ? Vector3.one * (size.x / nominal.x) : new Vector3(size.x / nominal.x, size.y / nominal.y, size.z / nominal.z);
                // kit pieces have their origin at the centre of their base
                Place(prefab, block, material, scale, block.position - block.rotation * new Vector3(0f, size.y / 2f, 0f));
            }
            block.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    static void Place(GameObject prefab, Transform block, Material material, Vector3 scale, Vector3 position)
    {
        var kit = (GameObject)PrefabUtility.InstantiatePrefab(prefab, block.parent);
        kit.name = "Kit_" + block.name;
        kit.transform.rotation = block.rotation;
        kit.transform.localScale = scale;
        kit.transform.position = position;
        foreach (var t in kit.GetComponentsInChildren<Transform>()) t.gameObject.layer = block.gameObject.layer;
        foreach (var kr in kit.GetComponentsInChildren<Renderer>())
        {
            var mats = kr.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) mats[i] = material;
            kr.sharedMaterials = mats;
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

    // Depth of the walkable main path: 2 m on the 2.5D levels, 8 m on the 3D worlds of levels 3-4 (ODD S19).
    static float pathDepth = 2f;

    static void Solid(Transform p, string name, float x, float y, float w, float h, Color? color = null, float z = 0f, float d = 2f)
        => Make(p, name, PrimitiveType.Cube, new Vector3(x, y, z), new Vector3(w, h, d), color ?? Ground, "Entorno");

    // Walkable surface of the main path (floors, terraces, arenas): uses the level's path depth.
    static void Walk(Transform p, string name, float x, float y, float w, float h)
        => Solid(p, name, x, y, w, h, null, 0f, pathDepth);

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
        => Walk(p, name, (x0 + x1) / 2f, y - 0.5f, x1 - x0, 1f);

    // Ramp from (x - w/2, y) to (x + w/2, y + h) (ODD S26): the slab is as long as the slope and its TOP face lies on that
    // line, so it meets both floors flush; its thickness hangs below the line, tucked under the floors.
    const float RampThickness = 0.5f;

    static void Rampa(Transform p, string name, float x, float y, float w, float h)
    {
        float angle = Mathf.Atan2(h, w);
        Vector3 normal = new Vector3(-Mathf.Sin(angle), Mathf.Cos(angle), 0f);
        Vector3 centre = new Vector3(x, y + h / 2f, 0f) - normal * (RampThickness / 2f);
        var go = Make(p, name, PrimitiveType.Cube, centre, new Vector3(Mathf.Sqrt(w * w + h * h), RampThickness, pathDepth), Wall, "Entorno");
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
    }

    // Height of the ramp surface at x (for obstacles that roll on it).
    static float RampTop(float x, float rx, float ry, float w, float h) => ry + (x - (rx - w / 2f)) * h / w;

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

    // Arrival pad: a solid flat Plataforma, because Integrante 1 completes the level in OnCollisionEnter (speed < 3, upright).
    // The tall beacon is decorative (no collider) so the goal stays visible from far away.
    static void Portal(Transform p, float x, float floorTopY)
    {
        Tagged(p, "PortalLlegada", "Plataforma", PrimitiveType.Cube, new Vector3(x, floorTopY + 0.2f, 0f), new Vector3(6f, 0.4f, pathDepth), false);
        var beacon = Make(p, "PortalFaro", PrimitiveType.Cube, new Vector3(x, floorTopY + 3.2f, 0f), new Vector3(0.4f, 5.6f, 0.4f), Color.white, "Default");
        Object.DestroyImmediate(beacon.GetComponent<Collider>());
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
