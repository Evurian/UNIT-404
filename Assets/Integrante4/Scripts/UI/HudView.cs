using Unit404.Integrante4.Core;
using UnityEngine;
using UnityEngine.UI;

// HUD basico (semana 12): vidas, puntaje y combustible leidos de GameEvents.
// Se suscribe en OnEnable y se desuscribe en OnDisable (eventos estaticos).
public class HudView : MonoBehaviour
{
    [Header("Vidas (5 nucleos)")]
    public Image[] lifeCores = new Image[HudFormatter.MaxLives];
    public Color coreActiveColor = new Color(1f, 0.48f, 0.1f);
    public Color coreEmptyColor = new Color(0.16f, 0.16f, 0.16f);

    [Header("Puntaje")]
    public Text scoreText;

    [Header("Combustible")]
    public RectTransform fuelFill;
    public Text fuelText;

    [Header("Valores iniciales hasta recibir eventos")]
    public int initialLives = HudFormatter.MaxLives;
    public int initialScore = 100;
    public float initialFuel = 10f;

    void OnEnable()
    {
        GameEvents.OnLivesChanged += ShowLives;
        GameEvents.OnScoreChanged += ShowScore;
        GameEvents.OnFuelChanged += ShowFuel;

        ShowLives(initialLives);
        ShowScore(initialScore);
        ShowFuel(initialFuel);
    }

    void OnDisable()
    {
        GameEvents.OnLivesChanged -= ShowLives;
        GameEvents.OnScoreChanged -= ShowScore;
        GameEvents.OnFuelChanged -= ShowFuel;
    }

    public void ShowLives(int lives)
    {
        for (int i = 0; i < lifeCores.Length; i++)
        {
            if (lifeCores[i] == null) continue;
            lifeCores[i].color = HudFormatter.IsCoreActive(lives, i) ? coreActiveColor : coreEmptyColor;
        }
    }

    public void ShowScore(int score)
    {
        if (scoreText != null) scoreText.text = HudFormatter.FormatScore(score);
    }

    public void ShowFuel(float fuel)
    {
        if (fuelText != null) fuelText.text = HudFormatter.FormatFuel(fuel);
        if (fuelFill != null)
        {
            Vector2 max = fuelFill.anchorMax;
            max.x = HudFormatter.FuelFraction(fuel);
            fuelFill.anchorMax = max;
        }
    }
}
