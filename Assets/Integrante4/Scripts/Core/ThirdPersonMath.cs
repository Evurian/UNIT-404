using System;

namespace Unit404.Integrante4.Core
{
    // Pure maths for the third-person test camera (levels 3-4). No engine references.
    public static class ThirdPersonMath
    {
        public const float MinPitch = -10f;
        public const float MaxPitch = 60f;

        public static float ClampPitch(float pitch) => Math.Max(MinPitch, Math.Min(MaxPitch, pitch));

        // Camera-relative ground direction. yaw in degrees around Y; returns a unit (or zero) vector on X/Z.
        public static (float x, float z) MoveDirection(float inputX, float inputY, float yawDegrees)
        {
            float len = (float)Math.Sqrt(inputX * inputX + inputY * inputY);
            if (len < 1e-5f) return (0f, 0f);
            float ix = inputX / len, iy = inputY / len;
            float r = yawDegrees * (float)Math.PI / 180f;
            float sin = (float)Math.Sin(r), cos = (float)Math.Cos(r);
            return (iy * sin + ix * cos, iy * cos - ix * sin);
        }

        // Offset from the player to the camera: behind the look direction, raised by pitch.
        public static (float x, float y, float z) CameraOffset(float yawDegrees, float pitchDegrees, float distance)
        {
            float yaw = yawDegrees * (float)Math.PI / 180f, pitch = pitchDegrees * (float)Math.PI / 180f;
            float horizontal = distance * (float)Math.Cos(pitch);
            return (-(float)Math.Sin(yaw) * horizontal, distance * (float)Math.Sin(pitch), -(float)Math.Cos(yaw) * horizontal);
        }
    }
}
