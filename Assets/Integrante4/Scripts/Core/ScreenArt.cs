namespace Unit404.Integrante4.Core
{
    // Which culture a scene belongs to and how the loading screen names it (ODD S33, T35).
    public static class ScreenArt
    {
        // 1-4 for "NivelN_*" and "TransicionN_*", 0 for every other scene.
        public static int CultureOf(string scene)
        {
            if (string.IsNullOrEmpty(scene)) return 0;
            string prefix = scene.StartsWith("Nivel") ? "Nivel" : scene.StartsWith("Transicion") ? "Transicion" : null;
            if (prefix == null || scene.Length <= prefix.Length) return 0;
            int n = scene[prefix.Length] - '0';
            return n >= 1 && n <= 4 ? n : 0;
        }

        public static string LoadingTitle(string scene)
        {
            int level = CultureOf(scene);
            if (level > 0) return "Nivel " + level + " — " + CulturePalette.CultureName(level);
            switch (scene)
            {
                case "Introduccion": return "Introducción";
                case "Creditos": return "Créditos";
                default: return (scene ?? "").Replace('_', ' ');
            }
        }
    }
}
