using System.Linq;
using UnityEngine;

// Integrante 1's CameraFollow (added by their InsertarJugador) owns the Main Camera once it exists.
// Matched by type name because this assembly cannot reference theirs; my placeholder cameras step aside.
public static class CameraYield
{
    public static bool RealCameraPresent(GameObject go)
        => go.GetComponents<MonoBehaviour>().Any(c => c != null && c.GetType().Name == "CameraFollow");
}
