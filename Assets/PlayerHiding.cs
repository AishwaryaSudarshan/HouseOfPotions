using UnityEngine;
using UnityEngine.InputSystem; // Or use the old Input Manager if preferred

public class PlayerHiding : MonoBehaviour
{
    public HideAndSeekNPC npcAgent; // Assign your NPC object in the Inspector
    public KeyCode hideKey = KeyCode.JoystickButton0; // Example: Cardboard trigger (might need adjustment based on Cardboard input setup)
    // OR use Input System Actions if you have them set up

    private bool isHiding = false;
    private CharacterController characterController; // Assuming you use CharacterController

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (npcAgent == null)
        {
            // Try find NPC automatically
            GameObject npcObj = GameObject.FindObjectOfType<HideAndSeekNPC>()?.gameObject;
            if (npcObj != null) npcAgent = npcObj.GetComponent<HideAndSeekNPC>();
            else Debug.LogError("NPC Agent not found or assigned on Player Hiding script!");
        }
    }

    void Update()
    {
        // --- Input Handling ---
        // Using old Input Manager:
        if (Input.GetKeyDown(hideKey))
        {
            ToggleHiding();
        }

        // Example using new Input System (requires setup):
        // if (Keyboard.current.spaceKey.wasPressedThisFrame) // Replace with your VR button action
        // {
        //    ToggleHiding();
        // }
    }

    void ToggleHiding()
    {
        isHiding = !isHiding;
        Debug.Log("Player Hiding: " + isHiding);

        // Tell the NPC about the player's state
        if (npcAgent != null)
        {
            npcAgent.SetPlayerHidingStatus(isHiding);
        }

        // Optional: Visual/Audio Feedback for player
        // e.g., change FOV, play sound, show UI icon

        // Optional: Make player unable to move while hiding
        if (characterController != null)
        {
            // You might need to disable your player movement script component instead/as well
            // characterController.enabled = !isHiding;
        }
    }
}