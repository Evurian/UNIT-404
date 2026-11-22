using UnityEngine;

// HUD temporal hasta que el Integrante 4 termine el real.
public class DebugHud : MonoBehaviour
{
    int lives;
    int score;
    float fuel;

    void OnEnable()
    {
        GameEvents.OnLivesChanged += SetLives;
        GameEvents.OnScoreChanged += SetScore;
        GameEvents.OnFuelChanged += SetFuel;
    }

    void OnDisable()
    {
        GameEvents.OnLivesChanged -= SetLives;
        GameEvents.OnScoreChanged -= SetScore;
        GameEvents.OnFuelChanged -= SetFuel;
    }

    void SetLives(int v) { lives = v; }
    void SetScore(int v) { score = v; }
    void SetFuel(float v) { fuel = v; }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 25), "Vidas: " + lives);
        GUI.Label(new Rect(10, 30, 300, 25), "Puntaje: " + score);
        GUI.Label(new Rect(10, 50, 300, 25), "Combustible: " + fuel.ToString("0.0"));
    }
}
