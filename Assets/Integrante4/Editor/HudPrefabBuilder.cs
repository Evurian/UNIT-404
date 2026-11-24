using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Genera Assets/Integrante4/Prefabs/HUD.prefab con el estilo pergamino (ODD S13, S22, T22; sprites de UiSkin).
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

        // Panel superior derecho (GDD 10): vidas, puntaje, combustible y anomalia, marco pergamino (ODD T22).
        RectTransform panel = CreateRect("PanelSuperiorDerecho", root.transform);
        SetAnchors(panel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
        panel.anchoredPosition = new Vector2(-32f, -32f);
        panel.sizeDelta = new Vector2(470f, 200f);
        AddSliced(panel.gameObject, UiSkin.Panel, Color.white);

        var view = root.AddComponent<HudView>();
        view.coreEmptyColor = new Color(0.33f, 0.27f, 0.2f, 0.85f);

        Label("EtiquetaNucleos", panel, font, "NÚCLEOS", new Vector2(0f, 1f), new Vector2(24f, -16f), new Vector2(220f, 22f));

        // Nucleos de vida: 5 diamantes
        RectTransform cores = CreateRect("Nucleos", panel);
        SetAnchors(cores, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        cores.anchoredPosition = new Vector2(22f, -40f);
        cores.sizeDelta = new Vector2(280f, 44f);
        var layout = cores.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        view.lifeCores = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            RectTransform core = CreateRect("Nucleo" + (i + 1), cores);
            core.sizeDelta = new Vector2(44f, 44f);
            view.lifeCores[i] = AddImage(core.gameObject, view.coreActiveColor);
            view.lifeCores[i].sprite = UiSkin.Core;
        }

        // Puntaje
        Label("EtiquetaPuntaje", panel, font, "PUNTAJE", new Vector2(1f, 1f), new Vector2(-24f, -16f), new Vector2(140f, 22f), TextAnchor.MiddleRight);
        view.scoreText = CreateText("Puntaje", panel, font, 38, TextAnchor.MiddleRight);
        view.scoreText.color = UiSkin.Parchment;
        RectTransform scoreRect = view.scoreText.rectTransform;
        SetAnchors(scoreRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
        scoreRect.anchoredPosition = new Vector2(-24f, -38f);
        scoreRect.sizeDelta = new Vector2(140f, 48f);
        view.scoreText.text = "100%";

        // Combustible: barra (anchorMax.x = fraccion) y texto
        Label("EtiquetaCombustible", panel, font, "COMBUSTIBLE", new Vector2(0f, 0f), new Vector2(24f, 102f), new Vector2(220f, 22f));
        view.fuelFill = BarWithFill("BarraCombustible", panel, new Vector2(22f, 72f), new Vector2(300f, 26f), new Color(0.3f, 0.86f, 1f), 0.5f);
        view.fuelText = CreateText("TextoCombustible", panel, font, 26, TextAnchor.MiddleRight);
        view.fuelText.color = UiSkin.Parchment;
        RectTransform fuelRect = view.fuelText.rectTransform;
        SetAnchors(fuelRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));
        fuelRect.anchoredPosition = new Vector2(-24f, 68f);
        fuelRect.sizeDelta = new Vector2(120f, 34f);
        view.fuelText.text = "10 / 20";

        BuildExtras(root, panel, font);

        System.IO.Directory.CreateDirectory("Assets/Integrante4/Prefabs");
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Debug.Log("[HudPrefabBuilder] " + PrefabPath + " generado.");
    }

    // Week 13: glyph progress, anomaly bar, chip slots, lock-on mark, minimap and Ada panel (parchment skin, ODD T22).
    static void BuildExtras(GameObject root, RectTransform topRight, Font font)
    {
        var extras = root.AddComponent<HudExtrasView>();

        // Top left: glyph progress
        RectTransform glyphs = CreateRect("ProgresoGlifos", root.transform);
        SetAnchors(glyphs, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        glyphs.anchoredPosition = new Vector2(32f, -32f);
        glyphs.sizeDelta = new Vector2(330f, 70f);
        AddSliced(glyphs.gameObject, UiSkin.Panel, Color.white);
        extras.glyphText = CreateText("Texto", glyphs, font, 30, TextAnchor.MiddleCenter);
        extras.glyphText.color = UiSkin.Parchment;
        Stretch(extras.glyphText.rectTransform);

        // Anomaly bar at the bottom of the top-right panel
        Label("EtiquetaAnomalia", topRight, font, "ANOMALÍA", new Vector2(0f, 0f), new Vector2(24f, 42f), new Vector2(220f, 22f));
        extras.anomalyFill = BarWithFill("BarraAnomalia", topRight, new Vector2(22f, 18f), new Vector2(426f, 20f), new Color(0.85f, 0.25f, 0.95f), 0f);

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
            AddSliced(slot.gameObject, UiSkin.Slot, Color.white);
            var overlayRect = CreateRect("Recarga", slot);
            Stretch(overlayRect, 3f);
            Image overlay = AddImage(overlayRect.gameObject, new Color(0f, 0f, 0f, 0.65f));
            overlay.type = Image.Type.Filled;
            overlay.fillMethod = Image.FillMethod.Vertical;
            overlay.fillOrigin = (int)Image.OriginVertical.Top;
            Text key = CreateText("Tecla", slot, font, 18, TextAnchor.UpperLeft);
            key.text = (i + 1).ToString();
            key.color = new Color(UiSkin.Parchment.r, UiSkin.Parchment.g, UiSkin.Parchment.b, 0.7f);
            Stretch(key.rectTransform, 10f);
            Text label = CreateText("Texto", slot, font, 26, TextAnchor.MiddleCenter);
            label.color = UiSkin.Parchment;
            Stretch(label.rectTransform);
            extras.chipSlots[i] = new HudExtrasView.ChipSlot { label = label, cooldownOverlay = overlay };
        }

        // Bottom left: framed minimap (Integrante 4 owns the frame, the map content arrives later)
        RectTransform minimap = CreateRect("Minimapa", root.transform);
        SetAnchors(minimap, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
        minimap.anchoredPosition = new Vector2(32f, 32f);
        minimap.sizeDelta = new Vector2(270f, 290f);
        AddSliced(minimap.gameObject, UiSkin.Panel, Color.white);
        RectTransform viewport = CreateRect("Marco", minimap);
        Stretch(viewport, 18f);
        viewport.offsetMax = new Vector2(-18f, -46f);
        AddSliced(viewport.gameObject, UiSkin.Slot, new Color(0.75f, 0.85f, 0.8f, 1f));
        Label("Titulo", minimap, font, "MAPA", new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(200f, 24f), TextAnchor.MiddleCenter);
        extras.minimapPanel = minimap.gameObject;

        // Bottom right: Ada panel (mission and hints) on parchment, with her portrait slot
        RectTransform ada = CreateRect("PanelAda", root.transform);
        SetAnchors(ada, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f));
        ada.anchoredPosition = new Vector2(-32f, 32f);
        ada.sizeDelta = new Vector2(500f, 180f);
        AddSliced(ada.gameObject, UiSkin.Sheet, Color.white);
        RectTransform portrait = CreateRect("RetratoAda", ada);
        SetAnchors(portrait, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
        portrait.anchoredPosition = new Vector2(20f, 0f);
        portrait.sizeDelta = new Vector2(130f, 130f);
        AddSliced(portrait.gameObject, UiSkin.Slot, Color.white);
        Text name = CreateText("Nombre", portrait, font, 26, TextAnchor.LowerCenter);
        name.text = "ADA";
        name.color = UiSkin.Parchment;
        Stretch(name.rectTransform, 8f);
        extras.adaText = CreateText("Mensaje", ada, font, 26, TextAnchor.UpperLeft);
        extras.adaText.color = UiSkin.Ink;
        Stretch(extras.adaText.rectTransform, 20f);
        extras.adaText.rectTransform.offsetMin = new Vector2(166f, 20f);

        // Center: pinned target mark (Q)
        RectTransform mark = CreateRect("MarcaObjetivo", root.transform);
        SetAnchors(mark, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        mark.anchoredPosition = Vector2.zero;
        mark.sizeDelta = new Vector2(64f, 64f);
        AddImage(mark.gameObject, new Color(1f, 0.3f, 0.2f, 0.8f)).sprite = UiSkin.Core;
        extras.lockOnMarker = mark.gameObject;
    }

    // Small caps label in parchment.
    static Text Label(string name, Transform parent, Font font, string text, Vector2 anchor, Vector2 pos, Vector2 size, TextAnchor align = TextAnchor.MiddleLeft)
    {
        Text t = CreateText(name, parent, font, 18, align);
        t.text = text;
        t.color = new Color(UiSkin.Parchment.r, UiSkin.Parchment.g, UiSkin.Parchment.b, 0.85f);
        SetAnchors(t.rectTransform, anchor, anchor, anchor);
        t.rectTransform.anchoredPosition = pos;
        t.rectTransform.sizeDelta = size;
        return t;
    }

    // Framed bar anchored bottom-left; returns the fill whose anchorMax.x the views drive.
    static RectTransform BarWithFill(string name, Transform parent, Vector2 pos, Vector2 size, Color color, float fraction)
    {
        RectTransform bar = CreateRect(name, parent);
        SetAnchors(bar, Vector2.zero, Vector2.zero, Vector2.zero);
        bar.anchoredPosition = pos;
        bar.sizeDelta = size;
        AddSliced(bar.gameObject, UiSkin.Bar, Color.white);
        RectTransform fill = CreateRect("Relleno", bar);
        SetAnchors(fill, Vector2.zero, new Vector2(fraction, 1f), new Vector2(0f, 0.5f));
        fill.offsetMin = new Vector2(3f, 3f);
        fill.offsetMax = new Vector2(-3f, -3f);
        AddSliced(fill.gameObject, UiSkin.Fill, color);
        return fill;
    }

    static Image AddSliced(GameObject go, Sprite sprite, Color color)
    {
        Image image = AddImage(go, color);
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        return image;
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
