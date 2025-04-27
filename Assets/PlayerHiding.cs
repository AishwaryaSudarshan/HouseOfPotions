using UnityEngine;
using UnityEngine.UI;

public class PlayerHiding : MonoBehaviour
{
    [Header("References")]
    public HideAndSeekNPC npcAgent; // Assign NPC
    public MonoBehaviour playerMovementScript; // Assign your character movement script
    public CharacterController characterController; // Optional: Assign if using CharacterController

    [Header("Hide UI Effect")]
    public Canvas hideUICanvas; // Canvas for the hiding overlay
    public Color hideOverlayColor = new Color(0, 0, 0, 0.5f); // Semi-transparent overlay color

    [Header("Debug")]
    public bool isTrainingMode = false; // Set true when AI training is active

    // Platform-specific joystick button mapping
    private string hideButtonName;
    private bool isHiding = false;
    private Image overlayImage;

    void Start()
    {
        // Set up platform-specific button mappings
#if UNITY_STANDALONE_OSX
            hideButtonName = "js13";
#elif UNITY_ANDROID
            hideButtonName = "js2";
#else // Windows and others
        hideButtonName = "js1";
#endif

        // Initialize UI for hiding if not assigned in inspector
        if (hideUICanvas == null)
        {
            CreateHidingOverlay();
        }
        else
        {
            overlayImage = hideUICanvas.GetComponentInChildren<Image>();
            hideUICanvas.gameObject.SetActive(false);
        }

        // Auto-assign references if not set
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (playerMovementScript == null)
        {
            // Try to find common movement scripts
            playerMovementScript = GetComponent<MonoBehaviour>();

            // Try to find common movement script types
            if (playerMovementScript == null)
                playerMovementScript = GetComponent<CharacterMovement>();
            if (playerMovementScript == null)
                playerMovementScript = GetComponentInChildren<CharacterMovement>();
        }
    }

    void Update()
    {
        // Skip input handling if in training mode
        if (isTrainingMode) return;

        // Check for input only in real gameplay mode
        if (Input.GetButtonDown(hideButtonName))
        {
            ToggleHiding();
        }
    }

    // Create a UI overlay for hiding visualization
    private void CreateHidingOverlay()
    {
        // Create a new canvas for the overlay
        GameObject canvasObj = new GameObject("HidingOverlayCanvas");
        hideUICanvas = canvasObj.AddComponent<Canvas>();
        hideUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hideUICanvas.sortingOrder = 100; // Make sure it appears on top

        // Add a canvas scaler for proper UI scaling
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create an image that fills the screen
        GameObject imageObj = new GameObject("HidingOverlay");
        imageObj.transform.SetParent(canvasObj.transform, false);

        overlayImage = imageObj.AddComponent<Image>();
        overlayImage.color = hideOverlayColor;

        // Make the image fill the screen
        RectTransform rectTransform = overlayImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        hideUICanvas.gameObject.SetActive(false);
    }

    // Toggle hiding state based on player input
    public void ToggleHiding()
    {
        SetPlayerHidingStatus(!isHiding);
    }

    // Set hiding status (can be called from simulator or player input)
    public void SetPlayerHidingStatus(bool shouldHide)
    {
        if (isHiding == shouldHide) return; // No change needed

        isHiding = shouldHide;
        Debug.Log("Player Hiding Script: Status set to " + isHiding);

        // Enable/disable movement
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = !isHiding;
        }

        // Optional: freeze the character controller if one exists
        if (characterController != null)
        {
            characterController.enabled = !isHiding;
        }

        // Show/hide the overlay UI
        if (hideUICanvas != null)
        {
            hideUICanvas.gameObject.SetActive(isHiding);
        }

        // Tell the NPC about the player's state
        if (npcAgent != null)
        {
            npcAgent.SetPlayerHidingStatus(isHiding);
        }
    }

    // Public method for external access to hiding state
    public bool IsHiding()
    {
        return isHiding;
    }
}
