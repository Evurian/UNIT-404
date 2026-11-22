using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public const int MaxLives = 5;

    public int lives = 5;
    public int score = 100;
    public float invulnerabilitySeconds = 0.5f;

    float invulnerableUntil = -1f;

    void Start()
    {
        GameEvents.RaiseLivesChanged(lives);
        GameEvents.RaiseScoreChanged(score);
    }

    public void TakeHit()
    {
        if (lives <= 0 || Time.time < invulnerableUntil) return;
        invulnerableUntil = Time.time + invulnerabilitySeconds;

        lives--;
        score = Mathf.Max(0, score - 20);
        GameEvents.RaisePlayerHit();
        GameEvents.RaiseLivesChanged(lives);
        GameEvents.RaiseScoreChanged(score);

        if (lives <= 0)
        {
            Debug.Log("UNIT-404 destruido (OnPlayerDied). La reaparicion llega en la semana 13.");
            GameEvents.RaisePlayerDied();
        }
    }

    public void Heal(int amount = 1)
    {
        lives = Mathf.Min(MaxLives, lives + amount);
        GameEvents.RaiseLivesChanged(lives);
    }
}
