using System.Linq;
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
        // Obstacles span the whole 8 m path (ODD S19): logs rolling down the slope and fallen columns dropping across it.
        float slope = Mathf.Atan2(10f, 40f);
        Vector3 downhill = new Vector3(-Mathf.Cos(slope), -Mathf.Sin(slope), 0f);
        for (int i = 0; i < 3; i++)
        {
            float lx = 90f + i * 10f;
            Beam(root, "Tronco_" + (i + 1), new Vector3(lx, RampTop(lx, 100f, 0f, 40f, 10f) + 1f / Mathf.Cos(slope), 0f), 2f, pathDepth, downhill * 6f, 3f);
        }
        for (int i = 0; i < 2; i++)
        {
            float cx = 96f + i * 14f;
            Beam(root, "ColumnaCaida_" + (i + 1), new Vector3(cx, RampTop(cx, 100f, 0f, 40f, 10f) + 0.6f / Mathf.Cos(slope) + 5f, 0f), 1.2f, pathDepth, Vector3.down * 5f, 2f);
        }
        // The side path next to the slope would skirt the logs: it gets its own (top 4, z -12..-6, x 93-107).
        Beam(root, "Tronco_Lateral_Ladera", new Vector3(97f, 4.8f, -9f), 1.6f, 6f, Vector3.right * 6f, 3f);
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
        SidePath(root, "Agora", 60f, 0f, 9f); SidePath(root, "Ladera", 100f, 4f, -9f); SidePath(root, "Templo", 140f, 9f, 9f);

        // Culture dressing (ODD S28-S29, T30-T31): a rhythm of gates along the corridor, props on the path edges and the
        // side paths. Nothing on the Ladera, where logs and columns roll across the whole width.
        Gate(root, 1, 8f, 0f, "kouros");                                   // harbour: statues greet the robot
        Prop(root, "amphorae", 4f, 0f, -3.3f); Prop(root, "amphorae", 5.5f, 0f, 3.3f); Prop(root, "brazier", 11f, 0f, -3.4f);
        Gate(root, 2, 46f, 0f, "column", "lintel_gr", "Hoplita_3");        // propylaea into the agora
        Gate(root, 3, 52f, 0f, "kouros", null, null);                      // frames Hoplita_1 (x 52)
        Prop(root, "altar", 59.5f, 0f, -3.2f); Prop(root, "amphorae", 56f, 0f, 3.3f); Prop(root, "amphorae", 62.5f, 0f, -3.3f);
        Gate(root, 4, 66f, 0f, "kouros", null, null);                      // frames Hoplita_2 (x 66)
        Gate(root, 5, 72.5f, 0f, "column", "lintel_gr", "Hoplita_4");
        Prop(root, "olive", 54.5f, 0f, 10.4f); Prop(root, "amphorae", 65.5f, 0f, 10.8f);   // agora side path
        Prop(root, "brazier", 127f, 10f, -3.4f); Prop(root, "brazier", 127f, 10f, 3.4f);   // arena: accent fire
        Prop(root, "brazier", 156f, 10f, -3.4f); Prop(root, "brazier", 156f, 10f, 3.4f);
        Prop(root, "olive", 134.5f, 9f, 10.4f); Prop(root, "brazier", 146f, 9f, 10.8f);    // temple side path
        Gate(root, 6, 170f, 10f, "column", "lintel_gr", "Hoplita_5");      // altar precinct (clear of Inscripcion_8 at x 168)
        Prop(root, "altar", 177f, 10f, -3.2f); Prop(root, "kouros", 177f, 10f, 3.35f);
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

        // 3. Adarve de la muralla (x 80-120): a ramp up the wall (0 -> 6, ODD S40), then the wall walk with carts and
        // dust storm. The wall is a solid body down to the ground, no void under the walkway.
        Rampa(root, "Rampa_Muralla", 86f, 0f, 12f, 6f);
        RampBody(root, "Muro_Rampa", 86f, 0f, 12f, 6f, 4);
        Walk(root, "Adarve", 106f, 5f, 28f, 2f);
        Solid(root, "Muro_Adarve", 106f, 2f, 28f, 4f, Wall, 0f, pathDepth);
        // Rows of carts side by side across the 8 m wall walk (ODD S19), on the walkway (top 6), moving together.
        for (int i = 0; i < 3; i++) CartRow(root, "Carreta_" + (i + 1), 95f + i * 9f, 6f, -pathDepth / 2f, pathDepth / 2f, 3);
        // The side path beside the wall walk would skirt the carts: it gets its own row (top 5, z 6..12, x 93-107).
        CartRow(root, "Carreta_Lateral_Adarve", 98f, 5f, 6f, 12f, 2);
        Tagged(root, "TormentaPolvo", "Contaminacion", PrimitiveType.Cube, new Vector3(108f, 8f, 0f), new Vector3(8f, 6f, 6f), true, new Color(0.8f, 0.7f, 0.3f));
        Pickups(root, "Fragmento", 84f, 118f, 3, 8f);

        // 4. Tramo ancho entre torres (x 120-160): second checkpoint and Chiyou arena
        Walk(root, "Tramo_Ancho", 140f, 5f, 40f, 2f);
        Solid(root, "Muro_Tramo", 140f, 2f, 40f, 4f, Wall, 0f, pathDepth);
        Tagged(root, "Checkpoint_2", "Checkpoint", PrimitiveType.Cube, new Vector3(124f, 8f, 0f), new Vector3(1f, 4f, 1f), true);
        Tagged(root, "Chiyou", "Jefe", PrimitiveType.Capsule, new Vector3(150f, 8.5f, 0f), new Vector3(3f, 3f, 3f), false);
        Solid(root, "Mastil_1", 130f, 9f, 0.5f, 8f, Wall);
        Pickups(root, "Fragmento", 126f, 156f, 3, 8f);
        Ring(root, 60f, 10f); Ring(root, 104f, 10f); Ring(root, 142f, 10f);

        // 5. Torre del estandarte (x 160-190): the wall climbs to the tower top (6 -> 12, ODD S40), arrival portal
        Rampa(root, "Rampa_Torre", 168f, 6f, 16f, 6f);
        RampBody(root, "Muro_Subida", 168f, 6f, 16f, 6f, 4);
        Solid(root, "Torre_Estandarte", 180f, 6f, 8f, 12f, Wall, 0f, pathDepth);
        Portal(root, 180f, 12f);
        foreach (float fx in new[] { 162f, 167f, 172f }) Pickups(root, "Fragmento", fx, fx, 1, RampTop(fx, 168f, 6f, 16f, 6f) + 1.5f);

        Glyphs(root, new[] { 10f, 30f, 48f, 72f, 96f, 114f, 138f, 166f }, new[] { 4f, 6f, 5f, 6f, 9f, 9f, 8f, RampTop(166f, 168f, 6f, 16f, 6f) + 1.5f }, "TiraBambu");
        SidePath(root, "Torre", 60f, 0f, -9f); SidePath(root, "Adarve", 100f, 5f, 9f); SidePath(root, "Tramo", 140f, 5f, -9f);

        // Culture dressing (ODD S28-S29, T30-T31): guardian lions, paifang gates with crossbowmen, lanterns and banners.
        // Nothing on the wall walk, where the cart rows run across the whole width.
        Gate(root, 1, 8f, 0f, "lion");
        Prop(root, "lantern", 15f, 0f, -3.3f); Prop(root, "lantern", 15f, 0f, 3.3f);
        Gate(root, 2, 20f, 0f, "column", "lintel_cn", "Ballestero_3");    // paifang at the mountain pass
        Prop(root, "bamboo", 33f, 0f, -3f); Prop(root, "banner", 35.5f, 0f, 3.6f);
        Gate(root, 3, 44f, 0f, "lion");
        Prop(root, "banner", 46.5f, 0f, -3.6f); Prop(root, "lantern", 56f, 0f, 3.3f); Prop(root, "lantern", 64f, 0f, -3.3f);
        Gate(root, 4, 60f, 0f, "column", "lintel_cn", "Ballestero_4");    // paifang between the towers
        Prop(root, "banner", 75f, 0f, 3.6f);
        Prop(root, "lantern", 54f, 0f, -11f); Prop(root, "bamboo", 66f, 0f, -10.8f);       // tower side path
        Gate(root, 5, 133f, 6f, "lantern", null, "Ballestero_5");          // arena: accent lamps (clear of the x 126 fragment)
        Gate(root, 6, 155f, 6f, "lantern");
        Prop(root, "lion", 134f, 5f, -11f); Prop(root, "lantern", 146f, 5f, -11f);         // wide-stretch side path
        Finish(scene, new Vector3(2f, 2f, 0f), "Nivel4_China", true);
    }

    // ---------------- helpers ----------------
    static Scene NewLevel(string name, Color sky)
    {
        propCount = 0;
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

        PlayBounds(freeDepth, name[5] - '0');
        ApplyKit(name[5] - '0');
        if (!freeDepth) Dress25D(name[5] - '0');
        ApplyElements(name[5] - '0', freeDepth);
        Backdrop(name[5] - '0', freeDepth);
        ApplyPalette(name[5] - '0');
        CultureDayLook(name[5] - '0', freeDepth ? 8f : 2f);

        var hud = AssetDatabase.LoadAssetAtPath<GameObject>(HudPath);
        if (hud != null) PrefabUtility.InstantiatePrefab(hud);
        new GameObject("HudEventSimulator (solo pruebas)").AddComponent<HudEventSimulator>();
        FlowSceneBuilder.AddPauseMenu().fallScreen = Object.FindFirstObjectByType<FallScreen>();
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

    static void PlayBounds(bool freeDepth, int level)
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

        // Fall zone (T27): fills the gap between the floor bound and just under the lowest surface.
        var zone = new GameObject("ZonaCaida");
        zone.transform.SetParent(parent, false);
        Vector3 zMin = new Vector3(x0, floorBoundTop, -zHalf), zMax = new Vector3(x1, levelBounds.min.y - 0.5f, zHalf);
        zone.transform.position = (zMin + zMax) / 2f;
        var trigger = zone.AddComponent<BoxCollider>();
        trigger.size = zMax - zMin;
        trigger.isTrigger = true;
        var fall = zone.AddComponent<FallZone>();

        var screen = FlowSceneBuilder.AddFallScreen(level);
        fall.screen = screen;
        screen.playerStart = GameObject.Find("PlayerStart").transform;
        screen.checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint").Select(c => c.transform).OrderBy(c => c.position.x).ToArray();
        screen.arenaStartX = ArenaStartX;
        screen.arenaEndX = ArenaEndX;
        var door = new GameObject("PuertaArena").transform;
        door.SetParent(parent, false);
        door.position = new Vector3(ArenaStartX + 2f, SurfaceTop(root, ArenaStartX + 2f) + 1f, 0f);
        screen.arenaDoor = door;
    }

    // Top of the highest solid scenery under (x, z = 0).
    static float SurfaceTop(Transform root, float x)
    {
        int entorno = Layer("Entorno");
        float top = 0f;
        foreach (var c in root.GetComponentsInChildren<Collider>())
            if (!c.isTrigger && c.gameObject.layer == entorno && c.CompareTag("Untagged") && c.bounds.min.x <= x && c.bounds.max.x >= x && c.bounds.min.z <= 0f && c.bounds.max.z >= 0f && c.bounds.size.y < 4f)
                top = Mathf.Max(top, c.bounds.max.y);
        return top;
    }

    static void Bound(Transform parent, string name, Vector3 min, Vector3 max)
    {
        var go = new GameObject("Limites_" + name);
        go.transform.SetParent(parent, false);
        go.transform.position = (min + max) / 2f;
        go.layer = Layer("Entorno");
        go.AddComponent<BoxCollider>().size = max - min;
    }

    // ---- Ambience (ODD S23, T20, T23): lit culture day on the route, the dark Apophis look only in the boss arena ----
    const string VolumeFolder = "Assets/Integrante4/Art/Volumes";

    // Per culture: sky tint, horizon/ground, haze (fog) and sun colour + intensity. Warm, readable daylight of each era.
    struct DayLook { public Color sky, ground, haze, sun; public float sunIntensity; }
    static readonly DayLook[] Days =
    {
        new DayLook { sky = new Color(0.62f, 0.55f, 0.45f), ground = new Color(0.55f, 0.45f, 0.32f), haze = new Color(0.86f, 0.78f, 0.62f), sun = new Color(1f, 0.88f, 0.7f), sunIntensity = 1.35f },  // Mesopotamia: dusty river plain
        new DayLook { sky = new Color(0.42f, 0.55f, 0.78f), ground = new Color(0.62f, 0.5f, 0.32f), haze = new Color(0.88f, 0.8f, 0.64f), sun = new Color(1f, 0.9f, 0.72f), sunIntensity = 1.45f },     // Egipto: clear sky, desert sand
        new DayLook { sky = new Color(0.35f, 0.55f, 0.85f), ground = new Color(0.45f, 0.5f, 0.4f), haze = new Color(0.75f, 0.85f, 0.95f), sun = new Color(1f, 0.97f, 0.9f), sunIntensity = 1.3f },      // Grecia: Mediterranean blue
        new DayLook { sky = new Color(0.6f, 0.62f, 0.6f), ground = new Color(0.55f, 0.5f, 0.38f), haze = new Color(0.84f, 0.8f, 0.66f), sun = new Color(1f, 0.9f, 0.78f), sunIntensity = 1.2f },       // China: loess haze
    };

    static void CultureDayLook(int level, float depth)
    {
        ColorUtility.TryParseHtmlString(Unit404.Integrante4.Core.CulturePalette.AccentHex(level), out Color accent);
        DayLook day = Days[level - 1];
        Color darkSky = Color.Lerp(new Color(0.008f, 0.009f, 0.014f), accent, 0.05f);

        var cam = Camera.main;
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.backgroundColor = day.haze;
        var camData = cam.GetComponent<UniversalAdditionalCameraData>() ?? cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
        camData.renderPostProcessing = true;

        var sunGo = GameObject.Find("Directional Light");
        sunGo.name = "Luz_Sol";
        var sun = sunGo.GetComponent<Light>();
        sun.color = day.sun;
        sun.intensity = day.sunIntensity;
        sun.shadows = LightShadows.Soft;
        sunGo.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        var rim = new GameObject("Luz_Contraluz").AddComponent<Light>();
        rim.type = LightType.Directional;
        rim.color = new Color(0.7f, 0.8f, 1f);
        rim.intensity = 0.25f;
        rim.transform.rotation = Quaternion.Euler(18f, 180f, 0f); // from behind the scenery towards the camera

        RenderSettings.sun = sun;
        RenderSettings.skybox = SkyMaterial(level, day);
        RenderSettings.ambientMode = AmbientMode.Flat;
        Color ambient = day.haze * 0.55f;
        ambient.a = 1f;
        RenderSettings.ambientLight = ambient;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = day.haze;
        RenderSettings.fogDensity = 0.006f;

        var dayVolume = new GameObject("PostProceso").AddComponent<Volume>();
        dayVolume.isGlobal = true;
        dayVolume.weight = 1f;
        dayVolume.sharedProfile = DayProfile(level);
        var arenaVolume = new GameObject("PostProceso_Arena").AddComponent<Volume>();
        arenaVolume.isGlobal = true;
        arenaVolume.weight = 0f;
        arenaVolume.sharedProfile = NightProfile(level);

        // boss embers rise from the arena floor the player stands on, in the culture accent (T23)
        float arenaY = SurfaceTop(GameObject.Find("Nivel").transform, (ArenaStartX + ArenaEndX) / 2f);
        var arenaEmbers = Embers("Brasas_Arena", new Vector3((ArenaStartX + ArenaEndX) / 2f, arenaY, 0f), new Vector2(ArenaEndX - ArenaStartX, depth + 6f), accent, 120f, false);

        var ambience = new GameObject("AmbienteArena").AddComponent<ArenaAmbience>();
        ambience.arenaStartX = ArenaStartX;
        ambience.arenaEndX = ArenaEndX;
        ambience.keyLight = sun;
        ambience.rimLight = rim;
        ambience.sceneCamera = cam;
        ambience.arenaEmbers = arenaEmbers;
        ambience.dayVolume = dayVolume;
        ambience.arenaVolume = arenaVolume;
        ambience.levelKey = sun.color;
        ambience.levelKeyIntensity = sun.intensity;
        ambience.levelRim = rim.color;
        ambience.levelRimIntensity = rim.intensity;
        ambience.levelAmbient = ambient;
        ambience.levelFog = day.haze;
        ambience.levelFogDensity = RenderSettings.fogDensity;
        ambience.bossRim = accent;
        ambience.bossFog = darkSky;
        ambience.bossAmbient = Color.Lerp(new Color(0.03f, 0.03f, 0.04f), accent, 0.06f);
    }

    static Material SkyMaterial(int level, DayLook day)
    {
        if (!AssetDatabase.IsValidFolder(VolumeFolder)) AssetDatabase.CreateFolder("Assets/Integrante4/Art", "Volumes");
        string path = VolumeFolder + "/Nivel" + level + "_Cielo.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Skybox/Procedural"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_SkyTint", day.sky);
        mat.SetColor("_GroundColor", day.ground);
        mat.SetFloat("_AtmosphereThickness", 1.1f);
        mat.SetFloat("_Exposure", 1.25f);
        mat.SetFloat("_SunSize", 0.035f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    // Day grade: neutral tonemapping, gentle bloom for the emissive pickups only, a touch of saturation; no vignette.
    static VolumeProfile DayProfile(int level)
    {
        if (!AssetDatabase.IsValidFolder(VolumeFolder)) AssetDatabase.CreateFolder("Assets/Integrante4/Art", "Volumes");
        string path = VolumeFolder + "/Nivel" + level + "_Dia.asset";
        AssetDatabase.DeleteAsset(path);
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, path);
        var bloom = profile.Add<Bloom>(true);
        bloom.threshold.Override(1.1f);
        bloom.intensity.Override(0.35f);
        bloom.scatter.Override(0.6f);
        profile.Add<Tonemapping>(true).mode.Override(TonemappingMode.Neutral);
        var color = profile.Add<ColorAdjustments>(true);
        color.postExposure.Override(0.1f);
        color.contrast.Override(6f);
        color.saturation.Override(8f);
        foreach (var c in profile.components) AssetDatabase.AddObjectToAsset(c, profile);
        EditorUtility.SetDirty(profile);
        return profile;
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
        if (PropSizes.TryGetValue(piece, out Vector3 prop)) return prop;
        switch (piece)
        {
            case "column": return new Vector3(1.4f, 6f, 1.4f);
            case "wall": return new Vector3(10f, 5f, 1f);
            case "tower": return new Vector3(5.6f, 8f, 5.6f);
            case "ship": return new Vector3(6f, 1.6f, 2f);
            case "stairs": return new Vector3(6f, 2f, 2f);
            case "block": return new Vector3(3f, 3f, 2f);
            case "mast": return new Vector3(0.5f, 8f, 2f);
            case "keep": return new Vector3(8f, 12f, 8f);
            default: return new Vector3(4f, 1f, 4f); // platform module (tiled, never stretched)
        }
    }

    // Culture props (ODD T30-T31), base-centred, facing -Z: keep in sync with props.py NOMINAL (width, height, depth).
    static readonly System.Collections.Generic.Dictionary<string, Vector3> PropSizes = new System.Collections.Generic.Dictionary<string, Vector3>
    {
        { "hut", new Vector3(4f, 3.2f, 3f) }, { "jars", new Vector3(1.6f, 1.3f, 1.4f) }, { "bricks", new Vector3(1.8f, 1.1f, 1.2f) },
        { "brazier", new Vector3(1f, 1.4f, 1f) }, { "lamassu", new Vector3(1.4f, 2.6f, 3f) }, { "palm", new Vector3(3.5f, 6.5f, 3.5f) },
        { "obelisk", new Vector3(1.2f, 6f, 1.2f) }, { "sphinx", new Vector3(1.6f, 2f, 3.6f) }, { "pharaoh", new Vector3(1.4f, 3.2f, 1.8f) },
        { "papyrus", new Vector3(1.8f, 2.2f, 1.8f) }, { "stele", new Vector3(3f, 2.8f, 0.6f) },
        { "amphorae", new Vector3(1.4f, 1.3f, 1.2f) }, { "kouros", new Vector3(1.1f, 3.4f, 1.1f) }, { "olive", new Vector3(3f, 3.8f, 3f) },
        { "altar", new Vector3(2f, 1.3f, 1.3f) }, { "lintel_gr", new Vector3(8.4f, 1.4f, 1.4f) },
        { "lantern", new Vector3(1.1f, 2.4f, 1.25f) }, { "lion", new Vector3(1.3f, 2.2f, 1.9f) }, { "bamboo", new Vector3(1.8f, 4.8f, 1.8f) },
        { "banner", new Vector3(1.3f, 5.1f, 0.4f) }, { "lintel_cn", new Vector3(9f, 1.6f, 1.6f) },
    };

    // Zone 4 of every level (x 120-160) is the boss arena: dark ground and ambience (ODD S23).
    public const float ArenaStartX = 120f, ArenaEndX = 160f;

    static string KitPiece(string blockName)
    {
        // culture props (T30) and corridor gates (T31): "Prop_<id>_<kind>", "Puerta<N>_<role>_<kind>"
        if (blockName.StartsWith("Prop_") || blockName.StartsWith("Puerta"))
            return blockName.Substring(blockName.IndexOf('_', blockName.IndexOf('_') + 1) + 1);
        if (blockName.StartsWith("Columna_")) return "column";
        if (blockName.StartsWith("Terraza")) return "stairs";
        if (blockName.StartsWith("Bloque")) return "block";
        if (blockName.StartsWith("Mastil")) return "mast";
        if (blockName == "Torre_Estandarte") return "keep";
        if (blockName.StartsWith("Muro") || blockName.StartsWith("Ladera_")) return "wall";
        if (blockName.StartsWith("Torre_")) return "tower";
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
        var occlusion = LoadMap(dir + piece + "_occlusion.png", false, linear: true);
        mat.color = albedo != null ? Color.white : fallback;
        mat.SetTexture("_BaseMap", albedo);
        // T29 (ODD S30): route kit maps carry smoothness (1 - roughness) in the albedo alpha; other maps are opaque
        var albedoImporter = AssetImporter.GetAtPath(dir + piece + "_albedo.png") as TextureImporter;
        bool smoothInAlpha = albedoImporter != null && albedoImporter.DoesSourceTextureHaveAlpha();
        mat.SetFloat("_Smoothness", smoothInAlpha ? 1f : 0.15f);
        mat.SetFloat("_SmoothnessTextureChannel", smoothInAlpha ? 1f : 0f);
        if (smoothInAlpha) mat.EnableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
        else mat.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
        mat.SetTexture("_OcclusionMap", occlusion);
        mat.SetFloat("_OcclusionStrength", 1f);
        if (occlusion != null) mat.EnableKeyword("_OCCLUSIONMAP"); else mat.DisableKeyword("_OCCLUSIONMAP");
        if (normal != null) { mat.SetTexture("_BumpMap", normal); mat.EnableKeyword("_NORMALMAP"); }
        if (emission != null)
        {
            mat.SetTexture("_EmissionMap", emission);
            mat.SetColor("_EmissionColor", Color.white * 3f); // HDR: the URP bloom picks the glowing cracks up
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            // day maps have no glow: clear what an earlier bake may have left on the material asset
            mat.SetTexture("_EmissionMap", null);
            mat.SetColor("_EmissionColor", Color.black);
            mat.DisableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Texture2D LoadMap(string path, bool isNormal, bool linear = false)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return null;
        var type = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
        bool srgb = !isNormal && !linear;
        // T29 (ODD S30): keep the 2048 bakes and filter them anisotropically, so floors stay crisp at grazing angles
        if (importer.textureType != type || importer.sRGBTexture != srgb || importer.maxTextureSize < 2048 || importer.anisoLevel < 8
            || importer.alphaIsTransparency)
        {
            importer.textureType = type;
            importer.sRGBTexture = srgb;
            importer.maxTextureSize = Mathf.Max(importer.maxTextureSize, 2048);
            importer.anisoLevel = 8;
            importer.alphaIsTransparency = false; // the albedo alpha is smoothness, not transparency
            importer.SaveAndReimport();
        }
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
            // ODD S23 (c): the arena floor is dark; Mesopotamia uses the teammate's red basalt (`arena` kit), the other
            // cultures their own platform mesh with the dark stone maps baked on the same UVs (<culture>_arena_platform_*).
            bool arena = piece == "platform" && block.position.x > ArenaStartX && block.position.x < ArenaEndX;
            bool basalt = arena && level == 1;
            string folder = basalt ? "Arena" : culture;
            string mesh = basalt ? "arena_platform" : prefix + "_" + piece;
            string maps = arena && !basalt ? prefix + "_arena_platform" : mesh;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArtFolder + folder + "/" + mesh + ".fbx");
            if (prefab == null) { Debug.LogWarning("Kit piece missing: " + mesh); continue; }
            var material = KitMaterial(folder, maps, baseColor * 0.3f);

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
            else if (piece == "wall" && size.x > nominal.x * 1.5f)
            {
                // Long walls (level 4 hillsides): 10 m pieces side by side, so merlons and pilasters keep their rhythm.
                int nx = Mathf.RoundToInt(size.x / nominal.x);
                var scale = new Vector3(size.x / (nx * nominal.x), size.y / nominal.y, size.z / nominal.z);
                for (int ix = 0; ix < nx; ix++)
                {
                    var local = new Vector3((ix + 0.5f) / nx - 0.5f, -0.5f, 0f);
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

    // ---- Gameplay elements (ODD T18): the primitive keeps its collider, tag and behaviour; a culture model rendered as
    // its child fills the same box (so it moves with obstacles and hides with collected pickups) ----
    static string ElementArt(GameObject go)
    {
        switch (go.tag)
        {
            case "Fragmento": return "crystal";
            case "Propulsor": return "ring";
            case "Glifo": return "tablet";
            case "Checkpoint": return "checkpoint";
        }
        if (go.name.StartsWith("Estaca_")) return "stake";
        if (go.name.StartsWith("Antorcha_")) return "torch";
        if (go.name.StartsWith("ColumnaCaida_")) return "drum";
        if (go.name.StartsWith("Tronco_")) return "log";
        if (go.name.StartsWith("Carreta_")) return "cart";
        return null;
    }

    // Size each element is modelled at (Unity axes), see .claude/skills/unit404-niveles-blender/scripts/elements.py NOMINAL.
    static Vector3 ElementSize(string art) => art switch
    {
        "crystal" => new Vector3(0.6f, 0.6f, 0.6f),
        "ring" => new Vector3(2.5f, 0.3f, 2.5f),
        "tablet" => new Vector3(0.7f, 1f, 0.2f),
        "checkpoint" => new Vector3(1f, 4f, 1f),
        "torch" => new Vector3(0.6f, 1.2f, 0.6f),
        "stake" => new Vector3(0.6f, 2f, 0.6f),
        "cart" => new Vector3(2f, 1.5f, 8f / 3f),
        _ => new Vector3(1f, 2f, 1f), // log, drum: a unit cylinder
    };

    static void ApplyElements(int level, bool freeDepth)
    {
        string culture = Cultures[level - 1], prefix = culture.ToLowerInvariant();
        ColorUtility.TryParseHtmlString(Unit404.Integrante4.Core.CulturePalette.AccentHex(level), out Color accent);
        var targets = GameObject.Find("Nivel").GetComponentsInChildren<MeshRenderer>()
            .Select(r => r.gameObject).Where(g => ElementArt(g) != null).ToArray();
        foreach (var go in targets)
        {
            string art = ElementArt(go);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArtFolder + culture + "/" + prefix + "_" + art + ".fbx");
            if (prefab == null) { Debug.LogWarning("Element art missing: " + prefix + "_" + art); continue; }
            var kit = (GameObject)PrefabUtility.InstantiatePrefab(prefab, go.transform);
            kit.name = "Kit_" + go.name;
            kit.transform.localPosition = Vector3.zero;
            kit.transform.localRotation = Quaternion.identity;
            // unit primitive (cylinder = 1 x 2 x 1) / modelled size: the model then takes the primitive's world box
            Vector3 unit = go.GetComponent<MeshFilter>().sharedMesh.name == "Cylinder" ? new Vector3(1f, 2f, 1f) : Vector3.one, size = ElementSize(art);
            kit.transform.localScale = new Vector3(unit.x / size.x, unit.y / size.y, unit.z / size.z);
            Skin(kit, go.layer, KitMaterial(culture, prefix + "_" + art, accent));
            go.GetComponent<MeshRenderer>().enabled = false;
        }

        // Pollution hazards (incense, dust storm): translucent haze instead of a solid primitive.
        foreach (var hazard in GameObject.FindGameObjectsWithTag("Contaminacion"))
        {
            var r = hazard.GetComponent<Renderer>();
            if (r == null) continue;
            Color c = r.sharedMaterial.color;
            r.sharedMaterial = HazeMaterial(new Color(c.r, c.g, c.b, 0.28f));
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        // Portal gate over the arrival pad: decoration only, spans the path (X in 2.5D, Z in the 3D worlds).
        var pad = GameObject.Find("PortalLlegada");
        var archPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArtFolder + culture + "/" + prefix + "_arch.fbx");
        if (pad == null || archPrefab == null) { Debug.LogWarning("Portal arch missing: " + prefix + "_arch"); return; }
        var arch = (GameObject)PrefabUtility.InstantiatePrefab(archPrefab, pad.transform.parent);
        arch.name = "PortalArco";
        arch.transform.position = pad.transform.position - Vector3.up * pad.transform.localScale.y / 2f;
        arch.transform.rotation = freeDepth ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
        Skin(arch, 0, KitMaterial(culture, prefix + "_arch", accent));
    }

    // ---- Backdrops (ODD S17, T19): three modelled depth layers per culture, tiled along the level outside the play
    // area; behind the 2.5D path, on both sides of the 3D worlds. No colliders (FBX import adds none). ----
    static readonly string[] BackdropLayers = { "near", "mid", "far" };
    static readonly float[] BackdropGap = { 4f, 18f, 42f }, BackdropDrop = { 1.5f, 3f, 5f }, BackdropScale = { 1f, 1.6f, 2.6f };
    const float BackdropSegment = 60f; // strip length in backdrops.py

    static void Backdrop(int level, bool freeDepth)
    {
        string culture = Cultures[level - 1], prefix = culture.ToLowerInvariant();
        var root = new GameObject("Paisaje").transform;
        float edge = GameObject.Find("Limites_Derecha").GetComponent<Collider>().bounds.max.z;
        float x0 = levelBounds.min.x - 30f, x1 = levelBounds.max.x + 30f;
        foreach (int side in freeDepth ? new[] { 1, -1 } : new[] { 1 })
            for (int i = 0; i < BackdropLayers.Length; i++)
            {
                string mesh = prefix + "_bg_" + BackdropLayers[i];
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArtFolder + culture + "/" + mesh + ".fbx");
                if (prefab == null) { Debug.LogWarning("Backdrop missing: " + mesh); continue; }
                var material = KitMaterial(culture, mesh, Color.gray);
                float s = BackdropScale[i], seg = BackdropSegment * s, z = side * (edge + BackdropGap[i]);
                int k = 0;
                for (float x = x0 + seg / 2f; x - seg / 2f < x1; x += seg)
                {
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
                    go.name = "Paisaje_" + BackdropLayers[i] + "_" + (side > 0 ? "D" : "I") + "_" + (++k);
                    go.transform.localScale = Vector3.one * s;
                    go.transform.position = new Vector3(x, -BackdropDrop[i], z);
                    go.transform.rotation = side > 0 ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
                    // the strip's front edge sits on its origin: turn it round if the scenery would face into the path
                    if (side * (go.GetComponent<Renderer>().bounds.center.z - z) < 0f) go.transform.rotation *= Quaternion.Euler(0f, 180f, 0f);
                    Skin(go, 0, material);
                }
            }
    }

    static void Skin(GameObject kit, int layer, Material material)
    {
        foreach (var t in kit.GetComponentsInChildren<Transform>()) t.gameObject.layer = layer;
        foreach (var kr in kit.GetComponentsInChildren<Renderer>())
        {
            var mats = kr.sharedMaterials;
            for (int i = 0; i < mats.Length; i++) mats[i] = material;
            kr.sharedMaterials = mats;
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

    // ---- Culture props (ODD S28, T30) ----
    static int propCount;

    // 3D worlds: a solid prop standing on a walkable floor (top = floorTop); the grey block is its collider (ApplyKit skins
    // it). Props on the far side face -Z, those on the near side are turned round, so both look at the path.
    static void Prop(Transform p, string kind, float x, float floorTop, float z)
    {
        Vector3 size = PropSizes[kind];
        var go = Make(p, "Prop_" + (++propCount) + "_" + kind, PrimitiveType.Cube, new Vector3(x, floorTop + size.y / 2f, z), size, Wall, "Entorno");
        if (z < 0f) go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    // ODD S29, T31: corridor gate on the 8 m path: a pillar or statue on each edge facing the centre, an optional lintel
    // across the top and an optional guard (Observador placeholder, Integrante 2 owns the logic) in the middle.
    static void Gate(Transform p, int n, float x, float floorTop, string pillar, string lintel = null, string guard = null)
    {
        Vector3 ps = pillar == "column" ? NominalSize("column") : PropSizes[pillar];
        float z = pathDepth / 2f - ps.z / 2f - 0.05f;
        foreach (int side in new[] { -1, 1 })
        {
            var go = Make(p, "Puerta" + n + "_" + (side < 0 ? "I" : "D") + "_" + pillar, PrimitiveType.Cube, new Vector3(x, floorTop + ps.y / 2f, side * z), ps, Wall, "Entorno");
            if (side < 0) go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        if (lintel != null)
        {
            Vector3 ls = PropSizes[lintel];
            var top = Make(p, "Puerta" + n + "_Dintel_" + lintel, PrimitiveType.Cube, new Vector3(x, floorTop + ps.y + ls.y / 2f, 0f), ls, Wall, "Entorno");
            top.transform.rotation = Quaternion.Euler(0f, 90f, 0f); // spans the path depth
        }
        if (guard != null) Guard(p, guard, x, floorTop + 1.5f);
    }

    // 2.5D levels: a decorative ground strip behind every main floor (z 1-5, same top, no collider: the player is held at
    // z = 0 by the side bounds) dressed with culture props. The boss arena gets neither (ODD S35: its floor keeps the path depth).
    const float ApronZ0 = 1f, ApronDepth = 4f;
    static readonly string[][] DecoKinds =
    {
        new[] { "hut", "jars", "bricks", "brazier", "lamassu", "palm", "palm", "jars", "hut" },
        new[] { "obelisk", "sphinx", "pharaoh", "papyrus", "stele", "palm", "papyrus", "brazier", "palm" },
    };

    static void Dress25D(int level)
    {
        string culture = Cultures[level - 1], prefix = culture.ToLowerInvariant();
        var root = new GameObject("Decorado").transform;
        int entorno = Layer("Entorno");
        var rnd = new System.Random(level * 7919);
        var seen = new System.Collections.Generic.HashSet<string>();
        foreach (Transform t in GameObject.Find("Nivel").transform)
        {
            var c = t.GetComponent<BoxCollider>();
            if (c == null || c.isTrigger || t.gameObject.layer != entorno || !t.CompareTag("Untagged")) continue;
            if (t.rotation != Quaternion.identity || Mathf.Abs(t.localScale.z - pathDepth) > 0.01f || t.localScale.y > 2.01f || t.localScale.x < 10f) continue;
            Bounds b = c.bounds;
            if (!seen.Add(b.min.x.ToString("0.0") + "/" + b.max.x.ToString("0.0") + "/" + b.max.y.ToString("0.0"))) continue; // overlapping floors
            Apron(root, level, b, t.name);
            float x = b.min.x + 1f;
            string[] kinds = DecoKinds[level - 1];
            while (true)
            {
                string kind = kinds[rnd.Next(kinds.Length)];
                Vector3 size = PropSizes[kind] * Mathf.Lerp(0.9f, 1.15f, (float)rnd.NextDouble());
                if (x + size.x > b.max.x - 0.5f) break;
                float cx = x + size.x / 2f;
                if (cx > ArenaStartX && cx < ArenaEndX) { x = cx + size.x / 2f + 2f; continue; }
                float zc = ApronZ0 + 0.5f + size.z / 2f + (float)rnd.NextDouble() * Mathf.Max(0f, ApronDepth - 1f - size.z);
                Deco(root, culture, prefix, kind, new Vector3(cx, b.max.y, zc), size, Mathf.Lerp(-6f, 6f, (float)rnd.NextDouble()));
                x = cx + size.x / 2f + Mathf.Lerp(0.6f, 3f, (float)rnd.NextDouble());
            }
        }
    }

    static void Apron(Transform root, int level, Bounds floor, string label)
    {
        string culture = Cultures[level - 1], prefix = culture.ToLowerInvariant();
        int nx = Mathf.Max(1, Mathf.RoundToInt(floor.size.x / 4f));
        float w = floor.size.x / nx;
        for (int i = 0; i < nx; i++)
        {
            float cx = floor.min.x + w * (i + 0.5f);
            if (cx > ArenaStartX && cx < ArenaEndX) continue;
            string folder = culture, mesh = prefix + "_platform";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArtFolder + folder + "/" + mesh + ".fbx");
            if (prefab == null) { Debug.LogWarning("Kit piece missing: " + mesh); return; }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
            go.name = "Deco_Explanada_" + label;
            go.transform.localScale = new Vector3(w / 4f, 1f, ApronDepth / 4f);
            go.transform.position = new Vector3(cx, floor.max.y - 1f, ApronZ0 + ApronDepth / 2f);
            Skin(go, 0, KitMaterial(folder, mesh, Color.gray));
        }
    }

    static void Deco(Transform root, string culture, string prefix, string kind, Vector3 basePos, Vector3 size, float yaw)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArtFolder + culture + "/" + prefix + "_" + kind + ".fbx");
        if (prefab == null) { Debug.LogWarning("Prop missing: " + prefix + "_" + kind); return; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root);
        go.name = "Deco_" + kind + "_" + (++propCount);
        Vector3 nominal = PropSizes[kind];
        go.transform.localScale = new Vector3(size.x / nominal.x, size.y / nominal.y, size.z / nominal.z);
        go.transform.SetPositionAndRotation(basePos, Quaternion.Euler(0f, yaw, 0f));
        Skin(go, 0, KitMaterial(culture, prefix + "_" + kind, Color.gray));
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

    // URP Lit, transparent alpha blend, no depth write.
    static Material HazeMaterial(Color c)
    {
        var m = Mat(c);
        m.SetFloat("_Surface", 1f);
        m.SetFloat("_Blend", 0f);
        m.SetOverrideTag("RenderType", "Transparent");
        m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.SetFloat("_Smoothness", 0f);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)RenderQueue.Transparent;
        return m;
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

    // Cylinder lying across the path (axis along Z): a rolling log or a fallen column.
    static void Beam(Transform p, string name, Vector3 centre, float diameter, float length, Vector3 travel, float speed)
    {
        Mover(p, name, "Obstaculo", PrimitiveType.Cylinder, centre, new Vector3(diameter, length / 2f, diameter), travel, speed);
        GameObject.Find(name).transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    // Carts placed side by side from z0 to z1 with no gap, standing on a floor whose top is floorTop.
    static void CartRow(Transform p, string name, float x, float floorTop, float z0, float z1, int count)
    {
        float w = (z1 - z0) / count;
        for (int i = 0; i < count; i++)
            Mover(p, name + "_" + (char)('A' + i), "Obstaculo", PrimitiveType.Cube, new Vector3(x, floorTop + 0.75f, z0 + w * (i + 0.5f)), new Vector3(2f, 1.5f, w), Vector3.right * 5f, 2.5f);
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

    // Solid masonry under a ramp, from the ground (y 0) up to just under the slab, in `steps` blocks whose tops follow the
    // slope at their low edge, so no void shows beneath the walkway (ODD S40). Named Muro_* so they wear the wall kit.
    static void RampBody(Transform p, string name, float x, float y, float w, float h, int steps)
    {
        float x0 = x - w / 2f, sw = w / steps;
        for (int i = 0; i < steps; i++)
        {
            float top = RampTop(x0 + i * sw, x, y, w, h) - RampThickness;
            if (top < 0.25f) continue;
            Solid(p, name + "_" + (i + 1), x0 + (i + 0.5f) * sw, top / 2f, sw, top, Wall, 0f, pathDepth);
        }
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

}
