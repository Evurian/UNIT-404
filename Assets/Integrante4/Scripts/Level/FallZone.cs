using UnityEngine;

// Trigger under the whole level (ODD S25, T27): the player falling into it opens the "Sistema caído" screen.
[RequireComponent(typeof(BoxCollider))]
public class FallZone : MonoBehaviour
{
    public FallScreen screen;

    void OnTriggerEnter(Collider other)
    {
        Transform body = other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform;
        if (body.CompareTag("Player") && screen != null) screen.Show(body);
    }
}
