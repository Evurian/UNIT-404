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
        BuildStory("Introduccion", StoryScene.Script.Intro, SceneLoader.FirstLevel);
        BuildStory("Despedida", StoryScene.Script.Epilogue, "Creditos");
        BuildStory("Creditos", StoryScene.Script.Credits, "Cierre");
        BuildStory("Cierre", StoryScene.Script.Farewell, SceneLoader.StartScene);
        AssetDatabase.SaveAssets();
    }

    public static void BuildStart()
    {
        var scene = NewScene();
        Canvas canvas = NewCanvas("Canvas");
        Fill(canvas.transform, "Fondo", new Color(0.08f, 0.07f, 0.09f));
        var menu = new GameObject("StartMenu").AddComponent<StartMenu>();

        Label(canvas.transform, "Titulo", "UNIT-404", 120, new Vector2(0f, 200f), new Vector2(1200f, 180f), Parchment);
        Label(canvas.transform, "Subtitulo", "El robot que cruzo las eras", 40, new Vector2(0f, 90f), new Vector2(1200f, 60f), Parchment);
        Button(canvas.transform, "BotonJugar", "Jugar", new Vector2(0f, -60f), menu.Play);
        Button(canvas.transform, "BotonCreditos", "Creditos", new Vector2(0f, -190f), menu.ShowCredits);
        Button(canvas.transform, "BotonSalir", "Salir", new Vector2(0f, -320f), menu.Quit);
        EditorSceneManager.SaveScene(scene, Folder + "Inicio.unity");
    }

    public static void BuildLoading()
    {
        var scene = NewScene();
        Canvas canvas = NewCanvas("Canvas");
        Fill(canvas.transform, "Fondo", new Color(0.08f, 0.07f, 0.09f));
        var loading = new GameObject("LoadingScreen").AddComponent<LoadingScreen>();

        loading.titleText = Label(canvas.transform, "Titulo", "Cargando", 56, new Vector2(0f, 80f), new Vector2(1400f, 90f), Parchment);
        RectTransform back = Rect("BarraFondo", canvas.transform, new Vector2(0f, -40f), new Vector2(900f, 36f));
        back.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);
        RectTransform fill = Rect("BarraRelleno", back, Vector2.zero, Vector2.zero);
        fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(0f, 1f); fill.offsetMin = fill.offsetMax = Vector2.zero;
        fill.gameObject.AddComponent<Image>().color = new Color(1f, 0.48f, 0.1f);
        loading.barFill = fill;
        loading.percentText = Label(canvas.transform, "Porcentaje", "0%", 40, new Vector2(0f, -110f), new Vector2(400f, 60f), Parchment);
        EditorSceneManager.SaveScene(scene, Folder + "Carga.unity");
    }

    static void BuildStory(string name, StoryScene.Script script, string next)
    {
        var scene = NewScene();
        Canvas canvas = NewCanvas("Canvas");
        Fill(canvas.transform, "Fondo", new Color(0.06f, 0.05f, 0.07f));
        var story = new GameObject("StoryScene").AddComponent<StoryScene>();
        story.script = script;
        story.nextScene = next;
        story.text = Label(canvas.transform, "Texto", "", 52, Vector2.zero, new Vector2(1500f, 400f), Parchment);
        Label(canvas.transform, "Ayuda", "Espacio para saltar", 28, new Vector2(0f, -460f), new Vector2(600f, 50f), new Color(1f, 1f, 1f, 0.4f));
        EditorSceneManager.SaveScene(scene, Folder + name + ".unity");
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
        narrator.summaryText = Label(card, "Detalle", "", 38, new Vector2(0f, 20f), new Vector2(700f, 300f), Ink);
        Button(card, "BotonContinuar", "Continuar", new Vector2(0f, -210f), narrator.Continue);
        narrator.summaryCard = card.gameObject;

        var toast = new GameObject("AchievementToast").AddComponent<AchievementToast>();
        RectTransform badge = Rect("Sello", canvas.transform, new Vector2(-30f, -30f), new Vector2(420f, 90f));
        badge.anchorMin = badge.anchorMax = badge.pivot = new Vector2(1f, 1f);
        badge.anchoredPosition = new Vector2(-30f, -30f);
        badge.gameObject.AddComponent<Image>().color = new Color(0.95f, 0.78f, 0.25f);
        toast.label = Label(badge, "Texto", "", 34, Vector2.zero, badge.sizeDelta, Ink);
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

    // Called by LevelSceneBuilder before saving each level scene.
    public static void AddPauseMenu()
    {
        EnsureEventSystem();
        Canvas canvas = NewCanvas("PausaCanvas", 20);
        var menu = new GameObject("PauseMenu").AddComponent<PauseMenu>();

        RectTransform panel = Rect("PanelPausa", canvas.transform, Vector2.zero, Vector2.zero);
        panel.anchorMin = Vector2.zero; panel.anchorMax = Vector2.one; panel.offsetMin = panel.offsetMax = Vector2.zero;
        panel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);
        Label(panel, "Titulo", "Pausa", 90, new Vector2(0f, 220f), new Vector2(800f, 120f), Parchment);
        Button(panel, "BotonReanudar", "Reanudar", new Vector2(0f, 60f), menu.Resume);
        Button(panel, "BotonReiniciar", "Reiniciar nivel", new Vector2(0f, -70f), menu.Restart);
        Button(panel, "BotonInicio", "Ir al inicio", new Vector2(0f, -200f), menu.ToStart);
        menu.panel = panel.gameObject;
    }

    // "Sistema caído" retry screen (ODD S25, T27); LevelSceneBuilder wires the respawn targets.
    public static FallScreen AddFallScreen()
    {
        EnsureEventSystem();
        Canvas canvas = NewCanvas("CaidaCanvas", 25);
        var screen = new GameObject("FallScreen").AddComponent<FallScreen>();

        RectTransform panel = Rect("PanelCaida", canvas.transform, Vector2.zero, Vector2.zero);
        panel.anchorMin = Vector2.zero; panel.anchorMax = Vector2.one; panel.offsetMin = panel.offsetMax = Vector2.zero;
        panel.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.05f, 0.03f, 0.82f);
        Label(panel, "Titulo", "Sistema caído", 90, new Vector2(0f, 140f), new Vector2(1000f, 120f), Parchment);
        Button(panel, "BotonReintentar", "Reintentar desde el punto de control", new Vector2(0f, -40f), screen.Retry);
        foreach (var rt in panel.Find("BotonReintentar").GetComponentsInChildren<RectTransform>()) rt.sizeDelta = new Vector2(900f, 96f);
        screen.panel = panel.gameObject;
        panel.gameObject.SetActive(false);
        return screen;
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

    static void Fill(Transform parent, string name, Color color)
    {
        RectTransform rt = Rect(name, parent, Vector2.zero, Vector2.zero);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        rt.gameObject.AddComponent<Image>().color = color;
    }

    static Text Label(Transform parent, string name, string text, int size, Vector2 pos, Vector2 box, Color color)
    {
        var t = Rect(name, parent, pos, box).gameObject.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = text; t.fontSize = size; t.color = color; t.alignment = TextAnchor.MiddleCenter;
        return t;
    }

    static void Button(Transform parent, string name, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        RectTransform rt = Rect(name, parent, pos, new Vector2(520f, 96f));
        var img = rt.gameObject.AddComponent<Image>();
        img.color = Parchment;
        var button = rt.gameObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = img;
        UnityEventTools.AddPersistentListener(button.onClick, onClick);
        Label(rt, "Texto", label, 44, Vector2.zero, rt.sizeDelta, Ink);
    }
}
