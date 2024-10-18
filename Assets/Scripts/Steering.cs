using UnityEngine;

public class Steering : MonoBehaviour
{
    // Public variables to set in the Unity Editor
    public AnimationCurve turnRadiusCurve; // Curve that influences turn radius based on speed
    public float maxSpeed = 20f;           // Maximum speed of the vehicle
    public float minTurnRadius = 1f;       // Minimum possible turn radius
    public float maxTurnRadius = 15f;      // Maximum possible turn radius

    private float currentSpeed;            // Current speed of the vehicle

    // Method to calculate the turn radius based on speed
    public float CalculateTurnRadius(float speed)
    {
        // Normalize speed as a percentage of max speed
        float normalizedSpeed = speed / maxSpeed;

        // Evaluate the turn radius from the AnimationCurve (normalized to [0, 1])
        float curveValue = turnRadiusCurve.Evaluate(normalizedSpeed);

        // Interpolate between min and max turn radius based on the curve value
        return Mathf.Lerp(minTurnRadius, maxTurnRadius, curveValue);
    }

    // Method to determine the required speed for turning towards a target Transform
    public float CalculateRequiredSpeed(Transform target)
    {
        // Vector from the entity to the target
        Vector3 toTarget = target.position - transform.position;

        // Angle between forward direction and the direction to the target
        float angleToTarget = Vector3.Angle(transform.forward, toTarget);

        // If already pointing directly at the target, no need to slow down
        if (angleToTarget < 1f)
        {
            return maxSpeed;
        }

        // Calculate a desired speed based on the angle to the target (higher angle = slower speed)
        float speedFactor = Mathf.Clamp01(1f - (angleToTarget / 90f));

        // The calculated speed based on the angle (slow down for sharper turns)
        return Mathf.Lerp(0f, maxSpeed, speedFactor);
    }

    // Example function to update the steering logic
    public void UpdateSteering(Transform target)
    {
        // Calculate the turn radius based on the current speed
        float turnRadius = CalculateTurnRadius(currentSpeed);

        // Calculate the required speed to turn towards the target
        currentSpeed = CalculateRequiredSpeed(target);

        // Apply the turn radius to steer the vehicle (this is a simplified example)
        // You can apply turning forces, adjust rotation, etc., based on your game's logic.
        Debug.Log($"Turn radius: {turnRadius}, Required speed: {currentSpeed}");
    }
}
