using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Skippable text sequence (intro, epilogue, credits, farewell). Space/Enter/Esc skips.
public class StoryScene : MonoBehaviour
{
    public enum Script { Intro, Epilogue, Credits, Farewell }

    public Script script;
    public Text text;
    public float secondsPerLine = 3.5f;
    public string nextScene = SceneLoader.StartScene;

    IEnumerator Start()
    {
        string[] lines = Lines();
        foreach (string line in lines)
        {
            if (text != null) text.text = line;
            float t = 0f;
            while (t < secondsPerLine)
            {
                if (SkipPressed()) { Finish(); yield break; }
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        Finish();
    }

    string[] Lines()
    {
        switch (script)
        {
            case Script.Intro: return Unit404.Integrante4.Core.StoryScripts.Intro;
            case Script.Epilogue: return Unit404.Integrante4.Core.StoryScripts.Epilogue;
            case Script.Credits: return Unit404.Integrante4.Core.StoryScripts.Credits;
            default: return Unit404.Integrante4.Core.StoryScripts.Farewell;
        }
    }

    static bool SkipPressed() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape);

    void Finish() => SceneLoader.Go(nextScene);
}
