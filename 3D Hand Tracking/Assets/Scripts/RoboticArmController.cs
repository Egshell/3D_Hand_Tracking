using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboticArmController : MonoBehaviour
{
    [Header("References")]
    public HandTracking handTracking;
    public Transform wristPoint; // Point 0 (wrist) from hand tracking

    [Header("Arm Segments")]
    public Transform armBase;          // Base attached to floor
    public Transform armSegment1;      // Shoulder arm
    public Transform armSegment2;      // Elbow forearm

    [Header("Arm Settings")]
    public float maxReach = 3.5f;
    public float segment1Length = 2f;
    public float segment2Length = 1.5f;

    private Vector3 targetPosition;
    private bool isInRange = true;

    void Update()
    {
        if (handTracking != null && handTracking.HandPoints.Length > 0 && handTracking.HandPoints[0] != null)
        {
            // Get wrist position (point 0)
            targetPosition = handTracking.HandPoints[0].transform.position;

            // Keep base fixed to floor
            armBase.position = new Vector3(armBase.position.x, 0, armBase.position.z);

            // Check if hand is in range from arm base
            float distanceToBase = Vector3.Distance(armBase.position, targetPosition);
            isInRange = distanceToBase <= maxReach;

            if (isInRange)
            {
                UpdateArmIK();
            }
        }
    }

    void UpdateArmIK()
    {
        Vector3 toTarget = targetPosition - armBase.position;
        float distance = toTarget.magnitude;

        // Clamp distance to arm's maximum reach
        distance = Mathf.Clamp(distance, Mathf.Abs(segment1Length - segment2Length), segment1Length + segment2Length);

        // Calculate base rotation (Y-axis) - follows hand horizontally
        Vector3 horizontalDir = new Vector3(toTarget.x, 0, toTarget.z);
        if (horizontalDir.magnitude > 0.01f)
        {
            armBase.rotation = Quaternion.LookRotation(horizontalDir, Vector3.up);
        }

        // Calculate angles using law of cosines for proper bending
        float a = segment1Length;
        float b = segment2Length;
        float c = distance;

        float angleB = Mathf.Acos((a * a + c * c - b * b) / (2 * a * c)) * Mathf.Rad2Deg;
        float angleC = Mathf.Acos((a * a + b * b - c * c) / (2 * a * b)) * Mathf.Rad2Deg;

        // Calculate the angle between base and first segment
        float basePitchAngle = angleB;

        // Calculate elbow angle (bend angle between segments)
        float elbowAngle = 180f - angleC;

        // Apply rotations - ArmSegment1 rotates around base (floor pivot)
        armSegment1.rotation = armBase.rotation * Quaternion.Euler(basePitchAngle, 0, 0);

        // Position ArmSegment1 so its base is at floor pivot
        armSegment1.position = armBase.position;

        // Calculate end position of ArmSegment1
        Vector3 arm1End = armBase.position + armSegment1.up * segment1Length;

        // ArmSegment2 rotates around the end of ArmSegment1 (elbow pivot)
        armSegment2.rotation = armSegment1.rotation * Quaternion.Euler(elbowAngle, 0, 0);

        // Position ArmSegment2 so its base is at the end of ArmSegment1
        armSegment2.position = arm1End;

        // Calculate end position of ArmSegment2 (should match point 0)
        Vector3 arm2End = arm1End + armSegment2.up * segment2Length;

        // Ensure wrist point follows the end of ArmSegment2
        wristPoint.position = arm2End;
    }

    void OnDrawGizmos()
    {
        if (armBase != null)
        {
            // Draw reach range
            Gizmos.color = isInRange ? Color.green : Color.red;
            Gizmos.DrawWireSphere(armBase.position, maxReach);
            Gizmos.DrawLine(armBase.position, targetPosition);

            // Draw arm segments
            Gizmos.color = Color.blue;
            if (armSegment1 != null)
                Gizmos.DrawLine(armBase.position, armBase.position + armSegment1.up * segment1Length);
            if (armSegment2 != null)
                Gizmos.DrawLine(armSegment2.position, armSegment2.position + armSegment2.up * segment2Length);

            // Draw pivot points
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(armBase.position, 0.1f);
            if (armSegment1 != null)
                Gizmos.DrawSphere(armBase.position + armSegment1.up * segment1Length, 0.1f);

            // Draw target point
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(targetPosition, 0.1f);
        }
    }
}