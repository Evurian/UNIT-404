using UnityEditor;
using UnityEngine;

/// <summary>
/// El Olvido: ajustes de importación (textura de glifos y modelos FBX de El_Olvido/Modelos)
/// y menú para crear los materiales de tinta y del núcleo.
/// </summary>
public class OlvidoTintaEditor : AssetPostprocessor
{
    const string Carpeta = "Assets/Integrante3/Jefes/El_Olvido";
    const string RutaTextura = Carpeta + "/Texturas/T_Olvido_Glifos.png";
    const string RutaMaterial = Carpeta + "/Materiales/M_Olvido_Tinta.mat";
    const string RutaNucleo = Carpeta + "/Materiales/M_Olvido_Nucleo.mat";
    const string RutaPagina = Carpeta + "/Materiales/M_Olvido_Pagina.mat";
    const string RutaTexPagina = Carpeta + "/Texturas/T_Olvido_Pagina.png";

    void OnPreprocessTexture()
    {
        if (!assetPath.EndsWith("T_Olvido_Glifos.png")) return;
        var ti = (TextureImporter)assetImporter;
        ti.sRGBTexture = false;                                   // R/G/B son máscaras
        ti.wrapMode = TextureWrapMode.Repeat;
        ti.mipmapEnabled = true;
        ti.filterMode = FilterMode.Trilinear;
        ti.anisoLevel = 4;
        ti.textureCompression = TextureImporterCompression.CompressedHQ;  // BC7: no mezcla canales
    }

    static bool EsModeloOlvido(string ruta) => ruta.StartsWith(Carpeta + "/Modelos/") && ruta.EndsWith(".fbx");

    // FBX de El Olvido: rig genérico y materiales del proyecto en vez de los del FBX.
    void OnPreprocessModel()
    {
        if (!EsModeloOlvido(assetPath)) return;
        var mi = (ModelImporter)assetImporter;
        mi.animationType = ModelImporterAnimationType.Generic;
        mi.importCameras = false;
        mi.importLights = false;
        mi.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        mi.materialLocation = ModelImporterMaterialLocation.InPrefab;
        Remapear(mi, "M_Olvido_Tinta", RutaMaterial);
        Remapear(mi, "M_Olvido_Nucleo", RutaNucleo);
        Remapear(mi, "M_Olvido_Pagina", RutaPagina);
    }

    static void Remapear(ModelImporter mi, string nombre, string ruta)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (mat != null) mi.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), nombre), mat);
    }

    // Clips: quita el prefijo "Rig|" del nombre y pone en bucle los de reposo.
    void OnPreprocessAnimation()
    {
        if (!EsModeloOlvido(assetPath)) return;
        var mi = (ModelImporter)assetImporter;
        var clips = mi.defaultClipAnimations;
        foreach (var c in clips)
        {
            int i = c.name.LastIndexOf('|');
            if (i >= 0) c.name = c.name.Substring(i + 1);
            c.loopTime = c.name.Contains("Reposo");
        }
        mi.clipAnimations = clips;
    }

    [MenuItem("UNIT-404/El Olvido/Crear materiales")]
    public static void CrearMaterial()
    {
        AssetDatabase.Refresh();
        var shader = Shader.Find("UNIT404/Olvido/Tinta");
        if (shader == null) { Debug.LogError("El Olvido: no se encontró el shader UNIT404/Olvido/Tinta"); return; }

        foreach (var m in ShaderUtil.GetShaderMessages(shader))
            Debug.Log($"El Olvido shader [{m.severity}] {m.message} (línea {m.line})");
        Debug.Log("El Olvido shader con errores: " + ShaderUtil.ShaderHasError(shader));

        System.IO.Directory.CreateDirectory(Carpeta + "/Materiales");
        var mat = AssetDatabase.LoadAssetAtPath<Material>(RutaMaterial);
        if (mat == null)
        {
            mat = new Material(shader) { name = "M_Olvido_Tinta" };
            AssetDatabase.CreateAsset(mat, RutaMaterial);
        }
        mat.SetTexture("_GlyphTex", AssetDatabase.LoadAssetAtPath<Texture2D>(RutaTextura));
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);

        // Núcleo de luz: Unlit con color HDR (luz pura; el bloom de la cámara lo hace brillar)
        var nuc = AssetDatabase.LoadAssetAtPath<Material>(RutaNucleo);
        if (nuc == null)
        {
            nuc = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = "M_Olvido_Nucleo" };
            AssetDatabase.CreateAsset(nuc, RutaNucleo);
        }
        nuc.shader = Shader.Find("Universal Render Pipeline/Unlit");
        nuc.SetColor("_BaseColor", new Color(1f, 0.8f, 0.5f) * 4f);
        EditorUtility.SetDirty(nuc);
        // Páginas arrancadas: Lit de URP con pergamino, visibles por las dos caras
        var pag = AssetDatabase.LoadAssetAtPath<Material>(RutaPagina);
        if (pag == null)
        {
            pag = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "M_Olvido_Pagina" };
            AssetDatabase.CreateAsset(pag, RutaPagina);
        }
        pag.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(RutaTexPagina));
        pag.SetColor("_BaseColor", Color.white);
        pag.SetFloat("_Smoothness", 0.1f);
        pag.SetFloat("_Cull", 0f);                                // Render Face: Both
        pag.doubleSidedGI = true;
        EditorUtility.SetDirty(pag);

        AssetDatabase.SaveAssets();
        Debug.Log("El Olvido: materiales listos en " + Carpeta + "/Materiales");
    }
}
