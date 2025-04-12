using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GyroMotionDetector : MonoBehaviour
{
    #region Triangle Motion Settings
    [Header("Triangle Motion Settings")]
    [Tooltip("Acceleration magnitude above which we consider 'movement' valid.")]
    public float triangleAccelThreshold = 0.3f;
    [Tooltip("How different directions must be in degrees to count as a distinct 'turn'.")]
    public float angleChangeThreshold = 60f;
    [Tooltip("Max time in seconds to complete the full triangle gesture.")]
    public float triangleTimeWindow = 2.0f;

    private List<Vector3> _trianglePoints = new List<Vector3>();
    private float _triangleStartTime;
    #endregion

    #region Left-Right-Left Settings
    [Header("Left-Right-Left Motion Settings")]
    [Tooltip("Acceleration on x-axis to consider 'left' vs 'right'.")]
    public float lrAccelThreshold = 0.4f;
    [Tooltip("How much time (seconds) the user has to do L->R->L.")]
    public float lrTimeWindow = 1.2f;

    private int _lrStep = 0;
    private float _lrStartTime = 0f;
    #endregion

    #region Head Shake Settings
    [Header("Head Shake Settings")]
    [Tooltip("Enable gyro if you want yaw-based head shake detection.")]
    public bool useGyroForHeadShake = true;
    [Tooltip("How large the yaw rotation rate must be (in deg/sec) for a 'shake'.")]
    public float shakeRateThreshold = 2.0f;
    [Tooltip("How quickly the user must switch left->right->left to trigger a shake.")]
    public float shakeTimeWindow = 1.0f;

    private bool _gyroEnabled = false;
    private float _shakeStartTime = 0f;
    private int _shakeCount = 0;
    #endregion

    #region Head Nod Settings
    [Header("Head Nod Settings")]
    private int _nodCount = 0;
    private float _nodStartTime = 0f;
    public float nodTimeWindow = 1.2f;
    #endregion

    #region Head Tilt Right Settings (Hint Display)
    [Header("Head Tilt Right Settings")]
    [Tooltip("Original roll angle threshold (unused after adjustment).")]
    public float headTiltRightAngleThreshold = 25f;
    [Tooltip("Adjusted angle threshold (in degrees, negative) after remapping for head tilt right detection.")]
    public float headTiltRightAdjustedThreshold = -118f; // Updated threshold
    [Tooltip("Time (in seconds) that head tilt right must be held to trigger a hint.")]
    public float headTiltRightHoldDuration = 2.0f; // Updated hold duration
    [Tooltip("Direction to look into for the hint.")]
    public string hintDirection = "right";
    private bool isHeadTiltingRight = false;
    private float headTiltRightStartTime = 0f;
    #endregion



    #region Hint UI Elements
    [Header("Hint UI Elements")]
    [Tooltip("Panel that displays the hint. Assign your UI panel here.")]
    public GameObject hintPanel;
    [Tooltip("Image component to display the ingredient sprite.")]
    public Image hintImage;
    [Tooltip("Text component to display the hint title or message.")]
    public TMP_Text hintText;
    #endregion

    void Start()
    {
        if (SystemInfo.supportsGyroscope && useGyroForHeadShake)
        {
            Input.gyro.enabled = true;
            _gyroEnabled = true;
        }
    }

    void Update()
    {
        Vector3 accel = Input.acceleration;
        DetectTriangle(accel);
        DetectLeftRightLeft(accel);

        if (_gyroEnabled)
        {
            Vector3 rotationRateInDeg = Input.gyro.rotationRate * Mathf.Rad2Deg;
            DetectHeadShake(rotationRateInDeg.y);
            DetectHeadNod(rotationRateInDeg.x);
            DetectHeadTiltRight();
        }
        else
        {
        }
    }

    #region TRIANGLE DETECTION
    private void DetectTriangle(Vector3 accel)
    {
        if (accel.sqrMagnitude > triangleAccelThreshold * triangleAccelThreshold)
        {
            if (_trianglePoints.Count == 0)
            {
                _triangleStartTime = Time.time;
                _trianglePoints.Add(accel.normalized);
                return;
            }
            float elapsed = Time.time - _triangleStartTime;
            if (elapsed <= triangleTimeWindow)
            {
                Vector3 lastDir = _trianglePoints[_trianglePoints.Count - 1];
                float angle = Vector3.Angle(lastDir, accel.normalized);

                if (angle >= angleChangeThreshold)
                {
                    _trianglePoints.Add(accel.normalized);
                }

                if (_trianglePoints.Count >= 3)
                {
                    OnTriangleDetected();
                    _trianglePoints.Clear();
                }
            }
            else
            {
                _trianglePoints.Clear();
            }
        }
    }

    private void OnTriangleDetected()
    {
        Debug.Log("Triangle gesture detected! Mixing the potion...");
        IngredientPot pot = FindFirstObjectByType<IngredientPot>();
        if (pot != null)
        {
            pot.MixPotion();
        }
    }
    #endregion

    #region LEFT-RIGHT-LEFT DETECTION
    private void DetectLeftRightLeft(Vector3 accel)
    {
        float timeElapsed = Time.time - _lrStartTime;
        if (timeElapsed > lrTimeWindow)
        {
            _lrStep = 0;
        }

        if (_lrStep == 0 && accel.x < -lrAccelThreshold)
        {
            _lrStep = 1;
            _lrStartTime = Time.time;
        }
        else if (_lrStep == 1 && accel.x > lrAccelThreshold)
        {
            _lrStep = 2;
        }
        else if (_lrStep == 2 && accel.x < -lrAccelThreshold)
        {
            OnLeftRightLeftDetected();
            _lrStep = 0;
        }
    }

    private void OnLeftRightLeftDetected()
    {
        Debug.Log("Left-Right-Left gesture detected! Releasing the potion...");
        IngredientPot pot = FindFirstObjectByType<IngredientPot>();
        if (pot != null)
        {
            pot.ReleasePotion();
        }
    }
    #endregion

    #region HEAD SHAKE DETECTION (GYRO)
    private void DetectHeadShake(float yawDegPerSec)
    {
        float absYaw = Mathf.Abs(yawDegPerSec);
        if (absYaw > shakeRateThreshold)
        {
            if (_shakeCount == 0)
            {
                _shakeCount = 1;
                _shakeStartTime = Time.time;
            }
            else
            {
                float elapsed = Time.time - _shakeStartTime;
                if (elapsed > shakeTimeWindow)
                {
                    _shakeCount = 1;
                    _shakeStartTime = Time.time;
                }
                else
                {
                    _shakeCount++;
                    if (_shakeCount >= 3)
                    {
                        OnHeadShakeDetected();
                        _shakeCount = 0;
                    }
                }
            }
        }
    }

    private void OnHeadShakeDetected()
    {
        Debug.Log("Head shake gesture detected! (Optional: you can also trigger a hint here.)");
    }
    #endregion

    #region HEAD NOD DETECTION
    private void DetectHeadNod(float pitchDegPerSec)
    {
        float nodThreshold = 30.0f;
        if (Mathf.Abs(pitchDegPerSec) > nodThreshold)
        {
            if (_nodCount == 0)
            {
                _nodCount = 1;
                _nodStartTime = Time.time;
            }
            else
            {
                if (Time.time - _nodStartTime <= nodTimeWindow)
                {
                    _nodCount++;
                }
                else
                {
                    _nodCount = 1;
                    _nodStartTime = Time.time;
                }
            }
            if (_nodCount >= 3)
            {
                OnHeadNodDetected();
                _nodCount = 0;
            }
        }
    }
    private void OnHeadNodDetected()
    {
        Debug.Log("Head nod detected! Executing DropObject method.");
        InventoryManager invManager = FindFirstObjectByType<InventoryManager>();
        if (invManager != null)
        {
            invManager.DropObjectWithHeadNod();
        }
    }
    #endregion

    #region HEAD TILT RIGHT DETECTION FOR HINT
    private void DetectHeadTiltRight()
    {
        Quaternion att = Input.gyro.attitude;
        Quaternion deviceRotation = new Quaternion(att.x, att.y, -att.z, -att.w);
        Vector3 euler = deviceRotation.eulerAngles;
        float roll = euler.z;  

        float adjustedRoll = roll > 180f ? roll - 360f : roll;
        Debug.Log($"Adjusted roll: {adjustedRoll}");

        if (adjustedRoll <= headTiltRightAdjustedThreshold)
        {
            if (!isHeadTiltingRight)
            {
                isHeadTiltingRight = true;
                headTiltRightStartTime = Time.time;
            }
            else
            {
                if (Time.time - headTiltRightStartTime >= headTiltRightHoldDuration)
                {
                    OnHeadTiltRightAndHold();
                    isHeadTiltingRight = false;
                }
            }
        }
        else
        {
            isHeadTiltingRight = false;
        }
    }



    private void OnHeadTiltRightAndHold()
    {
        Debug.Log("Head tilt right held for required duration. Attempting to show hint...");

        IngredientPot pot = FindFirstObjectByType<IngredientPot>();
        if (pot == null)
        {
            Debug.LogWarning("IngredientPot not found.");
            return;
        }

        GameObject closestIngredient = pot.GetClosestRequiredIngredient();
        if (closestIngredient == null)
        {
            if (hintPanel != null && hintText != null)
            {
                hintText.text = "No Hints Remaining";
                if (hintImage != null)
                {
                    hintImage.gameObject.SetActive(false);
                }
                hintPanel.SetActive(true);
                StartCoroutine(HideHintPanelAfterDelay(3f));
            }
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 directionToIngredient = (closestIngredient.transform.position - mainCamera.transform.position).normalized;
            float dotForward = Vector3.Dot(mainCamera.transform.forward, directionToIngredient);
            float dotRight = Vector3.Dot(mainCamera.transform.right, directionToIngredient);

            if (Mathf.Abs(dotForward) >= Mathf.Abs(dotRight))
            {
                hintDirection = dotForward > 0 ? "ahead" : "behind";
            }
            else
            {
                hintDirection = dotRight > 0 ? "right" : "left";
            }
        }
        else
        {
            hintDirection = "right"; 
        }

        InteractableObject interactable = closestIngredient.GetComponent<InteractableObject>();
        if (interactable != null && hintImage != null)
        {
            Sprite hintSprite = interactable.GetInventoryIcon();
            if (hintSprite != null)
            {
                hintImage.sprite = hintSprite;
                hintImage.gameObject.SetActive(true);
            }
        }
        if (hintText != null)
        {
            hintText.text = $"Hint: Look {hintDirection}";
        }
        if (hintPanel != null)
        {
            hintPanel.SetActive(true);
            StartCoroutine(HideHintPanelAfterDelay(3f));
        }
    }



    private IEnumerator HideHintPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
    }
    #endregion
}