namespace Unit404.Integrante4.Core
{
    // AsyncOperation.progress stops at 0.9 while allowSceneActivation is false.
    public static class LoadProgress
    {
        public const float ReadyThreshold = 0.9f;

        public static float Normalize(float raw)
        {
            float p = raw / ReadyThreshold;
            return p < 0f ? 0f : p > 1f ? 1f : p;
        }

        public static string Format(float raw) => (int)System.Math.Round(Normalize(raw) * 100f) + "%";

        public static bool CanActivate(float raw, float elapsed, float minSeconds)
            => raw >= ReadyThreshold && elapsed >= minSeconds;
    }

    public class PauseState
    {
        public bool Paused { get; private set; }
        public float TimeScale => Paused ? 0f : 1f;
        public void Toggle() => Paused = !Paused;
        public void SetPaused(bool paused) => Paused = paused;

        // Esc handling: ignored while another modal screen (the fall screen) is open. Returns true when it toggled.
        public bool HandleEscape(bool blocked)
        {
            if (blocked) return false;
            Toggle();
            return true;
        }
    }
}
