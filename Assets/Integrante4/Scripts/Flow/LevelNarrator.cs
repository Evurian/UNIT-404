using TMPro;
using System.Linq;
using Unit404.Integrante4.Core;
using UnityEngine;
using UnityEngine.UI;

// Per-level storytelling: Ada's lines, the end-of-level summary card and achievements.
// Score and fuel come from GameEvents; gameplay code sets glyphsCollected and calls OnCheckpoint.
public class LevelNarrator : MonoBehaviour
{
    public int level = 1;
    public int glyphTotal = 8;
    public int glyphsCollected;
    public GameObject summaryCard;
    public TMP_Text summaryText;
    public AchievementToast toast;
    public string nextScene;
    public CultureTransition transition;

    int score = 100;
    float fuel;
    HudExtrasView hud;

    void OnEnable()
    {
        GameEvents.OnScoreChanged += SetScore;
        GameEvents.OnFuelChanged += SetFuel;
        GameEvents.OnLevelCompleted += Complete;
    }

    void OnDisable()
    {
        GameEvents.OnScoreChanged -= SetScore;
        GameEvents.OnFuelChanged -= SetFuel;
        GameEvents.OnLevelCompleted -= Complete;
    }

    void Start()
    {
        hud = FindFirstObjectByType<HudExtrasView>();
        if (summaryCard != null) summaryCard.SetActive(false);
        Say(AdaDialogue.Start(level));
    }

    public void OnCheckpoint() => Say(AdaDialogue.Checkpoint(level));

    void SetScore(int value) => score = value;
    void SetFuel(float value) => fuel = value;

    void Complete()
    {
        Say(AdaDialogue.End(level));
        string[] earned = Achievements.Evaluate(level, score, glyphsCollected, glyphTotal, fuel).ToArray();
        if (summaryCard != null) summaryCard.SetActive(true);
        if (summaryText != null) summaryText.text = LevelSummary.Format(score, glyphsCollected, glyphTotal, fuel, earned);
        if (toast != null && earned.Length > 0) toast.Show(earned[0]);
    }

    // Wired to the summary card's Continue button.
    public void Continue()
    {
        if (string.IsNullOrEmpty(nextScene)) return;
        if (transition != null) transition.Play(level, () => SceneLoader.Go(nextScene));
        else SceneLoader.Go(nextScene);
    }

    void Say(string line)
    {
        if (hud != null) hud.SetAdaMessage(line);
    }
}
