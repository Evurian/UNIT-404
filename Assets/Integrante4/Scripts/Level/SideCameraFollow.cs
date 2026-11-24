using UnityEngine;

// Fixed side camera for 2.5D levels: follows the target on X and Y only (GDD 2.5D rule).
// If no target is set it looks for the object tagged Player.
public class SideCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2.5f, -16f);
    public float smoothing = 6f;
    // Levels 3-4: also follow on Z (3D flight). Levels 1-2 keep the camera fixed in depth.
    public bool followDepth;

    void LateUpdate()
    {
        if (CameraYield.RealCameraPresent(gameObject)) { enabled = false; return; }
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;
            target = player.transform;
        }
        Vector3 goal = new Vector3(target.position.x + offset.x, target.position.y + offset.y, followDepth ? target.position.z + offset.z : offset.z);
        transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
    }
}
