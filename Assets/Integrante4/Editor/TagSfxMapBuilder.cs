using System.IO;
using UnityEditor;
using UnityEngine;
using Unit404.Integrante4.Core;

// Creates Assets/Integrante4/Audio/TagSfxMap.asset from the placeholder clips (GDD 5.9 table).
public static class TagSfxMapBuilder
{
    const string Folder = "Assets/Integrante4/Audio/";

    static readonly string[] ClipFiles =
    {
        "recarga", "impulso", "dano", "alerta", "alerta_anomalia", "golpe",
        "golpe_fuerte", "chip_obtenido", "glifo", "guardado", "victoria",
    };

    [MenuItem("UNIT-404/Integrante 4/Crear mapa de sonidos por tag")]
    public static void Build()
    {
        AssetDatabase.Refresh();
        string assetPath = Folder + "TagSfxMap.asset";
        AssetDatabase.DeleteAsset(assetPath);
        var map = ScriptableObject.CreateInstance<TagSfxMap>();
        AssetDatabase.CreateAsset(map, assetPath);

        var so = new SerializedObject(map);
        SerializedProperty entries = so.FindProperty("entries");
        entries.arraySize = TagSoundCatalog.Tags.Count;
        for (int i = 0; i < TagSoundCatalog.Tags.Count; i++)
        {
            SerializedProperty e = entries.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("tag").stringValue = TagSoundCatalog.Tags[i];
            e.FindPropertyRelative("clip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(Folder + ClipFiles[i] + ".wav");
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(map);
        AssetDatabase.SaveAssets();
    }
}
