using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Genera Assets/Integrante4/Prefabs/HUD.prefab (arte gris; el estilo final llega en la semana 14).
public static class HudPrefabBuilder
{
    public const string PrefabPath = "Assets/Integrante4/Prefabs/HUD.prefab";

    [MenuItem("UNIT-404/Integrante 4/Crear prefab HUD")]
    public static void Build()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var root = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Panel superior derecho (GDD 10): vidas, puntaje y combustible.
        RectTransform panel = CreateRect("PanelSuperiorDerecho", root.transform);
        SetAnchors(panel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
        panel.anchoredPosition = new Vector2(-32f, -32f);
        panel.sizeDelta = new Vector2(420f, 150f);
        AddImage(panel.gameObject, new Color(0f, 0f, 0f, 0.45f));

        var view = root.AddComponent<HudView>();

        // Nucleos de vida
        RectTransform cores = CreateRect("Nucleos", panel);
        SetAnchors(cores, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        cores.anchoredPosition = new Vector2(20f, -18f);
        cores.sizeDelta = new Vector2(260f, 40f);
        var layout = cores.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        view.lifeCores = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            RectTransform core = CreateRect("Nucleo" + (i + 1), cores);
            core.sizeDelta = new Vector2(36f, 36f);
            view.lifeCores[i] = AddImage(core.gameObject, view.coreActiveColor);
        }

        // Puntaje
        view.scoreText = CreateText("Puntaje", panel, font, 34, TextAnchor.MiddleRight);
        RectTransform scoreRect = view.scoreText.rectTransform;
        SetAnchors(scoreRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
        scoreRect.anchoredPosition = new Vector2(-20f, -14f);
        scoreRect.sizeDelta = new Vector2(120f, 44f);
        view.scoreText.text = "100%";

        // Combustible: barra (anchorMax.x = fraccion) y texto
        RectTransform bar = CreateRect("BarraCombustible", panel);
        SetAnchors(bar, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        bar.anchoredPosition = new Vector2(20f, 24f);
        bar.sizeDelta = new Vector2(260f, 22f);
        AddImage(bar.gameObject, new Color(0.12f, 0.12f, 0.12f, 1f));

        RectTransform fill = CreateRect("Relleno", bar);
        SetAnchors(fill, Vector2.zero, new Vector2(0.5f, 1f), new Vector2(0f, 0.5f));
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;
        AddImage(fill.gameObject, new Color(0.25f, 0.85f, 1f, 1f));
        view.fuelFill = fill;

        view.fuelText = CreateText("TextoCombustible", panel, font, 24, TextAnchor.MiddleRight);
        RectTransform fuelRect = view.fuelText.rectTransform;
        SetAnchors(fuelRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));
        fuelRect.anchoredPosition = new Vector2(-20f, 16f);
        fuelRect.sizeDelta = new Vector2(110f, 36f);
        view.fuelText.text = "10 / 20";

        System.IO.Directory.CreateDirectory("Assets/Integrante4/Prefabs");
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log("[HudPrefabBuilder] " + PrefabPath + " generado.");
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 pivot)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.pivot = pivot;
    }

    static Image AddImage(GameObject go, Color color)
    {
        var image = go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    static Text CreateText(string name, Transform parent, Font font, int size, TextAnchor anchor)
    {
        RectTransform rect = CreateRect(name, parent);
        var text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = anchor;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }
}
