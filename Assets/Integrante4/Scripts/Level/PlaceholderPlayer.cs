using System.Collections;
using UnityEngine;

// Stand-in for UNIT-404 (Integrante 1) while the robot prefab is not on this branch.
// Removes itself when another object tagged Player appears. A/D move, Space thrusts.
[RequireComponent(typeof(Rigidbody))]
public class PlaceholderPlayer : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float thrust = 22f;

    Rigidbody body;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    IEnumerator Start()
    {
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
        if (Input.GetKey(KeyCode.Space)) v.y = Mathf.Min(v.y + thrust * Time.fixedDeltaTime, 7f);
        body.linearVelocity = v;
    }
}
