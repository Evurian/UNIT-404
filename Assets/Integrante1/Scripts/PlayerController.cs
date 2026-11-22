using UnityEngine;

public enum FlightMode { Mode2_5D, Mode3D }

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float thrustForce = 18f;
    public float torque = 6f;

    // 2.5D: al soltar A/D el robot vuelve solo a la vertical (evita saltos de lado).
    public bool autoLevel2D = true;
    public float autoLevelTorque = 8f;

    // 2.5D: A/D inclinan hasta un angulo maximo (no giran sin parar) y se estabiliza rapido.
    public bool tiltControl2D = true;
    public float maxTilt2D = 35f;
    public float tiltStiffness = 80f;

    // Lo apaga FuelSystem cuando no queda combustible.
    [HideInInspector] public bool thrustEnabled = true;

    public FlightMode Mode { get; private set; }

    Rigidbody rb;
    FuelSystem fuel;
    bool thrustInput;
    float inputH, inputV;
    float lastGroundedTime = -10f;

    // A/D (y W/S) no funcionan en el suelo: es un sistema de propulsion.
    public bool IsGrounded => Time.fixedTime - lastGroundedTime <= Time.fixedDeltaTime * 2f;

    void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                lastGroundedTime = Time.fixedTime;
                return;
            }
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        fuel = GetComponent<FuelSystem>();
        rb.linearDamping = 0.8f;      // Unity < 6: rb.drag
        rb.angularDamping = 3f;       // Unity < 6: rb.angularDrag
    }

    void Start()
    {
        SetMode(LevelSettings.CurrentMode);
    }

    public void SetMode(FlightMode mode)
    {
        Mode = mode;
        rb = rb != null ? rb : GetComponent<Rigidbody>();
        rb.constraints = mode == FlightMode.Mode2_5D
            ? RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY
            : RigidbodyConstraints.None;
    }

    void Update()
    {
        thrustInput = Input.GetKey(KeyCode.Space);
        inputH = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        inputV = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
    }

    void FixedUpdate()
    {
        if (IsGrounded)
        {
            inputH = 0f;
            inputV = 0f;
        }

        bool hasFuel = fuel == null || fuel.fuel > 0f;
        if (thrustInput && thrustEnabled && hasFuel)
            rb.AddRelativeForce(Vector3.up * thrustForce);

        if (Mode == FlightMode.Mode2_5D)
        {
            if (tiltControl2D)
            {
                // Control PD critico: A/D fijan un angulo objetivo; sin teclas vuelve a 0.
                float angle = Vector3.SignedAngle(Vector3.up, transform.up, Vector3.forward) * Mathf.Deg2Rad;
                float target = -inputH * maxTilt2D * Mathf.Deg2Rad;
                float kd = 2f * Mathf.Sqrt(tiltStiffness);
                float acc = tiltStiffness * (target - angle) - kd * rb.angularVelocity.z;
                rb.AddTorque(0f, 0f, acc, ForceMode.Acceleration);
            }
            else
            // A = giro antihorario (vista de lado, Z negativa hacia la camara)
            rb.AddRelativeTorque(0f, 0f, -inputH * torque);

            if (!tiltControl2D && autoLevel2D && Mathf.Approximately(inputH, 0f))
            {
                // Angulo con signo del eje up respecto a la vertical, en el plano XY.
                float angle = Vector3.SignedAngle(Vector3.up, transform.up, Vector3.forward);
                rb.AddTorque(0f, 0f, -Mathf.Clamp(angle / 45f, -1f, 1f) * autoLevelTorque, ForceMode.Acceleration);
            }
        }
        else
        {
            rb.AddRelativeTorque(0f, inputH * torque, 0f);
            rb.AddRelativeTorque(inputV * torque, 0f, 0f);
        }
    }
}
