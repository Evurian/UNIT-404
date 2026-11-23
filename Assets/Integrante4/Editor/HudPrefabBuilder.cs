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

        BuildExtras(root, panel, font);

        System.IO.Directory.CreateDirectory("Assets/Integrante4/Prefabs");
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log("[HudPrefabBuilder] " + PrefabPath + " generado.");
    }

    // Week 13: glyph progress, anomaly bar, chip slots, lock-on mark, minimap and Ada panel.
    static void BuildExtras(GameObject root, RectTransform topRight, Font font)
    {
        var extras = root.AddComponent<HudExtrasView>();
        Color glass = new Color(0f, 0f, 0f, 0.45f);

        // Top left: glyph progress
        RectTransform glyphs = CreateRect("ProgresoGlifos", root.transform);
        SetAnchors(glyphs, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        glyphs.anchoredPosition = new Vector2(32f, -32f);
        glyphs.sizeDelta = new Vector2(300f, 60f);
        AddImage(glyphs.gameObject, glass);
        extras.glyphText = CreateText("Texto", glyphs, font, 30, TextAnchor.MiddleCenter);
        Stretch(extras.glyphText.rectTransform);

        // Anomaly bar under the top-right panel
        RectTransform anomaly = CreateRect("BarraAnomalia", topRight);
        SetAnchors(anomaly, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 1f));
        anomaly.anchoredPosition = new Vector2(20f, -8f);
        anomaly.sizeDelta = new Vector2(380f, 18f);
        AddImage(anomaly.gameObject, new Color(0.12f, 0.12f, 0.12f, 1f));
        RectTransform anomalyFill = CreateRect("Relleno", anomaly);
        SetAnchors(anomalyFill, Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        anomalyFill.offsetMin = anomalyFill.offsetMax = Vector2.zero;
        AddImage(anomalyFill.gameObject, new Color(0.85f, 0.2f, 0.9f, 1f));
        extras.anomalyFill = anomalyFill;

        // Bottom center: 3 chip slots with cost and cooldown
        RectTransform slots = CreateRect("Ranuras", root.transform);
        SetAnchors(slots, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        slots.anchoredPosition = new Vector2(0f, 32f);
        slots.sizeDelta = new Vector2(600f, 120f);
        var layout = slots.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = true;
        extras.chipSlots = new HudExtrasView.ChipSlot[3];
        for (int i = 0; i < 3; i++)
        {
            RectTransform slot = CreateRect("Ranura" + (i + 1), slots);
            AddImage(slot.gameObject, glass);
            var overlayRect = CreateRect("Recarga", slot);
            Stretch(overlayRect);
            Image overlay = AddImage(overlayRect.gameObject, new Color(0f, 0f, 0f, 0.65f));
            overlay.type = Image.Type.Filled;
            overlay.fillMethod = Image.FillMethod.Vertical;
            overlay.fillOrigin = (int)Image.OriginVertical.Top;
            Text label = CreateText("Texto", slot, font, 26, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform);
            extras.chipSlots[i] = new HudExtrasView.ChipSlot { label = label, cooldownOverlay = overlay };
        }

        // Bottom left: minimap placeholder
        RectTransform minimap = CreateRect("Minimapa", root.transform);
        SetAnchors(minimap, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        minimap.anchoredPosition = new Vector2(32f, 32f);
        minimap.sizeDelta = new Vector2(260f, 260f);
        AddImage(minimap.gameObject, new Color(0.15f, 0.2f, 0.2f, 0.6f));
        extras.minimapPanel = minimap.gameObject;

        // Bottom right: Ada panel (mission and hints)
        RectTransform ada = CreateRect("PanelAda", root.transform);
        SetAnchors(ada, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));
        ada.anchoredPosition = new Vector2(-32f, 32f);
        ada.sizeDelta = new Vector2(420f, 180f);
        AddImage(ada.gameObject, new Color(0.86f, 0.77f, 0.58f, 0.85f));
        extras.adaText = CreateText("Mensaje", ada, font, 26, TextAnchor.UpperLeft);
        extras.adaText.color = new Color(0.16f, 0.11f, 0.07f);
        Stretch(extras.adaText.rectTransform, 16f);

        // Center: pinned target mark (Q)
        RectTransform mark = CreateRect("MarcaObjetivo", root.transform);
        SetAnchors(mark, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        mark.anchoredPosition = Vector2.zero;
        mark.sizeDelta = new Vector2(64f, 64f);
        AddImage(mark.gameObject, new Color(1f, 0.3f, 0.2f, 0.8f));
        extras.lockOnMarker = mark.gameObject;
    }

    static void Stretch(RectTransform rect, float margin = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(margin, margin);
        rect.offsetMax = new Vector2(-margin, -margin);
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
