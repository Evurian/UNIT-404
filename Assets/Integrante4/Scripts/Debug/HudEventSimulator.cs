using UnityEngine;

// Solo para la escena de prueba del Integrante 4: simula los eventos del jugador
// mientras el prefab de UNIT-404 (Integrante 1) no esta en esta rama.
// H = golpe, F = gastar 1 de combustible, G = recargar 1, R = reiniciar,
// L = completar el nivel (lo hace el aterrizaje en el portal del robot del Integrante 1).
public class HudEventSimulator : MonoBehaviour
{
    // Full HUD extras: N = glifo, M = +20 anomalia, Q = marca de objetivo, 1/2/3 = chip en recarga.
    HudExtrasView extras;
    int glyphs;
    float anomaly;
    bool lockOn;
    readonly float[] cooldown = new float[3];
    const float CooldownTotal = 5f;

    int lives = 5;
    int score = 100;
    float fuel = 10f;

    void Start()
    {
        extras = FindFirstObjectByType<HudExtrasView>();
        Publish();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H) && lives > 0)
        {
            lives--;
            score = Mathf.Max(0, score - 20);
            GameEvents.RaisePlayerHit();
            Publish();
        }
        if (Input.GetKeyDown(KeyCode.F)) { fuel = Mathf.Max(0f, fuel - 1f); Publish(); }
        if (Input.GetKeyDown(KeyCode.G)) { fuel = Mathf.Min(20f, fuel + 1f); Publish(); }
        if (Input.GetKeyDown(KeyCode.R)) { lives = 5; score = 100; fuel = 10f; glyphs = 0; anomaly = 0f; Publish(); }
        if (Input.GetKeyDown(KeyCode.L)) GameEvents.RaiseLevelCompleted();
        UpdateExtras();
    }

    void Publish()
    {
        GameEvents.RaiseLivesChanged(lives);
        GameEvents.RaiseScoreChanged(score);
        GameEvents.RaiseFuelChanged(fuel);
    }

    void UpdateExtras()
    {
        if (extras == null) return;
        if (Input.GetKeyDown(KeyCode.N)) { glyphs = Mathf.Min(8, glyphs + 1); extras.ShowGlyphs(glyphs, 8); }
        if (Input.GetKeyDown(KeyCode.M)) { anomaly = Mathf.Min(100f, anomaly + 20f); extras.SetAnomaly(anomaly); }
        if (Input.GetKeyDown(KeyCode.Q)) { lockOn = !lockOn; extras.SetLockOn(lockOn); }
        string[] names = { "Salto", "Escudo", "Pulso" };
        for (int i = 0; i < 3; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) cooldown[i] = CooldownTotal;
            cooldown[i] = Mathf.Max(0f, cooldown[i] - Time.deltaTime);
            extras.SetChip(i, names[i], i + 1, cooldown[i], CooldownTotal);
        }
    }
}
