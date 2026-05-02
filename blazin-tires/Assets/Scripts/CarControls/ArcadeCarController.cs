using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcadeCarController : MonoBehaviour
{
    private float moveInput;
    private float turnInput;
    private bool isCarGrounded;

    public float airDrag;
    public float groundDrag;

    public float fwdSpeed;
    public float reverseSpeed;
    public float turnSpeed;
    public LayerMask groundLayer;

    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float steerSpeedThreshold = 15f;
    [SerializeField] private float highSpeedSteerReduction = 0.4f;
    [SerializeField] private float lateralDragStrength = 0.6f;

    public Rigidbody sphereRB;

    void Start()
    {
        sphereRB.transform.parent = null;
    }

    void Update()
    {
        float rawVertical = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");

        moveInput = rawVertical * (rawVertical > 0 ? fwdSpeed : reverseSpeed);

        transform.position = sphereRB.transform.position;

        // Reduce steering authority at high speed for a more car-like feel
        float speedFactor = Mathf.Clamp01(sphereRB.linearVelocity.magnitude / steerSpeedThreshold);
        float steerMultiplier = Mathf.Lerp(1f, highSpeedSteerReduction, speedFactor);
        float newRotation = turnInput * turnSpeed * Time.deltaTime * rawVertical * steerMultiplier;
        transform.Rotate(0, newRotation, 0, Space.World);

        RaycastHit hit;
        isCarGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1f, groundLayer);

        if (isCarGrounded)
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

        sphereRB.linearDamping = isCarGrounded ? groundDrag : airDrag;
    }

    private void FixedUpdate()
    {
        if (isCarGrounded)
        {
            sphereRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration);

            // Cancel lateral (sideways) velocity to resist sliding
            Vector3 lateralVelocity = Vector3.Dot(sphereRB.linearVelocity, transform.right) * transform.right;
            sphereRB.AddForce(-lateralVelocity * lateralDragStrength, ForceMode.VelocityChange);

            // Enforce top speed
            if (sphereRB.linearVelocity.magnitude > maxSpeed)
                sphereRB.linearVelocity = sphereRB.linearVelocity.normalized * maxSpeed;
        }
        else
        {
            sphereRB.AddForce(transform.up * -30f);
        }
    }
}
