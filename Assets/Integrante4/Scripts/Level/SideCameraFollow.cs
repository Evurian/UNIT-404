using UnityEngine;

// Fixed side camera for 2.5D levels: follows the target on X and Y only (GDD 2.5D rule).
// If no target is set it looks for the object tagged Player.
public class SideCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2.5f, -16f);
    public float smoothing = 6f;

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;
            target = player.transform;
        }
        Vector3 goal = new Vector3(target.position.x + offset.x, target.position.y + offset.y, offset.z);
        transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
    }
}
