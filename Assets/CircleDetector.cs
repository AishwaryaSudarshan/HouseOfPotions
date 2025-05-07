using UnityEngine;
using TMPro;  

public class CircleDetector : MonoBehaviour
{
    public TMP_Text debugText;  

    private bool gyroEnabled;
    private float previousYaw = 0f;
    private float netRotation = 0f;         
    private float timeSinceLastMovement = 0f;   

    private const float minMovementThreshold = 0.5f; 
    private const float movementResetThreshold = 1f; 

    void Start()
    {
        gyroEnabled = EnableGyro();
        if (gyroEnabled)
        {
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
        Quaternion attitude = Input.gyro.attitude;
        Vector3 euler = attitude.eulerAngles;
        return euler.y;
    }

    void Update()
    {
        if (!gyroEnabled)
            return;

        float currentYaw = GetCurrentYaw();
        float deltaYaw = Mathf.DeltaAngle(previousYaw, currentYaw);
        previousYaw = currentYaw;

       
        if (Mathf.Abs(deltaYaw) < minMovementThreshold)
        {
            timeSinceLastMovement += Time.deltaTime;
            if(timeSinceLastMovement >= movementResetThreshold)
            {
                netRotation = 0f;
            }
        }
        else 
        {
            timeSinceLastMovement = 0f;
        }


        netRotation += deltaYaw;

       
        if (debugText != null)
        {
            debugText.text = $"Net Rotation: {netRotation:F2}°";
        }

        
        if (Mathf.Abs(netRotation) >= 50f)
        {
            Debug.Log("Circle motion detected!");
            if (debugText != null)
            {
                debugText.text = "Circle motion detected!";
            }
         
            netRotation = 0f;
        }
    }
}
