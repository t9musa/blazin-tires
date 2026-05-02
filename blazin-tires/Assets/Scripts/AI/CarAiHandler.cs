using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class CarAiHandler : MonoBehaviour
{
    public enum AIMode { followPlayer, followWaypoints }
    public AIMode aiMode;

    [SerializeField] WaypointCircuit circuit;
    [SerializeField] float lookAheadDistance = 8f;

    Vector3 targetPosition = Vector3.zero;
    Transform targetTransform = null;
    float progressDistance = 0f;

    CarController2 carController;
    Rigidbody rb;

    float stuckTimer = 0f;
    const float stuckSpeedThreshold = 0.5f;
    const float stuckTimeLimit = 3f;
    const float stuckAdvanceDistance = 8f;

    private void Awake()
    {
        carController = GetComponent<CarController2>();
        rb = GetComponent<Rigidbody>();
    }

    void Start() { }

    private void FixedUpdate()
    {
        switch (aiMode)
        {
            case AIMode.followPlayer:
                FollowPlayer();
                break;
            case AIMode.followWaypoints:
                FollowWaypoints();
                break;
        }

        float steer = TurnTowardTarget();
        // Reduce throttle proportionally when steering hard into a corner
        float throttle = Mathf.Lerp(0.4f, 1.0f, 1f - Mathf.Abs(steer));

        carController.SetInputVector(new Vector2(steer, throttle));

        CheckStuck();
    }

    void CheckStuck()
    {
        if (rb == null || circuit == null) return;

        if (rb.linearVelocity.magnitude < stuckSpeedThreshold)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= stuckTimeLimit)
            {
                progressDistance += stuckAdvanceDistance;
                var resetPoint = circuit.GetRoutePoint(progressDistance);
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                transform.position = resetPoint.position + Vector3.up * 0.5f;
                transform.rotation = Quaternion.LookRotation(resetPoint.direction);
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    private void FollowPlayer()
    {
        if (targetTransform == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                targetTransform = playerObj.transform;
        }

        if (targetTransform != null)
            targetPosition = targetTransform.position;
    }

    private void FollowWaypoints()
    {
        if (circuit == null) return;

        var routePoint = circuit.GetRoutePoint(progressDistance);
        Vector3 toPoint = routePoint.position - transform.position;

        // Advance along the route once we've passed the current point
        if (Vector3.Dot(routePoint.direction, toPoint) < 0)
            progressDistance += 1f;

        targetPosition = circuit.GetRoutePoint(progressDistance + lookAheadDistance).position;
    }

    float TurnTowardTarget()
    {
        Vector2 vectorToTarget = targetPosition - transform.position;
        vectorToTarget.Normalize();

        float angleToTarget = Vector2.SignedAngle(transform.up, vectorToTarget) * -1f;
        float steerAmount = Math.Clamp(angleToTarget / 45f, -1f, 1f);
        return steerAmount;
    }
}
