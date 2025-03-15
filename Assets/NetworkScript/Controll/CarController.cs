using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour
{
    [SerializeField] private Rigidbody sphereRB, carRB;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxSpeed, accelaration, turn, gravity = 7f, downforce = 5f, driftMultiplier = 1.5f, BodyTilt;
    [SerializeField] private PhysicsMaterial frictionMaterial;
    [SerializeField] private AnimationCurve turnCurve, frictionCurve;
    [SerializeField] private Transform BodyMesh;
    [SerializeField] private float boostPower = 0f;
    //[SerializeField] private Transform[] rearWheels;
    [SerializeField] private ParticleSystem slowEffectParticle;
    private RaycastHit hit;
    private Vector2 inputValue;
    private Vector3 origin;
    private Vector3 carVelocity;
    private float moveInput, turnInput, brakeInput, driftInput, itemInput;
    private float TurnMultiplyer, sign, radius;
    private bool isStunned = false;
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
    }
    private void Update()
    {
        if (itemInput > 0.1)
        {
            UseItem();
        }
        InputHandling();
        Visuals();
    }

    private void FixedUpdate()
    {
        carVelocity = carRB.transform.InverseTransformDirection(carRB.linearVelocity);

        if (Mathf.Abs(carVelocity.x) > 0)
        {
            frictionMaterial.dynamicFriction = frictionCurve.Evaluate(Mathf.Abs(carVelocity.x / 100));
        }

        if (Grounded())
        {
            Accelaration();
            Steering();
            sphereRB.AddForce(-transform.up * downforce * sphereRB.mass);
            carRB.MoveRotation(Quaternion.Slerp(carRB.rotation, Quaternion.FromToRotation(carRB.transform.up, hit.normal) * carRB.transform.rotation, 0.12f));
        }
        else
        {
            carRB.MoveRotation(Quaternion.Slerp(carRB.rotation,
            Quaternion.FromToRotation(carRB.transform.up, Vector3.up) *
            carRB.transform.rotation, 0.02f));

            sphereRB.linearVelocity = Vector3.Lerp(sphereRB.linearVelocity,
            sphereRB.linearVelocity + Vector3.down * gravity, Time.deltaTime * gravity);
        }
    }

    private void InputHandling()
    {
        inputValue = inputActions.Player.Movement.ReadValue<Vector2>();
        brakeInput = inputActions.Player.Brake.ReadValue<float>();
        driftInput = inputActions.Player.Drift.ReadValue<float>();
        itemInput = inputActions.Player.UseItem.ReadValue<float>();
        moveInput = inputValue.y;
        turnInput = inputValue.x;

        sign = Mathf.Sign(carVelocity.z);
        TurnMultiplyer = turnCurve.Evaluate(carVelocity.magnitude / maxSpeed);

        // Debug.Log("Move Input = " + moveInput + 
        // " Turn Input = " + turnInput + 
        // " Brake Input = " + brakeInput);
    }

    private void Accelaration()
    {
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            sphereRB.linearVelocity = Vector3.Lerp(sphereRB.linearVelocity,
            carRB.transform.forward * moveInput * maxSpeed,
            accelaration / 10 * Time.deltaTime);
        }
    }

    private void Steering()
    {
        if (driftInput > 0.1f)
        {
            Drifting();
        }
        if (moveInput > 0.1f || carVelocity.z > 1)
        {
            carRB.AddTorque(Vector3.up * turnInput * sign * turn * 100 * TurnMultiplyer);
        }
        else if (moveInput < -0.1f || carVelocity.z < -1)
        {
            carRB.AddTorque(Vector3.up * turnInput * sign * turn * 100 * TurnMultiplyer);
        }
    }

    private void Drifting()
    {
        TurnMultiplyer *= driftMultiplier;
    }

    private bool Grounded()
    {
        origin = sphereRB.position + sphereRB.GetComponent<SphereCollider>().radius * Vector3.up;
        var direction = -transform.up;
        var maxdistance = sphereRB.GetComponent<SphereCollider>().radius + 0.1f;

        if (Physics.SphereCast(origin, radius + 0.1f, direction, out hit, maxdistance, groundLayer)) //origin, radius + 0.1f, direction, out hit, maxdistance, groundLayer //sphereRB.position, Vector3.down, out hit, maxdistance, groundLayer
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Visuals()
    {
        if (carVelocity.z > 1)
        {
            BodyMesh.localRotation = Quaternion.Slerp(BodyMesh.localRotation, Quaternion.Euler(Mathf.Lerp(0, -5, carVelocity.z / maxSpeed),
            BodyMesh.localRotation.eulerAngles.y, BodyTilt * turnInput), 0.4f * Time.deltaTime / Time.fixedDeltaTime);
        }
        else
        {
            BodyMesh.localRotation = Quaternion.Slerp(BodyMesh.localRotation, Quaternion.Euler(0, 0, 0), 0.4f * Time.deltaTime / Time.fixedDeltaTime);
        }

        if (brakeInput > 0.1f)
        {
            BodyMesh.parent.localRotation = Quaternion.Slerp(BodyMesh.parent.localRotation,
            Quaternion.Euler(0, 45 * turnInput * Mathf.Sign(carVelocity.z), 0),
            0.1f * Time.deltaTime / Time.fixedDeltaTime);
        }
        else
        {
            BodyMesh.parent.localRotation = Quaternion.Slerp(BodyMesh.parent.localRotation,
            Quaternion.Euler(0, 0, 0),
            0.1f * Time.deltaTime / Time.fixedDeltaTime);
        }
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
            Debug.Log("Mobil terkena stun dari: " + source + " selama " + duration + " detik!");
            StartCoroutine(StunCoroutine(duration));
        }
    }
    //TODO : Habis UTS Ganti Boost Logic
    private IEnumerator StunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false;
        Debug.Log("Stun selesai.");
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
    //TODO : Habis UTS ganti Logic Boost
    public void TriggerBoostSlide(float speedIncrease, float duration)
    {
        StartCoroutine(BoostSpeedTemporarily(speedIncrease, duration));
    }

    private IEnumerator BoostSpeedTemporarily(float speedIncrease, float duration)
    {
        maxSpeed += speedIncrease;
        yield return new WaitForSeconds(duration);
        maxSpeed = originalMaxSpeed;
    }

    public void AddImpulseBoost()
    {
        if (Grounded())
        {
            Vector3 boostDirection = transform.forward;
            sphereRB.AddForce(boostDirection * boostPower, ForceMode.Impulse);
        }
    }

    public void ApplySlowEffect(float slowAmount, float duration)
    {
        if (!isSlowed)
        {
            isSlowed = true;
            maxSpeed *= slowAmount;
            if (slowEffectParticle != null)
            {
                slowEffectParticle.Play();
            }
            StartCoroutine(RemoveSlowEffect(duration));
        }
    }

    private IEnumerator RemoveSlowEffect(float duration)
    {
        yield return new WaitForSeconds(duration);
        maxSpeed = originalMaxSpeed;
        isSlowed = false;
        if (slowEffectParticle != null)
        {
            slowEffectParticle.Stop();
        }
    }
}
