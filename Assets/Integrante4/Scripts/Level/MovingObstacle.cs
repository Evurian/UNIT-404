using UnityEngine;

// Ping-pong obstacle for levels 3-4 (rolling rocks, falling columns, carts).
// Moves between its start position and start + travel at the given speed.
public class MovingObstacle : MonoBehaviour
{
    public Vector3 travel = Vector3.right * 4f;
    public float speed = 2f;

    Vector3 origin;

    void Awake() => origin = transform.position;

    void Update()
    {
        float length = travel.magnitude;
        if (length <= 0f) return;
        float t = Mathf.PingPong(Time.time * speed, length) / length;
        transform.position = origin + travel * t;
    }
}
