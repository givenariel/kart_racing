using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car2Con : MonoBehaviour
{

    [Header("Wheel Colliders")]
    public WheelCollider frontLeftWheelCollider;
    public WheelCollider frontRightWheelCollider;
    public WheelCollider rearLeftWheelCollider;
    public WheelCollider rearRightWheelCollider;

    [Header("Wheel Transforms")]
    public Transform frontLeftWheelTransform;
    public Transform frontRightWheelTransform;
    public Transform rearLeftWheelTransform;
    public Transform rearRightWheelTransform;

    [Header("Car Settings")]
    public float motorForce = 800f; // Power applied to wheels
    public float brakeForce = 3000f; // Braking force
    public float maxSteerAngle = 30f; // Max steering angle
    public float maxSpeed = 100f; // Maximum forward speed in km/h

    private Rigidbody rb;

    private float horizontalInput;
    private float verticalInput;
    private bool isBraking;

    private float currentBrakeForce;
    private float currentSteerAngle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0); // Adjust the center of mass for stability
    }

    void Update()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBraking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        float motorTorque = verticalInput * motorForce;

        // Apply motor torque to rear wheels only
        rearLeftWheelCollider.motorTorque = motorTorque;
        rearRightWheelCollider.motorTorque = motorTorque;

        // Apply brake force
        currentBrakeForce = isBraking ? brakeForce : 0f;
        ApplyBrakes();

        // Limit the vehicle's maximum speed
        float speed = rb.linearVelocity.magnitude * 3.6f; // Convert m/s to km/h
        if (speed > maxSpeed)
        {
            rearLeftWheelCollider.motorTorque = 0f;
            rearRightWheelCollider.motorTorque = 0f;
        }
    }

    private void ApplyBrakes()
    {
        frontLeftWheelCollider.brakeTorque = currentBrakeForce;
        frontRightWheelCollider.brakeTorque = currentBrakeForce;
        rearLeftWheelCollider.brakeTorque = currentBrakeForce;
        rearRightWheelCollider.brakeTorque = currentBrakeForce;
    }

    private void HandleSteering()
    {
        // Calculate steering angle based on input
        currentSteerAngle = maxSteerAngle * horizontalInput;

        // Apply steering to front wheels
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;

        // Get wheel collider position and rotation
        wheelCollider.GetWorldPose(out pos, out rot);

        // Update wheel transform position and rotation
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }
}
