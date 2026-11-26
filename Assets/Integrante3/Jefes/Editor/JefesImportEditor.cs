using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Jefes 5 a 7 (Camazotz, Tezcatlipoca, Supay): importación de sus FBX y texturas,
/// y menú para crear sus materiales URP a partir de las texturas exportadas de Blender.
/// Estructura: Assets/Integrante3/Jefes/&lt;Jefe&gt;/{Modelos, Texturas, Materiales}.
/// (El Olvido tiene su propio importador en El_Olvido/Editor.)
/// </summary>
public class JefesImportEditor : AssetPostprocessor
{
    const string Raiz = "Assets/Integrante3/Jefes/";

    // material del FBX -> prefijo de sus texturas (T_<Parte>_Color, _MetallicSmoothness, _N/_Normal, _Emission)
    static readonly (string jefe, string material, string prefijo, bool dosCaras, float emision)[] Tabla =
    {
        ("Camazotz", "MAT_Camazotz_Cuerpo", "T_Cuerpo", false, 0f),
        ("Camazotz", "MAT_Camazotz_Alas", "T_Alas", true, 0f),
        ("Camazotz", "MAT_Camazotz_Accesorios", "T_Accesorios", false, 0f),
        ("Tezcatlipoca", "MAT_Tezca_Cuerpo", "T_Cuerpo", false, 0f),
        ("Tezcatlipoca", "MAT_Tezca_Accesorios", "T_Accesorios", false, 0f),
        ("Tezcatlipoca", "M_Casco", "T_Casco", false, 0f),
        ("Tezcatlipoca", "M_Sandalia", "T_Sandalia", false, 0f),
        ("Tezcatlipoca", "MAT_Jaguar2", "T_Jaguar", false, 0f),
        ("Supay", "M_SPY_Cuerpo", "T_SPY_Cuerpo", true, 1.5f),
        ("Supay", "M_SPY_Cabeza", "T_SPY_Cabeza", true, 1.5f),
        ("Supay", "M_SPY_Cola", "T_SPY_Cola", true, 1.5f),
        ("Supay", "M_SPY_Arma", "T_SPY_Arma", false, 1.5f),
    };

    // clips que se repiten (según las notas de cada jefe)
    static readonly string[] Bucles = { "Reposo", "Vuelo_Idle", "Aturdido", "Recarga", "Correr", "Caminar" };

    static string JefeDe(string ruta)
    {
        if (!ruta.StartsWith(Raiz)) return null;
        var partes = ruta.Substring(Raiz.Length).Split('/');
        return partes.Length >= 3 && partes[0] != "El_Olvido" ? partes[0] : null;
    }

    void OnPreprocessTexture()
    {
        if (JefeDe(assetPath) == null || !assetPath.Contains("/Texturas/")) return;
        var ti = (TextureImporter)assetImporter;
        string n = Path.GetFileNameWithoutExtension(assetPath);
        if (n.EndsWith("_N") || n.EndsWith("_Normal")) ti.textureType = TextureImporterType.NormalMap;
        if (n.EndsWith("_MetallicSmoothness")) ti.sRGBTexture = false;
        ti.maxTextureSize = 2048;
    }

    void OnPreprocessModel()
    {
        string jefe = JefeDe(assetPath);
        if (jefe == null || !assetPath.EndsWith(".fbx")) return;
        var mi = (ModelImporter)assetImporter;
        mi.animationType = ModelImporterAnimationType.Generic;
        mi.importCameras = false;
        mi.importLights = false;
        mi.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        mi.materialLocation = ModelImporterMaterialLocation.InPrefab;
        foreach (var t in Tabla)
        {
            if (t.jefe != jefe) continue;
            var mat = AssetDatabase.LoadAssetAtPath<Material>($"{Raiz}{jefe}/Materiales/{t.material}.mat");
            if (mat != null) mi.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), t.material), mat);
        }
    }

    void OnPreprocessAnimation()
    {
        if (JefeDe(assetPath) == null || !assetPath.EndsWith(".fbx")) return;
        var mi = (ModelImporter)assetImporter;
        var clips = mi.defaultClipAnimations;
        foreach (var c in clips)
        {
            int i = c.name.LastIndexOf('|');
            if (i >= 0) c.name = c.name.Substring(i + 1);
            c.loopTime = System.Array.Exists(Bucles, b => c.name.EndsWith(b));
        }
        mi.clipAnimations = clips;
    }

    [MenuItem("UNIT-404/Jefes/Crear materiales de Camazotz, Tezcatlipoca y Supay")]
    public static void CrearMateriales()
    {
        AssetDatabase.Refresh();
        var lit = Shader.Find("Universal Render Pipeline/Lit");
        int hechos = 0;
        foreach (var t in Tabla)
        {
            string dir = $"{Raiz}{t.jefe}";
            var color = Tex(dir, t.prefijo + "_Color");
            if (color == null) continue;                                   // jefe aún no importado
            Directory.CreateDirectory(dir + "/Materiales");
            string ruta = $"{dir}/Materiales/{t.material}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (m == null) { m = new Material(lit) { name = t.material }; AssetDatabase.CreateAsset(m, ruta); }

            m.SetTexture("_BaseMap", color);
            m.SetColor("_BaseColor", Color.white);

            var ms = Tex(dir, t.prefijo + "_MetallicSmoothness");
            m.SetTexture("_MetallicGlossMap", ms);
            m.SetFloat("_Smoothness", ms != null ? 1f : 0.35f);             // con mapa: suavidad = alfa del mapa
            m.SetFloat("_Metallic", 0f);
            Palabra(m, "_METALLICSPECGLOSSMAP", ms != null);

            var nrm = Tex(dir, t.prefijo + "_N") ?? Tex(dir, t.prefijo + "_Normal");
            m.SetTexture("_BumpMap", nrm);
            Palabra(m, "_NORMALMAP", nrm != null);

            var em = Tex(dir, t.prefijo + "_Emission");
            m.SetTexture("_EmissionMap", em);
            m.SetColor("_EmissionColor", em != null ? Color.white * t.emision : Color.black);
            Palabra(m, "_EMISSION", em != null);
            m.globalIlluminationFlags = em != null ? MaterialGlobalIlluminationFlags.RealtimeEmissive
                                                   : MaterialGlobalIlluminationFlags.EmissiveIsBlack;

            m.SetFloat("_Cull", t.dosCaras ? 0f : 2f);                     // 0 = las dos caras
            m.doubleSidedGI = t.dosCaras;
            EditorUtility.SetDirty(m);
            hechos++;
            Debug.Log($"Jefes: {t.jefe}/{t.material} metal={ms != null} normal={nrm != null} emision={m.IsKeywordEnabled("_EMISSION")}");
        }
        AssetDatabase.SaveAssets();
        // reimporta los FBX para que tomen los materiales recién creados
        foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { Raiz.TrimEnd('/') }))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (JefeDe(p) != null) AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
        }
        Debug.Log($"Jefes: {hechos} materiales listos");
    }

    static Texture2D Tex(string dir, string nombre) =>
        AssetDatabase.LoadAssetAtPath<Texture2D>($"{dir}/Texturas/{nombre}.png");

    static void Palabra(Material m, string k, bool on)
    {
        if (on) m.EnableKeyword(k); else m.DisableKeyword(k);
    }
}
