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

    // Inicio background rotation (ODD S44, T43): each slide holds, then cross-fades into the next; wraps around.
    public static class SlideShow
    {
        public static int Index(float t, int count, float hold, float fade)
        {
            if (count <= 0) return 0;
            int k = (int)System.Math.Floor(System.Math.Max(0f, t) / (hold + fade));
            return k % count;
        }

        // 0 while holding, eased 0 -> 1 during the cross-fade into slide Index + 1.
        public static float Blend(float t, float hold, float fade)
        {
            float p = System.Math.Max(0f, t) % (hold + fade);
            if (p <= hold) return 0f;
            float u = (p - hold) / fade;
            return u * u * (3f - 2f * u);
        }
    }
}
