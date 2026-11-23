using System.Collections;
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
        float x = Input.GetAxisRaw("Horizontal") * moveSpeed;
        Vector3 v = body.linearVelocity;
        v.x = x;
        if (allowDepth) v.z = Input.GetAxisRaw("Vertical") * moveSpeed;
        if (Input.GetKey(KeyCode.Space)) v.y = Mathf.Min(v.y + thrust * Time.fixedDeltaTime, 7f);
        body.linearVelocity = v;
    }
}
