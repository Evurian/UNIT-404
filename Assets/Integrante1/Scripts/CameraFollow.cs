using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset2D = new Vector3(0f, 2f, -15f);
    public Vector3 offset3D = new Vector3(0f, 4f, -10f);
    public float smoothTime = 0.25f;

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        if (LevelSettings.Instance != null && LevelSettings.Instance.mode == FlightMode.Mode2_5D)
        {
            Vector3 desired = new Vector3(target.position.x + offset2D.x, target.position.y + offset2D.y, offset2D.z);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            transform.rotation = Quaternion.identity;
        }
        else
        {
            Vector3 desired = target.position + target.rotation * offset3D;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            transform.LookAt(target);
        }
    }
}
