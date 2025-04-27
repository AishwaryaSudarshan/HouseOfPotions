using UnityEngine;
using TMPro;  // Import TextMesh Pro namespace

public class CircleDetector : MonoBehaviour
{
    // Assign a TextMesh Pro Text component from your scene in the Inspector.
    public TMP_Text debugText;  

    private bool gyroEnabled;
    private float previousYaw = 0f;
    private float netRotation = 0f;           // Accumulates the signed rotation for one continuous gesture
    private float timeSinceLastMovement = 0f;   // Timer for inactivity reset

    // Thresholds (adjust these as needed for smooth detection)
    private const float minMovementThreshold = 0.5f; // Minimum delta (in degrees) considered as meaningful movement
    private const float movementResetThreshold = 1f; // Time in seconds before resetting netRotation

    void Start()
    {
        gyroEnabled = EnableGyro();
        if (gyroEnabled)
        {
            // Initialize the previous yaw using the current sensor reading.
            previousYaw = GetCurrentYaw();
        }
    }

    bool EnableGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            return true;
        }
        else
        {
            Debug.Log("Gyroscope not supported on this device");
            if (debugText != null)
            {
                debugText.text = "Gyroscope not supported on this device";
            }
            return false;
        }
    }

    float GetCurrentYaw()
    {
        // Get the current yaw by converting the gyro's attitude to Euler angles.
        Quaternion attitude = Input.gyro.attitude;
        Vector3 euler = attitude.eulerAngles;
        return euler.y; // Use the y-axis for yaw
    }

    void Update()
    {
        if (!gyroEnabled)
            return;

        float currentYaw = GetCurrentYaw();
        // Calculate the signed difference between current and previous yaw (range -180 to 180)
        float deltaYaw = Mathf.DeltaAngle(previousYaw, currentYaw);
        previousYaw = currentYaw;

        // If the change is negligible, start counting inactivity
        if (Mathf.Abs(deltaYaw) < minMovementThreshold)
        {
            timeSinceLastMovement += Time.deltaTime;
            // If the user stops moving for longer than the threshold, reset the net rotation.
            if(timeSinceLastMovement >= movementResetThreshold)
            {
                netRotation = 0f;
            }
        }
        else // Movement detected, reset inactivity timer
        {
            timeSinceLastMovement = 0f;
        }

        // Accumulate the signed rotation
        netRotation += deltaYaw;

        // Update the TMP text with the current net rotation
        if (debugText != null)
        {
            debugText.text = $"Net Rotation: {netRotation:F2}°";
        }

        // Check if a singular circular motion (approximately 360°) has been performed continuously.
        if (Mathf.Abs(netRotation) >= 50f)
        {
            Debug.Log("Circle motion detected!");
            if (debugText != null)
            {
                debugText.text = "Circle motion detected!";
            }
            // Reset after detection so that subsequent rotations can be detected independently.
            netRotation = 0f;
        }
    }
}
