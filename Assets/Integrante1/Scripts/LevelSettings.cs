using UnityEngine;

[DefaultExecutionOrder(-100)]
public class LevelSettings : MonoBehaviour
{
    public static LevelSettings Instance { get; private set; }

    public FlightMode mode = FlightMode.Mode2_5D;
    public float fuelCarryOver = 0f;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static FlightMode CurrentMode
    {
        get
        {
            if (Instance == null)
            {
                Debug.LogWarning("No hay LevelSettings en la escena: se usa el modo 3D por defecto.");
                return FlightMode.Mode3D;
            }
            return Instance.mode;
        }
    }

    public static float CarryOver => Instance != null ? Instance.fuelCarryOver : 0f;
}
