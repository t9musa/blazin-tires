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

    private void Awake()
    {
        carController = GetComponent<CarController2>();
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
