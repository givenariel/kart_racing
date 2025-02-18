using UnityEngine;
using Unity.Netcode; // Added for NGO

public class kartController : NetworkBehaviour // Inherit from NetworkBehaviour instead of MonoBehaviour
{
    [SerializeField] private Rigidbody sphereRB, carRB;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxSpeed, accelaration, turn, gravity = 7f, downforce = 5f, driftMultiplier = 1.5f;
    [SerializeField] private PhysicsMaterial frictionMaterial;
    [SerializeField] private AnimationCurve turnCurve;
    private RaycastHit hit;
    private Vector2 inputValue;
    private Vector3 origin;
    private Vector3 carVelocity;
    private float moveInput, turnInput, brakeInput, driftInput;
    private float TurnMultiplyer, sign, radius;
    IA_Default inputActions;

    // NGO integration: When the networked object spawns, disable input processing on non-owned instances.
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // Option 1: Disable the script so that non-owners do not process input/physics
            enabled = false;

            // Option 2: Alternatively, you could disable input handling only by checking IsOwner in Update/FixedUpdate.
        }
    }

    private void Start()
    {
        radius = sphereRB.GetComponent<SphereCollider>().radius;
        inputActions = new IA_Default();
        inputActions.Player.Movement.Enable();
        inputActions.Player.Brake.Enable();
        inputActions.Player.Drift.Enable();
    }

    private void Update()
    {
        // Only the owner should process local input.
        if (!IsOwner) return;
        InputHandling();
    }

    private void FixedUpdate()
    {
        // Only the owner should run the physics simulation.
        if (!IsOwner) return;

        carVelocity = carRB.transform.InverseTransformDirection(carRB.linearVelocity);

        if (Grounded())
        {
            Accelaration();
            Steering();
            sphereRB.AddForce(-transform.up * downforce * sphereRB.mass);
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
        if (Drifting())
        {
            TurnMultiplyer *= driftMultiplier;
        }
        if (moveInput > 0.1f || carVelocity.z > 1)
        {
            carRB.AddTorque(Vector3.up * turnInput * sign * turn * 100 * TurnMultiplyer);
        }
        else if (moveInput < -0.1f || carVelocity.z < -1)
        {
            carRB.AddTorque(Vector3.up * turnInput * sign * turn * 100 * TurnMultiplyer);
        }

        // if(Braking())
        // {
        //     sphereRB.constraints = RigidbodyConstraints.FreezeRotationX;
        // }
        // else
        // {
        //     sphereRB.constraints = RigidbodyConstraints.None;
        // }
    }

    private bool Drifting()
    {
        if (driftInput > 0.1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // private bool Braking(){
    //     if (brakeInput > 0.1f){ 
    //         return true; 
    //     }
    //     else
    //     {
    //         return false;
    //     }
    // }

    private bool Grounded()
    {
        origin = sphereRB.position + sphereRB.GetComponent<SphereCollider>().radius * Vector3.up;
        var direction = -transform.up;
        var maxdistance = sphereRB.GetComponent<SphereCollider>().radius + 0.2f;

        if (Physics.SphereCast(origin, radius + 0.1f, direction, out hit, maxdistance, groundLayer))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /*[ServerRpc]
    private void MoveServerRpc(Vector3 newPosition)
    {
        transform.position = newPosition;
    }*/

}
