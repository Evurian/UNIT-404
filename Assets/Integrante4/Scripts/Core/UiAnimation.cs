using System;

namespace Unit404.Integrante4.Core
{
    // Pure easing maths for week-14 UI animations (culture transition, chip obtained).
    public static class UiAnimation
    {
        // 0 -> 1 smoothstep over duration, clamped. Zero duration means instantly opaque.
        public static float FadeAlpha(float elapsed, float duration)
        {
            if (duration <= 0f) return 1f;
            float t = Math.Max(0f, Math.Min(1f, elapsed / duration));
            return t * t * (3f - 2f * t);
        }

        // Scale pulse: 1 -> peak (midway) -> 1; outside [0, duration] it rests at 1.
        public static float ChipPulse(float elapsed, float duration, float peak)
        {
            if (duration <= 0f || elapsed <= 0f || elapsed >= duration) return 1f;
            return 1f + (peak - 1f) * (float)Math.Sin(Math.PI * elapsed / duration);
        }
    }
}
