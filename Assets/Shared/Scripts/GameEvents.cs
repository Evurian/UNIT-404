using System;

// Los eventos son estaticos y sobreviven al cambio de escena:
// todo suscriptor debe desuscribirse en OnDisable.
public static class GameEvents
{
    public static event Action<int>   OnLivesChanged;
    public static event Action<int>   OnScoreChanged;   // 0 a 100
    public static event Action<float> OnFuelChanged;
    public static event Action        OnPlayerHit;
    public static event Action        OnPlayerDied;
    public static event Action        OnBossDefeated;
    public static event Action        OnLevelCompleted;

    public static void RaiseLivesChanged(int v)   => OnLivesChanged?.Invoke(v);
    public static void RaiseScoreChanged(int v)   => OnScoreChanged?.Invoke(v);
    public static void RaiseFuelChanged(float v)  => OnFuelChanged?.Invoke(v);
    public static void RaisePlayerHit()           => OnPlayerHit?.Invoke();
    public static void RaisePlayerDied()          => OnPlayerDied?.Invoke();
    public static void RaiseBossDefeated()        => OnBossDefeated?.Invoke();
    public static void RaiseLevelCompleted()      => OnLevelCompleted?.Invoke();
}
