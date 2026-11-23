using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Unit404.Integrante4.Core;

namespace Unit404.Integrante4.Tests
{
    public class TagSoundTests
    {
        [TestCase("Fragmento", "Recarga")]
        [TestCase("Propulsor", "Impulso")]
        [TestCase("Obstaculo", "Daño")]
        [TestCase("Contaminacion", "Alerta")]
        [TestCase("Observador", "Alerta de anomalía")]
        [TestCase("Enemigo", "Golpe")]
        [TestCase("Jefe", "Golpe fuerte")]
        [TestCase("Chip", "Chip obtenido")]
        [TestCase("Glifo", "Glifo")]
        [TestCase("Checkpoint", "Guardado")]
        [TestCase("Plataforma", "Victoria")]
        public void Catalog_MapsEachGddTagToItsSoundName(string tag, string expected)
        {
            Assert.AreEqual(expected, TagSoundCatalog.GetSoundName(tag));
        }

        [TestCase("Player")]
        [TestCase("Untagged")]
        [TestCase("")]
        [TestCase(null)]
        public void Catalog_ReturnsNullForTagsWithoutSound(string tag)
        {
            Assert.IsNull(TagSoundCatalog.GetSoundName(tag));
        }

        [Test]
        public void Catalog_ListsExactlyElevenTags()
        {
            Assert.AreEqual(11, TagSoundCatalog.Tags.Count);
        }

        [Test]
        public void SfxMapAsset_HasAClipForEveryCatalogTag()
        {
            var map = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Integrante4/Audio/TagSfxMap.asset");
            Assert.IsNotNull(map, "TagSfxMap.asset missing");
            var entries = new SerializedObject(map).FindProperty("entries");
            Assert.AreEqual(TagSoundCatalog.Tags.Count, entries.arraySize);
            foreach (string tag in TagSoundCatalog.Tags)
            {
                bool found = false;
                for (int i = 0; i < entries.arraySize; i++)
                {
                    var e = entries.GetArrayElementAtIndex(i);
                    if (e.FindPropertyRelative("tag").stringValue == tag)
                    {
                        found = true;
                        Assert.IsNotNull(e.FindPropertyRelative("clip").objectReferenceValue, "no clip for " + tag);
                    }
                }
                Assert.IsTrue(found, "no entry for " + tag);
            }
        }
    }
}
