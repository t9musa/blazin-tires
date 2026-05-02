using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController2 : MonoBehaviour
{
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private bool isBreaking;
    private float currentBreakForce;
    Rigidbody carRigidbody;

    // Enable on the player car; leave off on AI cars (they use SetInputVector instead)
    [SerializeField] private bool isPlayerControlled = false;

    [SerializeField] private float motorForce = 1500f;
    [SerializeField] private float breakForce = 3000f;
    [SerializeField] private float maxSteerAngle = 30f;
    // Keeps car planted at speed — scales with velocity
    [SerializeField] private float downForce = 100f;
    // Resists body roll in corners
    [SerializeField] private float antiRollStrength = 5000f;

    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (isPlayerControlled)
            GetInput();

        HandleMotor();
        HandleSteering();
        UpdateWheels();
        ApplyDownForce();
        ApplyAntiRoll();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    public void SetInputVector(Vector2 inputVector)
    {
        horizontalInput = inputVector.x;
        verticalInput = inputVector.y;
    }

    public void SetInputs(float forwardAmount, float turnAmount)
    {
        verticalInput = forwardAmount;
        horizontalInput = turnAmount;
    }

    private void HandleMotor()
    {
        // Rear-wheel drive: livelier feel with natural oversteer on throttle
        rearLeftWheelCollider.motorTorque = verticalInput * motorForce;
        rearRightWheelCollider.motorTorque = verticalInput * motorForce;
        frontLeftWheelCollider.motorTorque = 0f;
        frontRightWheelCollider.motorTorque = 0f;

        currentBreakForce = isBreaking ? breakForce : 0f;
        frontLeftWheelCollider.brakeTorque = currentBreakForce;
        frontRightWheelCollider.brakeTorque = currentBreakForce;
        rearLeftWheelCollider.brakeTorque = currentBreakForce;
        rearRightWheelCollider.brakeTorque = currentBreakForce;

        // Coast drag when foot is off the gas
        if (verticalInput == 0f && !isBreaking)
            carRigidbody.linearDamping = Mathf.Lerp(carRigidbody.linearDamping, 0.5f, Time.fixedDeltaTime * 3f);
        else
            carRigidbody.linearDamping = 0f;
    }

    public void ApplyBreaking()
    {
        frontLeftWheelCollider.brakeTorque = breakForce;
        frontRightWheelCollider.brakeTorque = breakForce;
        rearLeftWheelCollider.brakeTorque = breakForce;
        rearRightWheelCollider.brakeTorque = breakForce;
    }

    private void HandleSteering()
    {
        // Lerp toward target angle for smooth steering response
        float targetAngle = maxSteerAngle * horizontalInput;
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle, Time.fixedDeltaTime * 10f);
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void ApplyDownForce()
    {
        carRigidbody.AddForce(-transform.up * downForce * carRigidbody.linearVelocity.magnitude);
    }

    private void ApplyAntiRoll()
    {
        ApplyAntiRollAxle(frontLeftWheelCollider, frontRightWheelCollider);
        ApplyAntiRollAxle(rearLeftWheelCollider, rearRightWheelCollider);
    }

    private void ApplyAntiRollAxle(WheelCollider leftWheel, WheelCollider rightWheel)
    {
        WheelHit hit;
        float travelLeft = 1f;
        float travelRight = 1f;

        bool groundedLeft = leftWheel.GetGroundHit(out hit);
        if (groundedLeft)
            travelLeft = (-leftWheel.transform.InverseTransformPoint(hit.point).y - leftWheel.radius)
                         / leftWheel.suspensionDistance;

        bool groundedRight = rightWheel.GetGroundHit(out hit);
        if (groundedRight)
            travelRight = (-rightWheel.transform.InverseTransformPoint(hit.point).y - rightWheel.radius)
                          / rightWheel.suspensionDistance;

        float antiRollForce = (travelLeft - travelRight) * antiRollStrength;

        if (groundedLeft)
            carRigidbody.AddForceAtPosition(leftWheel.transform.up * -antiRollForce, leftWheel.transform.position);
        if (groundedRight)
            carRigidbody.AddForceAtPosition(rightWheel.transform.up * antiRollForce, rightWheel.transform.position);
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
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    public float GetVelocityMagnitude()
    {
        return carRigidbody.linearVelocity.magnitude;
    }

    public double GetCarSpeed()
    {
        return carRigidbody.linearVelocity.magnitude * 3.6;
    }
}
