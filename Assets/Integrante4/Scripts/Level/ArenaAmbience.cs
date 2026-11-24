using Unit404.Integrante4.Core;
using UnityEngine;

// ODD S23: entering the boss arena (zone 4) switches light, fog and embers to the Apophis look;
// GameEvents.OnBossDefeated brings the level ambience back. Built by LevelSceneBuilder.
public class ArenaAmbience : MonoBehaviour
{
    public float arenaStartX = 120f;
    public float arenaEndX = 160f;
    public Light rimLight;
    public Camera sceneCamera;
    public GameObject arenaEmbers;

    public Color levelRim = Color.white, bossRim = new Color(1f, 0.35f, 0.08f);
    public float levelRimIntensity = 1f, bossRimIntensity = 2.2f;
    public Color levelFog = Color.black, bossFog = new Color(0.02f, 0.012f, 0.01f);
    public float levelFogDensity = 0.012f, bossFogDensity = 0.02f;

    bool bossDefeated;
    Transform player;
    AmbienceMode current = (AmbienceMode)(-1);

    void OnEnable() => GameEvents.OnBossDefeated += HandleBossDefeated;
    void OnDisable() => GameEvents.OnBossDefeated -= HandleBossDefeated;

    void HandleBossDefeated() => bossDefeated = true;

    void Update()
    {
        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go == null) return;
            player = go.transform;
        }
        Apply(ArenaAmbienceLogic.Mode(player.position.x, arenaStartX, arenaEndX, bossDefeated));
    }

    public void Apply(AmbienceMode mode)
    {
        if (mode == current) return;
        current = mode;
        bool boss = mode == AmbienceMode.Boss;
        if (rimLight != null)
        {
            rimLight.color = boss ? bossRim : levelRim;
            rimLight.intensity = boss ? bossRimIntensity : levelRimIntensity;
        }
        RenderSettings.fogColor = boss ? bossFog : levelFog;
        RenderSettings.fogDensity = boss ? bossFogDensity : levelFogDensity;
        if (sceneCamera != null) sceneCamera.backgroundColor = RenderSettings.fogColor;
        if (arenaEmbers != null) arenaEmbers.SetActive(boss);
    }
}
