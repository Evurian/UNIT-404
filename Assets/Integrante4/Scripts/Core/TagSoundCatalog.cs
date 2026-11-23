using System.Collections.Generic;

namespace Unit404.Integrante4.Core
{
    // Sound name per gameplay tag (GDD 5.9). Pure logic, no engine references.
    public static class TagSoundCatalog
    {
        static readonly (string tag, string sound)[] Entries =
        {
            ("Fragmento", "Recarga"),
            ("Propulsor", "Impulso"),
            ("Obstaculo", "Daño"),
            ("Contaminacion", "Alerta"),
            ("Observador", "Alerta de anomalía"),
            ("Enemigo", "Golpe"),
            ("Jefe", "Golpe fuerte"),
            ("Chip", "Chip obtenido"),
            ("Glifo", "Glifo"),
            ("Checkpoint", "Guardado"),
            ("Plataforma", "Victoria"),
        };

        public static IReadOnlyList<string> Tags { get; } = BuildTags();

        // Returns null when the tag has no sound (Player, Untagged, null, empty).
        public static string GetSoundName(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return null;
            foreach (var entry in Entries)
                if (entry.tag == tag) return entry.sound;
            return null;
        }

        static IReadOnlyList<string> BuildTags()
        {
            var list = new List<string>();
            foreach (var entry in Entries) list.Add(entry.tag);
            return list;
        }
    }
}
