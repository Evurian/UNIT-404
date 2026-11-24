namespace Unit404.Integrante4.Core
{
    public enum AmbienceMode { Level, Boss }

    // ODD S23: inside the boss arena the scene switches to the Apophis ambience until the boss is defeated.
    public static class ArenaAmbienceLogic
    {
        public static AmbienceMode Mode(float playerX, float arenaStartX, float arenaEndX, bool bossDefeated)
        {
            if (bossDefeated) return AmbienceMode.Level;
            return playerX >= arenaStartX && playerX <= arenaEndX ? AmbienceMode.Boss : AmbienceMode.Level;
        }
    }
}
