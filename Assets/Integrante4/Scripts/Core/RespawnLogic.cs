namespace Unit404.Integrante4.Core
{
    public enum RespawnPoint { PlayerStart, LastCheckpoint, ArenaDoor }

    // ODD S25 / T27: falling below the level is a death of GDD 5.6.
    // Route: back to the last checkpoint passed (PlayerStart if none). Boss arena while the boss lives: the arena door.
    public static class RespawnLogic
    {
        public const int Lives = 5;
        public const float MinFuel = 3f;

        // Index of the furthest checkpoint whose x the player has reached, -1 if none.
        public static int LastCheckpoint(float[] checkpointXs, float furthestX)
        {
            int best = -1;
            for (int i = 0; i < checkpointXs.Length; i++)
                if (checkpointXs[i] <= furthestX && (best < 0 || checkpointXs[i] > checkpointXs[best])) best = i;
            return best;
        }

        public static RespawnPoint Choose(float fallX, float arenaStartX, float arenaEndX, bool bossDefeated, int lastCheckpoint)
        {
            if (ArenaAmbienceLogic.Mode(fallX, arenaStartX, arenaEndX, bossDefeated) == AmbienceMode.Boss) return RespawnPoint.ArenaDoor;
            return lastCheckpoint >= 0 ? RespawnPoint.LastCheckpoint : RespawnPoint.PlayerStart;
        }

        public static float RespawnFuel(float fuel) => fuel < MinFuel ? MinFuel : fuel;
    }
}
