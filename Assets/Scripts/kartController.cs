using UnityEngine;
using System.Collections;

public class KartController : MonoBehaviour
{
    [SerializeField] private Rigidbody sphereRB, carRB;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxSpeed, acceleration, turn, gravity = 7f, downforce = 5f, driftMultiplier = 1.5f;
    [SerializeField] private float boostPower = 0f;
    [SerializeField] private float driftOutwardForce = 550f; // Gaya geser ke samping saat drift
    [SerializeField] private Transform[] rearWheels;
    [SerializeField] private ParticleSystem slowEffectParticle;
    [SerializeField] private ParticleSystem driftEffect;
    [SerializeField] private Transform driftPosition;
    [SerializeField] private AnimationCurve turnCurve;

    private float speedMultiplier = 1f;
    private RaycastHit hit;
    private Vector2 inputValue;
    private Vector3 origin;
    private Vector3 carVelocity;
    private float moveInput, turnInput, brakeInput, driftInput, useItemInput;
    private float TurnMultiplier, sign, radius;
    private bool isStunned = false;
    private bool isDrifting = false;
    private bool isSlowed = false;
    private float originalMaxSpeed;
    private IA_Default inputActions;
    private PlayerInventory playerInventory;

    private void Start()
    {
        radius = sphereRB.GetComponent<SphereCollider>().radius;
        inputActions = new IA_Default();
        inputActions.Player.Movement.Enable();
        inputActions.Player.Brake.Enable();
        inputActions.Player.Drift.Enable();
        inputActions.Player.UseItem.Enable();

        playerInventory = GetComponent<PlayerInventory>();
        originalMaxSpeed = maxSpeed;

        if (driftEffect != null)
        {
            driftEffect.Stop();
        }
    }

    private void Update()
    {
        InputHandling();
    }

    private void FixedUpdate()
    {
        if (isStunned)
        {
            sphereRB.linearVelocity = Vector3.zero;
            sphereRB.angularVelocity = Vector3.zero;
            carRB.linearVelocity = Vector3.zero;
            carRB.angularVelocity = Vector3.zero;
            return;
        }

        carVelocity = carRB.transform.InverseTransformDirection(carRB.linearVelocity);

        if (Grounded())
        {
            Acceleration();
            Steering();
            Drifting();
            sphereRB.AddForce(-transform.up * downforce * sphereRB.mass);
        }
        else
        {
            carRB.MoveRotation(Quaternion.Lerp(carRB.rotation,
            Quaternion.FromToRotation(carRB.transform.up, Vector3.up) *
            carRB.transform.rotation, 0.02f));

            sphereRB.linearVelocity = Vector3.Lerp(sphereRB.linearVelocity,
            sphereRB.linearVelocity + Vector3.down * gravity, Time.deltaTime * gravity);
        }

        RotateWheels();
    }

    private void InputHandling()
    {
        inputValue = inputActions.Player.Movement.ReadValue<Vector2>();
        brakeInput = inputActions.Player.Brake.ReadValue<float>();
        driftInput = inputActions.Player.Drift.ReadValue<float>();
        useItemInput = inputActions.Player.UseItem.ReadValue<float>();
        moveInput = inputValue.y;
        turnInput = inputValue.x;

        sign = Mathf.Sign(carVelocity.z);
        TurnMultiplier = turnCurve.Evaluate(carVelocity.magnitude / maxSpeed);

        if (useItemInput > 0.1f)
        {
            UseItem();
        }
    }

    private void Acceleration()
    {
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            sphereRB.linearVelocity = Vector3.Lerp(sphereRB.linearVelocity,
            carRB.transform.forward * moveInput * maxSpeed,
            acceleration / 10 * Time.deltaTime);
        }
    }

    private void Steering()
    {
         if (Mathf.Abs(moveInput) > 0.1f || Mathf.Abs(carVelocity.z) > 1)
            {
        float turnStrength = isDrifting ? turn * driftMultiplier : turn; // Gunakan driftMultiplier saat drifting
        carRB.AddTorque(Vector3.up * turnInput * sign * turnStrength * 100 * TurnMultiplier);
         }
    }

    private void Drifting()
    {
        if (driftInput > 0.1f && Mathf.Abs(turnInput) > 0.1f && carVelocity.magnitude > 2f)
        {
            isDrifting = true;
            if (driftEffect != null && !driftEffect.isPlaying)
            {
                driftEffect.Play();
            }

            // Terapkan gaya geser ke samping untuk efek drift yang lebih realistis
            Vector3 driftForce = transform.right * (turnInput > 0 ? 1 : -1) * driftOutwardForce * Time.deltaTime;
            sphereRB.AddForce(driftForce, ForceMode.Acceleration);
        }
        else
        {
            isDrifting = false;
            if (driftEffect != null && driftEffect.isPlaying)
            {
                driftEffect.Stop();
            }
        }
    }

    private bool Grounded()
    {
        origin = sphereRB.position + sphereRB.GetComponent<SphereCollider>().radius * Vector3.up;
        var direction = -transform.up;
        var maxdistance = sphereRB.GetComponent<SphereCollider>().radius + 0.2f;

        return Physics.SphereCast(origin, radius + 0.1f, direction, out hit, maxdistance, groundLayer);
    }


    void UseItem()
    {
        if (playerInventory != null)
        {
            playerInventory.UseItem();
        }
    }

    public void Stun(float duration, string source)
    {
        if (!isStunned)
        {
            isStunned = true;
            Debug.Log("Kart terkena stun dari: " + source + " selama " + duration + " detik!");
            StartCoroutine(StunCoroutine(duration));
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false;
        Debug.Log("Stun selesai.");
    }


    private void RotateWheels()
    {
        float rotationSpeed = (sphereRB.linearVelocity.magnitude / maxSpeed) * 360f;
        float direction = Vector3.Dot(sphereRB.linearVelocity.normalized, transform.forward) >= 0 ? 1 : -1;

        foreach (Transform wheel in rearWheels)
        {
            wheel.Rotate(Vector3.up * -direction * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    public void TriggerBoostZone(float boostForce, Vector3 boostDirection)
    {
        Boost(boostForce, boostDirection);
    }

    private void Boost(float boostForce, Vector3 boostDirection)
    {
        if (Grounded())
        {
            sphereRB.AddForce(boostDirection * boostForce, ForceMode.Impulse);
        }
    }

    public void AddImpulseBoost()
    {
        if (Grounded()) // Pastikan kart menyentuh tanah
        {
            Vector3 boostDirection = transform.forward; // Arah dorongan ke depan
            sphereRB.AddForce(boostDirection * boostPower, ForceMode.Impulse);
        }
    }

    public void ApplySlowEffect(float slowAmount, float duration)
    {
        if (!isSlowed)
        {
            isSlowed = true;
            maxSpeed *= slowAmount;  // Kurangi kecepatan maksimum
            if (slowEffectParticle != null)
            {
                slowEffectParticle.Play(); // Efek visual slow
            }

            StartCoroutine(RemoveSlowEffect(duration));
        }
    }

    private IEnumerator RemoveSlowEffect(float duration)
    {
        yield return new WaitForSeconds(duration);
        maxSpeed = originalMaxSpeed;  // Kembalikan kecepatan asli
        isSlowed = false;
        if (slowEffectParticle != null)
        {
            slowEffectParticle.Stop(); // Matikan efek visual slow
        }
    }

}
