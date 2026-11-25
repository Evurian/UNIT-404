using System;

namespace Unit404.Integrante4.Core
{
    // The order of Integrante 4's scenes on integrante-4 (ODD S31, T33). One source for the build list and
    // every "next scene" field, so the story cannot skip or loop by accident.
    public static class SceneFlow
    {
        public const string Start = "Inicio";
        public const string Loading = "Carga";

        // Story order; the last scene returns to Start.
        public static readonly string[] Story =
        {
            Start, "Introduccion",
            "Nivel1_Mesopotamia", "Nivel2_Egipto", "Nivel3_Grecia", "Nivel4_China",
            "Despedida", "Creditos", "Cierre",
        };

        // Build list: Start first (it opens the game), then the loading scene, then the story.
        public static readonly string[] BuildOrder = BuildList();

        public static string Next(string current)
        {
            int i = Array.IndexOf(Story, current);
            return i < 0 ? Start : Story[(i + 1) % Story.Length];
        }

        static string[] BuildList()
        {
            var list = new string[Story.Length + 1];
            list[0] = Start;
            list[1] = Loading;
            Array.Copy(Story, 1, list, 2, Story.Length - 1);
            return list;
        }
    }
}
