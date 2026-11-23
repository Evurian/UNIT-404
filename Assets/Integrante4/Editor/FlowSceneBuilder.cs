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
        Button(canvas.transform, "BotonSalir", "Salir", new Vector2(0f, -190f), menu.Quit);
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
