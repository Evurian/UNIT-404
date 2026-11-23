using Unit404.Integrante4.Core;
using UnityEngine;

// Test-only third-person camera for levels 3-4 (Integrante 1 owns the real one).
// Sits behind the player, mouse orbits. Disables itself when the Player is not a PlaceholderPlayer.
public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 9f;
    public float height = 1.5f;
    public float yaw;
    public float pitch = 20f;
    public float mouseSensitivity = 3f;
    public float smoothing = 10f;

    public float Yaw => yaw;

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) return;
            target = player.transform;
        }
        if (target.GetComponent<PlaceholderPlayer>() == null)
        {
            enabled = false; // the real player brings its own camera
            return;
        }
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch = ThirdPersonMath.ClampPitch(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity);
        var o = ThirdPersonMath.CameraOffset(yaw, pitch, distance);
        Vector3 focus = target.position + Vector3.up * height;
        Vector3 goal = focus + new Vector3(o.x, o.y, o.z);
        transform.position = Vector3.Lerp(transform.position, goal, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
        transform.rotation = Quaternion.LookRotation(focus - transform.position, Vector3.up);
    }
}
