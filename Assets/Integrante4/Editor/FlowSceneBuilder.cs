using System.Linq;
using TMPro;
using Unit404.Integrante4.Core;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Generates the flow scenes (Inicio, Carga) and the pause menu used by every level.
public static class FlowSceneBuilder
{
    const string Folder = "Assets/Integrante4/Scenes/";
    static readonly Color Parchment = new Color(0.86f, 0.77f, 0.58f);
    static readonly Color Ink = new Color(0.16f, 0.11f, 0.07f);

    [MenuItem("UNIT-404/Integrante 4/Crear pantallas de inicio y carga")]
    public static void BuildFlowScenes()
    {
        BuildStart();
        BuildLoading();
        BuildStory("Introduccion", StoryScene.Script.Intro);
        BuildStory("Despedida", StoryScene.Script.Epilogue);
        BuildStory("Creditos", StoryScene.Script.Credits);
        BuildStory("Cierre", StoryScene.Script.Farewell);
        ApplyBuildSettings();
        AssetDatabase.SaveAssets();
    }

    // Build list = SceneFlow.BuildOrder, Inicio first (ODD S31, T33). Scenes of other members already in the
    // list stay, disabled, so the build opens on Inicio and only my flow ships from this branch.
    [MenuItem("UNIT-404/Integrante 4/Conectar escenas (Build Settings)")]
    public static void ApplyBuildSettings()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        foreach (string name in SceneFlow.BuildOrder)
            scenes.Add(new EditorBuildSettingsScene(Folder + name + ".unity", true));
        foreach (var other in EditorBuildSettings.scenes)
            if (!scenes.Exists(s => s.path == other.path))
                scenes.Add(new EditorBuildSettingsScene(other.path, false));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ---- Flow screens (ODD S33, T35): level art behind a vignette, Metamorphous titles, parchment buttons ----
    static readonly Color Amber = new Color32(0xFF, 0x9A, 0x2E, 0xFF);
    static readonly Color Shade = new Color(0.04f, 0.03f, 0.02f, 0.86f);

    // Inicio (ODD S43/S44, T43): the four cultures rotate behind a left column; everything in the column shares
    // one left edge (MenuLeft) so title, rule, subtitle, buttons and the culture caption line up.
    const float MenuLeft = -820f; // 140 px from the left edge of the 1920 x 1080 reference
    static readonly string[] CultureArt = { "Mesopotamia", "Egipto", "Grecia", "China" };

    public static void BuildStart()
    {
        var scene = NewScene();
        Canvas canvas = NewCanvas("Canvas");
        var menu = new GameObject("StartMenu").AddComponent<StartMenu>();
        var cycle = new GameObject("BackgroundCycle").AddComponent<BackgroundCycle>();

        RectTransform stage = Rect("Escenario", canvas.transform, Vector2.zero, Vector2.zero);
        stage.anchorMin = Vector2.zero; stage.anchorMax = Vector2.one;
        stage.offsetMin = new Vector2(-60f, -34f); stage.offsetMax = new Vector2(60f, 34f);
        stage.gameObject.AddComponent<ScreenMotion>();
        cycle.slides = CultureArt.Select(ScreenArtBuilder.Background).ToArray();
        cycle.front = StageLayer(stage, "Fondo", cycle.slides[0], 1f);
        cycle.back = StageLayer(stage, "FondoSiguiente", cycle.slides[1], 0f);
        cycle.names = CultureArt;
        cycle.accents = new Color[4];
        for (int i = 0; i < 4; i++) ColorUtility.TryParseHtmlString(CulturePalette.AccentHex(i + 1), out cycle.accents[i]);

        RectTransform v = Rect("Vineta", canvas.transform, Vector2.zero, Vector2.zero);
        v.anchorMin = Vector2.zero; v.anchorMax = Vector2.one; v.offsetMin = v.offsetMax = Vector2.zero;
        var vignette = v.gameObject.AddComponent<Image>();
        vignette.sprite = UiSkin.Vignette; vignette.raycastTarget = false;

        // shade eased from the left edge into the art: no hard box, no edge line
        RectTransform column = Rect("Columna", canvas.transform, Vector2.zero, new Vector2(1250f, 0f));
        column.anchorMin = new Vector2(0f, 0f); column.anchorMax = new Vector2(0f, 1f); column.pivot = new Vector2(0f, 0.5f);
        column.anchoredPosition = Vector2.zero;
        var shade = column.gameObject.AddComponent<Image>();
        shade.sprite = UiSkin.SideShade; shade.raycastTarget = false;

        var title = Tmp(canvas.transform, "Titulo", "UNIT-404", 150, AtLeft(900f, 235f), new Vector2(900f, 180f), Parchment, TextAlignmentOptions.Left, true);
        title.textWrappingMode = TextWrappingModes.NoWrap;
        title.margin = Vector4.zero;
        Plain(Rect("Linea", canvas.transform, AtLeft(360f, 138f), new Vector2(360f, 4f)).gameObject, Amber);
        Tmp(canvas.transform, "Subtitulo", "El robot que cruzó las eras", 40, AtLeft(900f, 92f), new Vector2(900f, 56f),
            new Color(Parchment.r, Parchment.g, Parchment.b, 0.88f), TextAlignmentOptions.Left, false, body: true);
        MenuButton(canvas.transform, "BotonJugar", "Jugar", AtLeft(440f, -40f), menu.Play);
        MenuButton(canvas.transform, "BotonCreditos", "Créditos", AtLeft(440f, -145f), menu.ShowCredits);
        MenuButton(canvas.transform, "BotonSalir", "Salir", AtLeft(440f, -250f), menu.Quit);

        // culture now on screen + one dot per culture, both driven by BackgroundCycle
        var caption = Tmp(canvas.transform, "CulturaActual", CultureArt[0].ToUpperInvariant(), 30, AtLeft(600f, -415f), new Vector2(600f, 44f),
            cycle.accents[0], TextAlignmentOptions.Left, false, body: true);
        caption.characterSpacing = 16f;
        cycle.caption = caption;
        cycle.dots = new Image[4];
        for (int i = 0; i < 4; i++)
        {
            var dot = Rect("Punto_" + (i + 1), canvas.transform, new Vector2(MenuLeft + 9f + i * 30f, -458f), new Vector2(18f, 18f)).gameObject.AddComponent<Image>();
            dot.sprite = UiSkin.Core; dot.raycastTarget = false;
            cycle.dots[i] = dot;
        }
        cycle.Show(0f);
        EditorSceneManager.SaveScene(scene, Folder + "Inicio.unity");
    }

    // Centre of a centre-anchored rect whose left edge sits on MenuLeft.
    static Vector2 AtLeft(float width, float y) => new Vector2(MenuLeft + width / 2f, y);

    static Image StageLayer(RectTransform stage, string name, Sprite sprite, float alpha)
    {
        RectTransform rt = Rect(name, stage, Vector2.zero, Vector2.zero);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = new Color(1f, 1f, 1f, alpha);
        img.raycastTarget = false;
        return img;
    }

    public static void BuildLoading()
    {
        var scene = NewScene();
        Canvas canvas = NewCanvas("Canvas");
        var loading = new GameObject("LoadingScreen").AddComponent<LoadingScreen>();
        loading.background = Backdrop(canvas.transform, "Inicio");
        loading.cultureBackgrounds = new[] { "Inicio", "Mesopotamia", "Egipto", "Grecia", "China" }
            .Select(ScreenArtBuilder.Background).ToArray();

        var label = Tmp(canvas.transform, "Etiqueta", "CARGANDO", 28, new Vector2(0f, -225f), new Vector2(900f, 40f), Amber, TextAlignmentOptions.Center, false, body: true);
        label.characterSpacing = 18f;
        loading.titleText = Tmp(canvas.transform, "Titulo", "Cargando", 76, new Vector2(0f, -315f), new Vector2(1400f, 100f), Parchment, TextAlignmentOptions.Center, true);
        RectTransform back = Rect("BarraFondo", canvas.transform, new Vector2(0f, -395f), new Vector2(900f, 26f));
        var trough = back.gameObject.AddComponent<Image>();
        trough.sprite = UiSkin.Bar; trough.type = Image.Type.Sliced;
        RectTransform fill = Rect("BarraRelleno", back, Vector2.zero, Vector2.zero);
        fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(0f, 1f); fill.offsetMin = new Vector2(4f, 4f); fill.offsetMax = new Vector2(-4f, -4f);
        var bar = fill.gameObject.AddComponent<Image>();
        bar.sprite = UiSkin.Fill; bar.type = Image.Type.Sliced; bar.color = Amber;
        loading.barFill = fill;
        loading.percentText = Tmp(canvas.transform, "Porcentaje", "0%", 30, new Vector2(0f, -445f), new Vector2(400f, 44f),
            new Color(Parchment.r, Parchment.g, Parchment.b, 0.8f), TextAlignmentOptions.Center, false, body: true);
        EditorSceneManager.SaveScene(scene, Folder + "Carga.unity");
    }

    static readonly System.Collections.Generic.Dictionary<string, (string art, string header)> StoryLook =
        new System.Collections.Generic.Dictionary<string, (string, string)>
        {
            { "Introduccion", ("Mesopotamia", "INTRODUCCIÓN") },
            { "Despedida", ("China", "EPÍLOGO") },
            { "Creditos", ("Grecia", "CRÉDITOS") },
            { "Cierre", ("Egipto", "FIN") },
        };

    static void BuildStory(string name, StoryScene.Script script)
    {
        var scene = NewScene();
        Canvas canvas = NewCanvas("Canvas");
        var look = StoryLook[name];
        Backdrop(canvas.transform, look.art);
        var story = new GameObject("StoryScene").AddComponent<StoryScene>();
        story.script = script;
        story.nextScene = SceneFlow.Next(name);

        Band(canvas.transform, "BandaSuperior", Shade, 1f, 120f);
        Band(canvas.transform, "BandaInferior", Shade, 0f, 120f);
        Rule(canvas.transform, "LineaSuperior", new Color(Amber.r, Amber.g, Amber.b, 0.8f), 1f, -120f);
        Rule(canvas.transform, "LineaInferior", new Color(Amber.r, Amber.g, Amber.b, 0.8f), 0f, 120f);
        var header = Tmp(canvas.transform, "Encabezado", look.header, 34, new Vector2(0f, 480f), new Vector2(1200f, 60f), Amber, TextAlignmentOptions.Center, false);
        header.characterSpacing = 14f;
        story.text = Tmp(canvas.transform, "Texto", "", 56, new Vector2(0f, 0f), new Vector2(1500f, 420f), Parchment, TextAlignmentOptions.Center, true);
        Tmp(canvas.transform, "Ayuda", "Espacio para saltar", 24, new Vector2(760f, -480f), new Vector2(360f, 40f),
            new Color(1f, 1f, 1f, 0.5f), TextAlignmentOptions.Right, false, body: true);
        EditorSceneManager.SaveScene(scene, Folder + name + ".unity");
    }

    // Level art behind every flow screen: slightly oversized so ScreenMotion can drift it, then a vignette.
    static Image Backdrop(Transform canvas, string art)
    {
        RectTransform rt = Rect("Fondo", canvas, Vector2.zero, Vector2.zero);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-60f, -34f); rt.offsetMax = new Vector2(60f, 34f);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = ScreenArtBuilder.Background(art);
        img.color = img.sprite != null ? Color.white : new Color(0.08f, 0.07f, 0.09f);
        img.raycastTarget = false;
        rt.gameObject.AddComponent<ScreenMotion>();
        RectTransform v = Rect("Vineta", canvas, Vector2.zero, Vector2.zero);
        v.anchorMin = Vector2.zero; v.anchorMax = Vector2.one; v.offsetMin = v.offsetMax = Vector2.zero;
        var vignette = v.gameObject.AddComponent<Image>();
        vignette.sprite = UiSkin.Vignette;
        vignette.raycastTarget = false;
        return img;
    }

    static void Plain(GameObject go, Color color)
    {
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    // TextMeshPro label: Metamorphous (titles, story) or the body font; `glow` uses the outlined title material.
    static TextMeshProUGUI Tmp(Transform parent, string name, string text, float size, Vector2 pos, Vector2 box, Color color,
        TextAlignmentOptions align, bool glow, bool body = false)
    {
        var t = Rect(name, parent, pos, box).gameObject.AddComponent<TextMeshProUGUI>();
        t.font = body ? ScreenArtBuilder.BodyFont : ScreenArtBuilder.TitleFont;
        if (glow && !body) t.fontSharedMaterial = TitleMaterial();
        t.text = text; t.fontSize = size; t.color = color; t.alignment = align;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.raycastTarget = false;
        return t;
    }

    // Ink outline and a soft amber underlay on the title font (saved next to the font asset).
    static Material TitleMaterial()
    {
        const string path = "Assets/Integrante4/Art/Fonts/Metamorphous Titulo.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;
        mat = new Material(ScreenArtBuilder.TitleFont.material) { name = "Metamorphous Titulo" };
        mat.SetFloat("_OutlineWidth", 0.12f);
        mat.SetColor("_OutlineColor", new Color(0.1f, 0.06f, 0.03f, 1f));
        mat.EnableKeyword("UNDERLAY_ON");
        mat.SetColor("_UnderlayColor", new Color(0.55f, 0.28f, 0.05f, 0.6f));
        mat.SetFloat("_UnderlayDilate", 0.3f);
        mat.SetFloat("_UnderlaySoftness", 0.6f);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static void MenuButton(Transform parent, string name, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        RectTransform rt = Rect(name, parent, pos, new Vector2(440f, 84f));
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = UiSkin.Sheet;
        img.type = Image.Type.Sliced;
        var button = rt.gameObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = img;
        var colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.86f, 0.62f);
        colors.pressedColor = new Color(0.85f, 0.7f, 0.45f);
        button.colors = colors;
        UnityEventTools.AddPersistentListener(button.onClick, onClick);
        var t = Tmp(rt, "Texto", label, 44, Vector2.zero, rt.sizeDelta, Ink, TextAlignmentOptions.Center, false);
        t.raycastTarget = false;
    }

    // Adds Ada's narrator, the summary card and the achievement toast to a level scene.
    public static void AddNarrator(int level, string nextScene)
    {
        Canvas canvas = NewCanvas("NarradorCanvas", 15);
        var narrator = new GameObject("LevelNarrator").AddComponent<LevelNarrator>();
        narrator.level = level;
        narrator.nextScene = nextScene;

        RectTransform card = Rect("TarjetaResumen", canvas.transform, Vector2.zero, new Vector2(800f, 560f));
        card.gameObject.AddComponent<Image>().color = new Color(Parchment.r, Parchment.g, Parchment.b, 0.95f);
        Label(card, "Titulo", "Resumen del nivel", 52, new Vector2(0f, 220f), new Vector2(700f, 80f), Ink);
        narrator.summaryText = Label(card, "Detalle", "", 34, new Vector2(0f, 20f), new Vector2(700f, 300f), Ink, body: true);
        FitText(narrator.summaryText, 22f);
        Button(card, "BotonContinuar", "Continuar", new Vector2(0f, -210f), narrator.Continue);
        narrator.summaryCard = card.gameObject;

        var toast = new GameObject("AchievementToast").AddComponent<AchievementToast>();
        RectTransform badge = Rect("Sello", canvas.transform, new Vector2(-30f, -30f), new Vector2(420f, 90f));
        badge.anchorMin = badge.anchorMax = badge.pivot = new Vector2(1f, 1f);
        badge.anchoredPosition = new Vector2(-30f, -30f);
        badge.gameObject.AddComponent<Image>().color = new Color(0.95f, 0.78f, 0.25f);
        toast.label = Label(badge, "Texto", "", 30, Vector2.zero, badge.sizeDelta, Ink, body: true);
        FitText(toast.label, 18f);
        toast.badge = badge;
        narrator.toast = toast;

        // Culture transition overlay: last child of the canvas so it covers the summary card.
        RectTransform fade = Rect("TransicionCultura", canvas.transform, Vector2.zero, Vector2.zero);
        fade.anchorMin = Vector2.zero; fade.anchorMax = Vector2.one; fade.offsetMin = fade.offsetMax = Vector2.zero;
        var fadeImage = fade.gameObject.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        var transition = new GameObject("CultureTransition").AddComponent<CultureTransition>();
        transition.overlay = fadeImage;
        narrator.transition = transition;
    }

    // Letterboxed culture transition (ODD S27, T34): title card, Ada's subtitle in the lower band, skip hint, black fade.
    public static CinematicDirector AddCinematic(int level, string nextScene, Transform[] path, Transform[] lookAt)
    {
        ColorUtility.TryParseHtmlString(CulturePalette.AccentHex(level), out Color accent);
        Canvas canvas = NewCanvas("CinematicaCanvas", 30);
        var director = new GameObject("CinematicDirector").AddComponent<CinematicDirector>();
        director.level = level;
        director.nextScene = nextScene;
        director.path = path;
        director.lookAt = lookAt;

        Band(canvas.transform, "BandaSuperior", Color.black, 1f, 120f);
        Band(canvas.transform, "BandaInferior", Color.black, 0f, 150f);

        RectTransform card = Rect("Titulo", canvas.transform, new Vector2(0f, 60f), new Vector2(1400f, 320f));
        var group = card.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        Card(card, new Vector2(0f, 10f), new Vector2(1000f, 300f)); // framed dark card: the title reads over any landscape
        var number = Tmp(card, "Nivel", "NIVEL " + level, 44, new Vector2(0f, 95f), new Vector2(1200f, 60f), accent, TextAlignmentOptions.Center, true);
        number.characterSpacing = 18f;
        var culture = Tmp(card, "Cultura", CulturePalette.CultureName(level).ToUpperInvariant(), 140, new Vector2(0f, -5f), new Vector2(920f, 170f), Parchment, TextAlignmentOptions.Center, true);
        culture.textWrappingMode = TextWrappingModes.NoWrap;
        culture.enableAutoSizing = true; // long names (MESOPOTAMIA) shrink to stay inside the card
        culture.fontSizeMin = 80f;
        culture.fontSizeMax = 140f;
        var line = Rect("Linea", card, new Vector2(0f, -100f), new Vector2(520f, 4f)).gameObject.AddComponent<Image>();
        line.color = accent;
        line.raycastTarget = false;
        director.title = group;

        director.subtitle = Tmp(canvas.transform, "Subtitulo", "", 38, new Vector2(0f, -465f), new Vector2(1600f, 100f), Parchment, TextAlignmentOptions.Center, false, body: true);
        Tmp(canvas.transform, "Ayuda", "Espacio para saltar", 24, new Vector2(780f, -505f), new Vector2(360f, 40f), new Color(1f, 1f, 1f, 0.45f), TextAlignmentOptions.Right, false, body: true);

        RectTransform fade = Rect("Fundido", canvas.transform, Vector2.zero, Vector2.zero);
        fade.anchorMin = Vector2.zero; fade.anchorMax = Vector2.one; fade.offsetMin = fade.offsetMax = Vector2.zero;
        director.fade = fade.gameObject.AddComponent<Image>();
        director.fade.color = Color.black;
        director.fade.raycastTarget = false;
        return director;
    }

    // Called by LevelSceneBuilder before saving each level scene.
    public static PauseMenu AddPauseMenu()
    {
        EnsureEventSystem();
        Canvas canvas = NewCanvas("PausaCanvas", 20);
        var menu = new GameObject("PauseMenu").AddComponent<PauseMenu>();

        RectTransform panel = Rect("PanelPausa", canvas.transform, Vector2.zero, Vector2.zero);
        panel.anchorMin = Vector2.zero; panel.anchorMax = Vector2.one; panel.offsetMin = panel.offsetMax = Vector2.zero;
        panel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);
        Card(panel, new Vector2(0f, 10f), new Vector2(760f, 640f));
        Label(panel, "Titulo", "Pausa", 90, new Vector2(0f, 220f), new Vector2(800f, 120f), Parchment);
        Button(panel, "BotonReanudar", "Reanudar", new Vector2(0f, 60f), menu.Resume);
        Button(panel, "BotonReiniciar", "Reiniciar nivel", new Vector2(0f, -70f), menu.Restart);
        Button(panel, "BotonInicio", "Ir al inicio", new Vector2(0f, -200f), menu.ToStart);
        menu.panel = panel.gameObject;
        return menu;
    }

    // "Sistema caído" retry screen (ODD S25, T27; redesigned S34/S36, T36): a full screen in the culture's dark
    // shade with its accent, not an overlay. LevelSceneBuilder wires the respawn targets.
    public static FallScreen AddFallScreen(int level)
    {
        EnsureEventSystem();
        Canvas canvas = NewCanvas("CaidaCanvas", 25);
        var screen = new GameObject("FallScreen").AddComponent<FallScreen>();
        ColorUtility.TryParseHtmlString(CulturePalette.FallHex(level), out Color shade);
        ColorUtility.TryParseHtmlString(CulturePalette.AccentHex(level), out Color accent);
        shade.a = 0.97f;

        RectTransform panel = Rect("PanelCaida", canvas.transform, Vector2.zero, Vector2.zero);
        panel.anchorMin = Vector2.zero; panel.anchorMax = Vector2.one; panel.offsetMin = panel.offsetMax = Vector2.zero;
        panel.gameObject.AddComponent<Image>().color = shade;
        // darker bands top and bottom frame the screen like a letterbox
        Color band = Color.Lerp(shade, Color.black, 0.55f);
        Band(panel, "BandaSuperior", band, 1f, 140f);
        Band(panel, "BandaInferior", band, 0f, 140f);
        Rule(panel, "LineaSuperior", accent, 1f, -140f);
        Rule(panel, "LineaInferior", accent, 0f, 140f);

        Card(panel, new Vector2(0f, 10f), new Vector2(1180f, 560f));
        var emblem = Rect("Emblema", panel, new Vector2(0f, 230f), new Vector2(72f, 72f)).gameObject.AddComponent<Image>();
        emblem.sprite = UiSkin.Core;
        emblem.color = accent;
        emblem.raycastTarget = false;
        Label(panel, "Cultura", CulturePalette.CultureName(level).ToUpperInvariant(), 34, new Vector2(0f, 160f), new Vector2(1000f, 50f), accent, body: true);
        Label(panel, "Titulo", "Sistema caído", 96, new Vector2(0f, 80f), new Vector2(1100f, 120f), Parchment);
        Label(panel, "Subtitulo", "Señal perdida. UNIT-404 se restaura desde el último punto seguro.", 34,
            new Vector2(0f, -10f), new Vector2(1100f, 50f), new Color(Parchment.r, Parchment.g, Parchment.b, 0.75f), body: true);
        Button(panel, "BotonReintentar", "Reintentar desde el punto de control", new Vector2(0f, -130f), screen.Retry);
        foreach (var rt in panel.Find("BotonReintentar").GetComponentsInChildren<RectTransform>()) rt.sizeDelta = new Vector2(900f, 96f);
        screen.panel = panel.gameObject;
        panel.gameObject.SetActive(false);
        return screen;
    }

    // Full-width strip anchored to the top (anchorY = 1) or bottom (anchorY = 0) edge.
    static void Band(Transform parent, string name, Color color, float anchorY, float height)
    {
        RectTransform rt = Rect(name, parent, Vector2.zero, new Vector2(0f, height));
        rt.anchorMin = new Vector2(0f, anchorY); rt.anchorMax = new Vector2(1f, anchorY);
        rt.pivot = new Vector2(0.5f, anchorY);
        rt.anchoredPosition = Vector2.zero;
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    // Thin accent line across the screen at `offset` from the top/bottom edge.
    static void Rule(Transform parent, string name, Color color, float anchorY, float offset)
    {
        RectTransform rt = Rect(name, parent, Vector2.zero, new Vector2(0f, 4f));
        rt.anchorMin = new Vector2(0f, anchorY); rt.anchorMax = new Vector2(1f, anchorY);
        rt.anchoredPosition = new Vector2(0f, offset);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    // ---- UI helpers ----
    static UnityEngine.SceneManagement.Scene NewScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = Color.black;
        EnsureEventSystem();
        return scene;
    }

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    static Canvas NewCanvas(string name, int order = 0)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = order;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    static RectTransform Rect(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }


    // Dark glass card with the parchment double frame (UiSkin.Panel) behind a menu.
    static void Card(Transform parent, Vector2 pos, Vector2 size)
    {
        var img = Rect("Marco", parent, pos, size).gameObject.AddComponent<Image>();
        img.sprite = UiSkin.Panel;
        img.type = Image.Type.Sliced;
        img.raycastTarget = false;
    }

    // Overlay label (ODD S48, T45): Metamorphous with the outlined title material, or Kenney Future for body copy.
    static TextMeshProUGUI Label(Transform parent, string name, string text, int size, Vector2 pos, Vector2 box, Color color, bool body = false)
    {
        return Tmp(parent, name, text, size, pos, box, color, TextAlignmentOptions.Center, glow: !body, body: body);
    }

    // Kenney Future is wide: shrink long summaries and achievement names instead of overflowing the card.
    static void FitText(TMP_Text text, float min)
    {
        text.enableAutoSizing = true;
        text.fontSizeMin = min;
        text.fontSizeMax = text.fontSize;
    }

    static void Button(Transform parent, string name, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        RectTransform rt = Rect(name, parent, pos, new Vector2(520f, 96f));
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = UiSkin.Sheet; // parchment sheet with an ink frame (ODD T22)
        img.type = Image.Type.Sliced;
        img.color = Color.white;
        var button = rt.gameObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = img;
        UnityEventTools.AddPersistentListener(button.onClick, onClick);
        Tmp(rt, "Texto", label, 44, Vector2.zero, rt.sizeDelta, Ink, TextAlignmentOptions.Center, false);
    }
}
