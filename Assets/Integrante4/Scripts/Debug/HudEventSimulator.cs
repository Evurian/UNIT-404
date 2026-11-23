using UnityEngine;

// Solo para la escena de prueba del Integrante 4: simula los eventos del jugador
// mientras el prefab de UNIT-404 (Integrante 1) no esta en esta rama.
// H = golpe, F = gastar 1 de combustible, G = recargar 1, R = reiniciar.
public class HudEventSimulator : MonoBehaviour
{
    int lives = 5;
    int score = 100;
    float fuel = 10f;

    void Start() => Publish();

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
        if (Input.GetKeyDown(KeyCode.R)) { lives = 5; score = 100; fuel = 10f; Publish(); }
    }

    void Publish()
    {
        GameEvents.RaiseLivesChanged(lives);
        GameEvents.RaiseScoreChanged(score);
        GameEvents.RaiseFuelChanged(fuel);
    }
}
