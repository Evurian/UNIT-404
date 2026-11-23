using System.Collections.Generic;
using System.Text;

namespace Unit404.Integrante4.Core
{
    // Achievements computable from a finished level (GDD 11.6). Ritual/boss/global ones belong to other members.
    public static class Achievements
    {
        public static IEnumerable<string> Evaluate(int level, int scorePercent, int glyphs, int glyphTotal, float fuel)
        {
            if (level == 1) yield return "Primer vuelo";
            if (scorePercent >= 100) yield return "Sin un rasguño";
            if (glyphs >= glyphTotal) yield return "Coleccionista";
            if (fuel >= 10f) yield return "Administrador";
        }
    }

    // Ada speaks at the start, at checkpoints and at the end of each level (GDD 6).
    public static class AdaDialogue
    {
        static readonly string[] Starts =
        {
            "Ada: Mesopotamia. Cruza las orillas, sube el zigurat y recupera el primer Escrito.",
            "Ada: Egipto. La sala de columnas es oscura; sigue la luz de las antorchas.",
            "Ada: Grecia. Con la profundidad desbloqueada, puedes volar sobre los barcos. Cuidado con las rocas.",
            "Ada: China. La muralla es larga; los carros y el polvo no perdonan.",
        };
        static readonly string[] Checkpoints =
        {
            "Ada: Punto de control guardado. El guardián del zigurat se acerca.",
            "Ada: Punto de control guardado. Siento a Apofis en la cámara del Duat.",
            "Ada: Punto de control guardado. Las ruinas esconden a Cronos.",
            "Ada: Punto de control guardado. Chiyou espera en el tramo ancho.",
        };
        static readonly string[] Ends =
        {
            "Ada: Escrito recuperado. La memoria de Mesopotamia vuelve al Archivo.",
            "Ada: Escrito recuperado. Egipto recuerda su lección.",
            "Ada: Escrito recuperado. Grecia ya no está olvidada.",
            "Ada: Escrito recuperado. La muralla guarda de nuevo su historia.",
        };
        const string Generic = "Ada: Sigamos adelante, UNIT-404.";

        public static string Start(int level) => Pick(Starts, level);
        public static string Checkpoint(int level) => Pick(Checkpoints, level);
        public static string End(int level) => Pick(Ends, level);

        static string Pick(string[] lines, int level) => level >= 1 && level <= lines.Length ? lines[level - 1] : Generic;
    }

    public static class LevelSummary
    {
        public static string Format(int scorePercent, int glyphs, int glyphTotal, float fuel, IEnumerable<string> achievements)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Puntaje: " + scorePercent + "%");
            sb.AppendLine("Glifos: " + glyphs + "/" + glyphTotal);
            sb.AppendLine("Combustible: " + (int)System.Math.Floor(fuel));
            foreach (string a in achievements) sb.AppendLine("Logro: " + a);
            return sb.ToString().TrimEnd();
        }
    }

    // Text of the skippable story scenes (GDD 11.2 and 11.8).
    public static class StoryScripts
    {
        public static readonly string[] Intro =
        {
            "UNIT-404 despierta en un archivo vacío.",
            "Error 404: memoria no encontrada.",
            "Ada: Soy Ada. La humanidad no fue destruida: fue olvidada. Ayúdame a recuperar su memoria.",
        };

        public static readonly string[] Epilogue =
        {
            "El Libro de la Humanidad se reconstruye.",
            "Pasan las páginas de las culturas restauradas.",
            "El siguiente capítulo lo escribes tú",
        };

        public static readonly string[] Credits =
        {
            "UNIT-404",
            "Integrante 1 — Jugador, eventos y sistemas base",
            "Integrante 2 — Jefes y combate",
            "Integrante 3 — Niveles 5 a 8",
            "Integrante 4 — Niveles 1 a 4, interfaz y audio",
        };

        public static readonly string[] Farewell =
        {
            "Gracias por recordar",
        };
    }
}
