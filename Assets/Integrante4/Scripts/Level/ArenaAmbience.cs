using Unit404.Integrante4.Core;
using UnityEngine;
using UnityEngine.Rendering;

// ODD S23 (b, d) / T20, T23: the route is a lit culture day; entering the boss arena (zone 4) switches sun, ambient,
// fog, sky, post-processing and embers to the dark Apophis look in the culture accent.
// GameEvents.OnBossDefeated brings the culture day back. Built by LevelSceneBuilder.
public class ArenaAmbience : MonoBehaviour
{
    public float arenaStartX = 120f;
    public float arenaEndX = 160f;
    public Light keyLight;
    public Light rimLight;
    public Camera sceneCamera;
    public GameObject arenaEmbers;
    public Volume dayVolume;
    public Volume arenaVolume;

    [Header("Route: culture day")]
    public Color levelKey = Color.white;
    public float levelKeyIntensity = 1.3f;
    public Color levelRim = Color.white;
    public float levelRimIntensity = 0.25f;
    public Color levelAmbient = Color.gray;
    public Color levelFog = Color.white;
    public float levelFogDensity = 0.006f;

    [Header("Boss arena: dark, culture accent")]
    public Color bossKey = new Color(0.55f, 0.62f, 0.8f);
    public float bossKeyIntensity = 0.35f;
    public Color bossRim = new Color(1f, 0.35f, 0.08f);
    public float bossRimIntensity = 2.2f;
    public Color bossAmbient = new Color(0.03f, 0.03f, 0.04f);
    public Color bossFog = new Color(0.02f, 0.012f, 0.01f);
    public float bossFogDensity = 0.02f;

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
        if (keyLight != null)
        {
            keyLight.color = boss ? bossKey : levelKey;
            keyLight.intensity = boss ? bossKeyIntensity : levelKeyIntensity;
        }
        if (rimLight != null)
        {
            rimLight.color = boss ? bossRim : levelRim;
            rimLight.intensity = boss ? bossRimIntensity : levelRimIntensity;
        }
        RenderSettings.ambientLight = boss ? bossAmbient : levelAmbient;
        RenderSettings.fogColor = boss ? bossFog : levelFog;
        RenderSettings.fogDensity = boss ? bossFogDensity : levelFogDensity;
        if (sceneCamera != null)
        {
            sceneCamera.clearFlags = boss ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
            sceneCamera.backgroundColor = RenderSettings.fogColor;
        }
        if (dayVolume != null) dayVolume.weight = boss ? 0f : 1f;
        if (arenaVolume != null) arenaVolume.weight = boss ? 1f : 0f;
        if (arenaEmbers != null) arenaEmbers.SetActive(boss);
    }
}
