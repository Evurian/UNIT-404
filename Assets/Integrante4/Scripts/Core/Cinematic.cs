using System;

namespace Unit404.Integrante4.Core
{
    // Script of the culture transition played before levels 2-4 (ODD S27, T34): names, timing, Ada's lines.
    public static class CinematicScript
    {
        public const float Duration = 16f;
        const float FadeIn = 1f, FadeOut = 0.8f;
        const float TitleIn = 1f, TitleFull = 2f, TitleHold = 5.5f, TitleOut = 6.5f;
        const float LinesFrom = 2f, LinesTo = Duration - 1f;

        static readonly string[][] Lines =
        {
            new[]
            {
                "Ada: El Archivo abre la siguiente memoria: Egipto, a orillas del Nilo.",
                "Ada: Sus templos guardan el saber entre columnas y jeroglíficos.",
                "Ada: Apofis acecha en la cámara del Duat. Prepárate, UNIT-404.",
            },
            new[]
            {
                "Ada: Grecia. Puertos, ágoras y colinas frente al mar Egeo.",
                "Ada: Aquí se desbloquea la profundidad: podrás volar en tres dimensiones.",
                "Ada: Cronos espera en el templo en ruinas de la ladera.",
            },
            new[]
            {
                "Ada: China. La Gran Muralla serpentea entre montañas.",
                "Ada: Recorre el adarve entre torres, carretas y estandartes.",
                "Ada: Chiyou aguarda en el tramo ancho entre las torres.",
            },
        };

        public static bool HasCinematic(int level) => level >= 2 && level <= 4;
        public static string SceneName(int level) => "Transicion" + Check(level) + "_" + CulturePalette.CultureName(level);
        public static string Title(int level) => "Nivel " + Check(level) + " — " + CulturePalette.CultureName(level);
        public static string[] Subtitles(int level) => Lines[Check(level) - 2];

        // Title card: fades in, holds, fades out early so the landscape carries the rest of the shot.
        public static float TitleAlpha(float t)
        {
            if (t <= TitleIn || t >= TitleOut) return 0f;
            if (t < TitleFull) return (t - TitleIn) / (TitleFull - TitleIn);
            if (t <= TitleHold) return 1f;
            return 1f - (t - TitleHold) / (TitleOut - TitleHold);
        }

        // Black overlay: out of black at the start, back to black at the end.
        public static float FadeAlpha(float t)
        {
            if (t <= 0f) return 1f;
            if (t < FadeIn) return 1f - t / FadeIn;
            if (t >= Duration) return 1f;
            if (t > Duration - FadeOut) return (t - (Duration - FadeOut)) / FadeOut;
            return 0f;
        }

        // Index of the subtitle on screen at time t, -1 before the first line. Lines share the shot evenly.
        public static int SubtitleIndex(int level, float t)
        {
            int count = Subtitles(level).Length;
            if (t < LinesFrom) return -1;
            int i = (int)((t - LinesFrom) / ((LinesTo - LinesFrom) / count));
            return Math.Min(i, count - 1);
        }

        static int Check(int level)
        {
            if (!HasCinematic(level)) throw new ArgumentOutOfRangeException(nameof(level), "cinematics exist for levels 2-4");
            return level;
        }
    }

    // Smooth camera path through key values (one axis at a time, no engine types).
    public static class CinematicPath
    {
        // Catmull-Rom through every key; u in [0, 1] spans the whole path and is clamped.
        public static float CatmullRom(float[] keys, float u)
        {
            int n = keys.Length;
            if (n == 1) return keys[0];
            float f = Math.Max(0f, Math.Min(1f, u)) * (n - 1);
            int i = Math.Min((int)f, n - 2);
            float t = f - i;
            float p0 = keys[Math.Max(i - 1, 0)], p1 = keys[i], p2 = keys[i + 1], p3 = keys[Math.Min(i + 2, n - 1)];
            return 0.5f * (2f * p1 + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t + (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t);
        }

        // Smoothstep: the camera eases out of the first key and into the last.
        public static float Ease(float u)
        {
            u = Math.Max(0f, Math.Min(1f, u));
            return u * u * (3f - 2f * u);
        }
    }
}
