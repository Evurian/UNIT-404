using System.IO;
using UnityEditor;
using UnityEngine;

// Parchment UI skin (ODD S13, S22, T22): procedural sprites drawn once into Assets/Integrante4/Art/UI and imported as
// 9-slice sprites. Palette of the art style sheet: parchment #DBC594 on dark ink-brown #291C12.
public static class UiSkin
{
    const string Folder = "Assets/Integrante4/Art/UI/";
    public static readonly Color Parchment = new Color32(0xDB, 0xC5, 0x94, 0xFF);
    public static readonly Color Ink = new Color32(0x29, 0x1C, 0x12, 0xFF);
    static readonly Color InkLine = new Color32(0x4A, 0x34, 0x20, 0xFF);

    // Framed dark glass panel: parchment double frame and corner diamonds.
    public static Sprite Panel => Get("ui_panel", 64, 20, (x, y, s) =>
    {
        float d = RoundRect(x, y, s, 9f);
        if (d > 0f) return Clear;
        if (d > -3f) return Parchment;
        if (d > -5f) return new Color(Ink.r, Ink.g, Ink.b, 0.92f);
        if (d > -6f) return new Color(Parchment.r, Parchment.g, Parchment.b, 0.55f);
        if (Corner(x, y, s, 11f, 4f)) return Parchment;
        return new Color(Ink.r, Ink.g, Ink.b, 0.88f);
    });

    // Parchment sheet with an ink frame (Ada panel, buttons, story cards).
    public static Sprite Sheet => Get("ui_parchment", 64, 18, (x, y, s) =>
    {
        float d = RoundRect(x, y, s, 8f);
        if (d > 0f) return Clear;
        if (d > -3f) return InkLine;
        if (d > -5f) return Parchment;
        if (d > -6f) return new Color(InkLine.r, InkLine.g, InkLine.b, 0.6f);
        float grain = Mathf.PerlinNoise(x * 0.21f, y * 0.21f) * 0.08f - 0.04f;
        return new Color(Parchment.r + grain, Parchment.g + grain, Parchment.b + grain * 0.6f, 1f);
    });

    // Chip slot / minimap frame: dark fill, thin parchment frame with notched corners.
    public static Sprite Slot => Get("ui_slot", 64, 16, (x, y, s) =>
    {
        float d = RoundRect(x, y, s, 4f);
        if (d > 0f) return Clear;
        if (Corner(x, y, s, 4f, 6f) && d > -8f) return Parchment;
        if (d > -2f) return Parchment;
        return new Color(Ink.r, Ink.g, Ink.b, 0.78f);
    });

    // Bar trough (fuel, anomaly): dark with a parchment rim.
    public static Sprite Bar => Get("ui_bar", 32, 7, (x, y, s) =>
    {
        float d = RoundRect(x, y, s, 5f);
        if (d > 0f) return Clear;
        if (d > -2f) return Parchment;
        return new Color(0.07f, 0.05f, 0.03f, 0.95f);
    });

    // Bar fill: white, lighter on top (tinted by Image.color).
    public static Sprite Fill => Get("ui_fill", 16, 4, (x, y, s) =>
    {
        float d = RoundRect(x, y, s, 3f);
        if (d > 0f) return Clear;
        float v = Mathf.Lerp(0.72f, 1f, y / (float)s);
        return new Color(v, v, v, 1f);
    });

    // Life core: faceted diamond, white so HudView tints it (active / empty colour).
    public static Sprite Core => Get("ui_core", 64, 0, (x, y, s) =>
    {
        float cx = x - s / 2f + 0.5f, cy = y - s / 2f + 0.5f, r = s * 0.46f;
        float m = Mathf.Abs(cx) + Mathf.Abs(cy);
        if (m > r) return Clear;
        if (m > r - 4f) return new Color(0.55f, 0.55f, 0.55f, 1f);
        float facet = (cx < 0f ? 0.1f : 0f) + (cy > 0f ? 0.12f : 0f);
        float v = 0.78f + facet + (m < r * 0.35f ? 0.1f : 0f);
        return new Color(v, v, v, 1f);
    });

    // Full-screen vignette over background art (T35): clear centre, ink edges, heavier at the bottom where text sits.
    public static Sprite Vignette => Get("ui_vignette", 256, 0, (x, y, s) =>
    {
        float u = (x + 0.5f) / s - 0.5f, v = (y + 0.5f) / s - 0.5f;
        float radial = Mathf.Clamp01((Mathf.Sqrt(u * u + v * v) - 0.3f) / 0.45f);
        radial = radial * radial * (3f - 2f * radial);
        float bottom = Mathf.Clamp01(0.2f - v);
        float a = Mathf.Clamp01(radial * 0.5f + bottom * 0.45f + 0.1f);
        return new Color(0.04f, 0.03f, 0.02f, a);
    });

    // Horizontal shade for the Inicio menu column (T43): dense ink on the left, eased to clear on the right.
    public static Sprite SideShade => Get("ui_side_shade", 256, 0, (x, y, s) =>
    {
        float u = (x + 0.5f) / s;
        float a = 1f - u * u * (3f - 2f * u);
        return new Color(0.04f, 0.03f, 0.02f, 0.88f * a);
    });

    static readonly Color Clear = new Color(0f, 0f, 0f, 0f);

    // Signed distance to a rounded square filling the texture (negative inside).
    static float RoundRect(int x, int y, int s, float radius)
    {
        float h = s / 2f, px = Mathf.Abs(x + 0.5f - h), py = Mathf.Abs(y + 0.5f - h);
        float qx = Mathf.Max(px - (h - radius), 0f), qy = Mathf.Max(py - (h - radius), 0f);
        float outside = Mathf.Sqrt(qx * qx + qy * qy);
        float inside = Mathf.Min(Mathf.Max(px - (h - radius), py - (h - radius)), 0f);
        return outside + inside - radius;
    }

    // Small diamond at each corner, centred `inset` pixels from both edges.
    static bool Corner(int x, int y, int s, float inset, float size)
    {
        float cx = Mathf.Min(x + 0.5f, s - x - 0.5f) - inset, cy = Mathf.Min(y + 0.5f, s - y - 0.5f) - inset;
        return Mathf.Abs(cx) + Mathf.Abs(cy) <= size / 2f;
    }

    static Sprite Get(string name, int size, int border, System.Func<int, int, int, Color> pixel)
    {
        string path = Folder + name + ".png";
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Folder);
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, pixel(x, y, size));
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
        }
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        var b = new Vector4(border, border, border, border);
        if (importer.textureType != TextureImporterType.Sprite || importer.spriteBorder != b || importer.mipmapEnabled)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = b;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
