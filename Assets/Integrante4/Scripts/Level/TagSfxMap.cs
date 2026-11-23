using System;
using System.Collections.Generic;
using UnityEngine;

// Clip per gameplay tag. Placeholder tones until the final audio (week 14).
[CreateAssetMenu(menuName = "UNIT-404/Integrante 4/Tag SFX Map", fileName = "TagSfxMap")]
public class TagSfxMap : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public string tag;
        public AudioClip clip;
    }

    [SerializeField] List<Entry> entries = new List<Entry>();

    public AudioClip GetClip(string tag)
    {
        foreach (Entry e in entries)
            if (e.tag == tag) return e.clip;
        return null;
    }
}
