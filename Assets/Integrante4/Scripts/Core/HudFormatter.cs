using System;

namespace Unit404.Integrante4.Core
{
    // Logica pura del HUD: sin dependencias de Unity para poder probarla en EditMode.
    public static class HudFormatter
    {
        public const int MaxLives = 5;
        public const float MaxFuel = 20f;

        // Puntaje del nivel: 100% al inicio, -20% por golpe (GDD 5.2).
        public static string FormatScore(int score) => Math.Max(0, Math.Min(100, score)) + "%";

        // El combustible baja por unidades enteras (1 cada 20 s), asi que se muestra truncado.
        public static string FormatFuel(float fuel) => (int)Math.Floor(Clamp(fuel, 0f, MaxFuel)) + " / " + (int)MaxFuel;

        public static float FuelFraction(float fuel) => Clamp(fuel, 0f, MaxFuel) / MaxFuel;

        public static bool IsCoreActive(int lives, int coreIndex) => coreIndex >= 0 && coreIndex < lives;

        static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;
    }
}
