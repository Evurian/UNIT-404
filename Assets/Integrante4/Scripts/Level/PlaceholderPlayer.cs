using System.Collections;
using Unit404.Integrante4.Core;
using UnityEngine;

// Stand-in for UNIT-404 (Integrante 1) while the robot prefab is not on this branch.
// Removes itself when another object tagged Player appears. A/D move, Space thrusts.
[RequireComponent(typeof(Rigidbody))]
public class PlaceholderPlayer : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float thrust = 22f;
    // Levels 3-4 allow free depth (W/S move along Z); levels 1-2 stay on Z = 0.
    public bool allowDepth;
    // Levels 3-4: WASD relative to the third-person camera yaw.
    public bool cameraRelative;

    Rigidbody body;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        ApplyConstraints();
    }

    void ApplyConstraints()
    {
        body.constraints = allowDepth
            ? RigidbodyConstraints.FreezeRotation
            : RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    IEnumerator Start()
    {
        ApplyConstraints();
        yield return null;
        foreach (GameObject other in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (other != gameObject)
            {
                Destroy(gameObject);
                yield break;
            }
        }
    }

    void FixedUpdate()
    {
        Vector3 v = body.linearVelocity;
        var cam = cameraRelative && Camera.main != null ? Camera.main.GetComponent<ThirdPersonCamera>() : null;
        if (cam != null && cam.enabled)
        {
            var d = ThirdPersonMath.MoveDirection(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), cam.Yaw);
            v.x = d.x * moveSpeed;
            v.z = d.z * moveSpeed;
        }
        else
        {
            v.x = Input.GetAxisRaw("Horizontal") * moveSpeed;
            if (allowDepth) v.z = Input.GetAxisRaw("Vertical") * moveSpeed;
        }
        if (Input.GetKey(KeyCode.Space)) v.y = Mathf.Min(v.y + thrust * Time.fixedDeltaTime, 7f);
        body.linearVelocity = v;
    }
}
