using UnityEngine;

public enum FlightMode { Mode2_5D, Mode3D }

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float thrustForce = 18f;
    public float torque = 6f;

    // Lo apaga FuelSystem cuando no queda combustible.
    [HideInInspector] public bool thrustEnabled = true;

    public FlightMode Mode { get; private set; }

    Rigidbody rb;
    FuelSystem fuel;
    bool thrustInput;
    float inputH, inputV;

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
        bool hasFuel = fuel == null || fuel.fuel > 0f;
        if (thrustInput && thrustEnabled && hasFuel)
            rb.AddRelativeForce(Vector3.up * thrustForce);

        if (Mode == FlightMode.Mode2_5D)
        {
            // A = giro antihorario (vista de lado, Z negativa hacia la camara)
            rb.AddRelativeTorque(0f, 0f, -inputH * torque);
        }
        else
        {
            rb.AddRelativeTorque(0f, inputH * torque, 0f);
            rb.AddRelativeTorque(inputV * torque, 0f, 0f);
        }
    }
}
