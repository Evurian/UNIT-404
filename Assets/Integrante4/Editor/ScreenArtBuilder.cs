using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

// Fonts and background art of the flow screens (ODD S33, T35).
// Titles: Metamorphous (SIL OFL 1.1). Body: Kenney Future (CC0) when its TTF is in Art/Fonts, else TMP's default font.
// Backgrounds: renders of my own level scenes at 1920x1080 (needs a GPU: run without -nographics).
public static class ScreenArtBuilder
{
    const string FontFolder = "Assets/Integrante4/Art/Fonts/";
    const string ScreenFolder = "Assets/Integrante4/Art/Screens/";
    const string Characters =
        " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~" +
        "ÁÉÍÓÚÜÑáéíóúüñ¿¡—–·«»“”‘’…";

    public static TMP_FontAsset TitleFont => Font("Metamorphous-Regular.ttf", "Metamorphous SDF");
    public static TMP_FontAsset BodyFont => Font("Kenney Future.ttf", "Kenney Future SDF") ?? TMP_Settings.defaultFontAsset;

    [MenuItem("UNIT-404/Integrante 4/Crear fuentes SDF")]
    public static void CreateFonts()
    {
        _ = TitleFont;
        _ = BodyFont;
    }

    // Static SDF asset built once from the TTF (glyphs baked, so builds need no source font at runtime).
    static TMP_FontAsset Font(string ttf, string assetName)
    {
        string path = FontFolder + assetName + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (existing != null) return existing;
        var source = AssetDatabase.LoadAssetAtPath<Font>(FontFolder + ttf);
        if (source == null) return null;

        var asset = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048, AtlasPopulationMode.Dynamic, false);
        asset.name = assetName;
        if (!asset.TryAddCharacters(Characters, out string missing) && !string.IsNullOrEmpty(missing))
            Debug.LogWarning(assetName + " lacks: " + missing);
        AssetDatabase.CreateAsset(asset, path);
        asset.atlasTextures[0].name = assetName + " Atlas";
        AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
        asset.material.name = assetName + " Material";
        AssetDatabase.AddObjectToAsset(asset.material, asset);
        asset.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        return asset;
    }

    public static Sprite Background(string name)
    {
        string path = ScreenFolder + "Fondo_" + name + ".png";
        if (!File.Exists(path)) return null;
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer.textureType != TextureImporterType.Sprite || importer.mipmapEnabled || importer.maxTextureSize < 2048)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // Inicio: the cinematic crane shot over the pyramids; one shot per culture for the other screens.
    [MenuItem("UNIT-404/Integrante 4/Renderizar fondos de pantallas")]
    public static void RenderBackgrounds()
    {
        Directory.CreateDirectory(ScreenFolder);
        RenderCinematic("Transicion2_Egipto", 1.5f, "Inicio");
        RenderLevel1("Mesopotamia");
        RenderCinematic("Transicion2_Egipto", 8f, "Egipto");
        RenderCinematic("Transicion3_Grecia", 1.5f, "Grecia");
        RenderCinematic("Transicion4_China", 4f, "China");
        AssetDatabase.Refresh();
        foreach (string n in new[] { "Inicio", "Mesopotamia", "Egipto", "Grecia", "China" }) Background(n);
    }

    // Backgrounds first, then the flow screens that show them (run after LevelSceneBuilder.BuildAll, with a GPU).
    public static void RenderAndRebuildScreens()
    {
        RenderBackgrounds();
        FlowSceneBuilder.BuildFlowScenes();
    }

    static void RenderCinematic(string scene, float time, string output)
    {
        EditorSceneManager.OpenScene("Assets/Integrante4/Scenes/" + scene + ".unity");
        Object.FindFirstObjectByType<CinematicDirector>().Preview(time);
        Render(output);
    }

    // Level 1 has no transition before it: same framing as the 2.5D cinematic, over the middle of the route.
    static void RenderLevel1(string output)
    {
        EditorSceneManager.OpenScene("Assets/Integrante4/Scenes/Nivel1_Mesopotamia.unity");
        foreach (string tag in new[] { "Player", "Jefe", "Glifo", "Fragmento", "Propulsor", "Observador", "Checkpoint" })
            foreach (var p in GameObject.FindGameObjectsWithTag(tag)) p.SetActive(false);
        var solids = GameObject.Find("Nivel").GetComponentsInChildren<Collider>().Where(c => !c.isTrigger).ToArray();
        Bounds b = solids[0].bounds;
        foreach (var c in solids) b.Encapsulate(c.bounds);
        float x = b.min.x + b.size.x * 0.3f, mid = (b.min.y + b.max.y) / 2f;
        var cam = Camera.main;
        foreach (var follow in cam.GetComponents<MonoBehaviour>()) follow.enabled = false;
        var pos = new Vector3(x, b.max.y + 5f, -19f);
        cam.transform.SetPositionAndRotation(pos, Quaternion.LookRotation(new Vector3(x + 26f, mid, 8f) - pos));
        Render(output);
    }

    static void Render(string output)
    {
        var cam = Camera.main;
        var rt = new RenderTexture(1920, 1080, 24) { antiAliasing = 8 };
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = null;
        RenderTexture.active = rt;
        var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        File.WriteAllBytes(ScreenFolder + "Fondo_" + output + ".png", tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        rt.Release();
    }
}
