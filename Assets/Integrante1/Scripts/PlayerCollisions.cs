using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    public float maxLandingSpeed = 3f;
    public float maxLandingAngle = 35f;
    public float contaminationInterval = 2f;
    public float ringImpulse = 12f;

    PlayerHealth health;
    FuelSystem fuel;
    SfxPlayer sfx;
    Rigidbody rb;
    float contaminationTimer;
    bool levelCompleted;

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        fuel = GetComponent<FuelSystem>();
        sfx = GetComponent<SfxPlayer>();
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        string tag = collision.collider.tag;

        if (tag == Tags.Obstaculo || tag == Tags.Enemigo || tag == Tags.Jefe)
        {
            health.TakeHit();
            if (sfx != null) sfx.Play(sfx.dano);
        }
        else if (tag == Tags.Plataforma)
        {
            float speed = collision.relativeVelocity.magnitude;
            float angle = Vector3.Angle(transform.up, Vector3.up);
            if (!levelCompleted && speed < maxLandingSpeed && angle < maxLandingAngle)
            {
                levelCompleted = true;
                GameEvents.RaiseLevelCompleted();
                if (sfx != null) sfx.Play(sfx.victoria);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        string tag = other.tag;

        if (tag == Tags.Fragmento)
        {
            fuel.AddFuel(1f);
            Destroy(other.gameObject);
            if (sfx != null) sfx.Play(sfx.recarga);
        }
        else if (tag == Tags.Propulsor)
        {
            rb.AddForce(other.transform.up * ringImpulse, ForceMode.Impulse);
            if (sfx != null) sfx.Play(sfx.impulso);
        }
        else if (tag == Tags.Contaminacion)
        {
            contaminationTimer = 0f;
        }
        else if (tag == Tags.Chip || tag == Tags.Glifo || tag == Tags.Checkpoint)
        {
            Debug.Log("Recogido/tocado: " + tag);
        }
        // Observador: lo maneja el script del Integrante 2.
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(Tags.Contaminacion)) return;

        contaminationTimer += Time.deltaTime;
        if (contaminationTimer >= contaminationInterval)
        {
            contaminationTimer = 0f;
            health.TakeHit();
            if (sfx != null) sfx.Play(sfx.dano);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Tags.Contaminacion)) contaminationTimer = 0f;
    }
}
