using Unit404.Integrante4.Core;
using UnityEngine;

// "Sistema caído" (ODD S25, T27): a fall is a death of GDD 5.6. Freezes time and offers
// "Reintentar desde el punto de control"; the player comes back at the last checkpoint passed, or at the arena door
// while the boss lives.
// Integration note for PR 1: Integrante 1 owns lives, fuel and the real respawn (5 lives, at least
// RespawnLogic.MinFuel fuel, score keeps its penalties) and Integrante 2 resets the boss to phase 1.
// Until those hooks exist this screen only moves whatever object is tagged Player.
public class FallScreen : MonoBehaviour
{
    public GameObject panel;
    public Transform playerStart;
    public Transform[] checkpoints;
    public Transform arenaDoor;
    public float arenaStartX = 120f, arenaEndX = 160f;

    Transform player;
    float furthestX = float.MinValue;
    bool bossDefeated;
    RespawnPoint pending;

    void OnEnable() => GameEvents.OnBossDefeated += HandleBossDefeated;
    void OnDisable()
    {
        GameEvents.OnBossDefeated -= HandleBossDefeated;
        Time.timeScale = 1f;
    }

    void HandleBossDefeated() => bossDefeated = true;

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go == null) return;
            player = go.transform;
        }
        if (panel != null && !panel.activeSelf) furthestX = Mathf.Max(furthestX, player.position.x);
    }

    public void Show(Transform fallen)
    {
        if (panel == null || panel.activeSelf) return;
        player = fallen;
        pending = RespawnLogic.Choose(fallen.position.x, arenaStartX, arenaEndX, bossDefeated, LastCheckpoint());
        panel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Retry()
    {
        if (player != null)
        {
            player.position = Target(pending);
            var body = player.GetComponent<Rigidbody>();
            if (body != null) { body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero; }
        }
        panel.SetActive(false);
        Time.timeScale = 1f;
    }

    int LastCheckpoint()
    {
        var xs = new float[checkpoints.Length];
        for (int i = 0; i < xs.Length; i++) xs[i] = checkpoints[i].position.x;
        return RespawnLogic.LastCheckpoint(xs, furthestX);
    }

    Vector3 Target(RespawnPoint point)
    {
        switch (point)
        {
            case RespawnPoint.ArenaDoor: return arenaDoor.position;
            case RespawnPoint.LastCheckpoint:
                // stand on the floor at the checkpoint, not inside its 4 m tall trigger
                var cp = checkpoints[LastCheckpoint()];
                var col = cp.GetComponent<Collider>();
                return new Vector3(cp.position.x, (col != null ? col.bounds.min.y : cp.position.y) + 1f, 0f);
            default: return playerStart.position;
        }
    }
}
