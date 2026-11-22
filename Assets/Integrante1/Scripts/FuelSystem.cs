using System;
using UnityEngine;

public class FuelSystem : MonoBehaviour
{
    public const float MaxFuel = 20f;

    public float fuel;
    public float drainInterval = 20f;   // segundos por cada unidad consumida

    public event Action OnFuelEmpty;

    PlayerHealth health;
    PlayerController controller;
    float timer;
    bool emptyHandled;

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        controller = GetComponent<PlayerController>();
    }

    void Start()
    {
        fuel = Mathf.Min(10f + LevelSettings.CarryOver, MaxFuel);
        GameEvents.RaiseFuelChanged(fuel);
    }

    void Update()
    {
        if (fuel <= 0f) return;

        timer += Time.deltaTime;
        if (timer >= drainInterval)
        {
            timer -= drainInterval;
            SetFuel(fuel - 1f);
        }
    }

    public void AddFuel(float amount)
    {
        SetFuel(fuel + amount);
    }

    public bool TrySpend(float amount)
    {
        if (fuel < amount) return false;
        SetFuel(fuel - amount);
        return true;
    }

    void SetFuel(float value)
    {
        fuel = Mathf.Clamp(value, 0f, MaxFuel);
        GameEvents.RaiseFuelChanged(fuel);

        if (fuel <= 0f)
        {
            if (!emptyHandled)
            {
                emptyHandled = true;
                if (controller != null) controller.thrustEnabled = false;
                OnFuelEmpty?.Invoke();
                if (health != null) health.TakeHit();   // un solo golpe
            }
        }
        else if (emptyHandled)
        {
            emptyHandled = false;
            if (controller != null) controller.thrustEnabled = true;
        }
    }
}
