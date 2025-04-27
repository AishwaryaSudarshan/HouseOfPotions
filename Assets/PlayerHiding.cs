using UnityEngine;
// using UnityEngine.InputSystem; // Keep your input system if you use it for the real game

public class PlayerHiding : MonoBehaviour
{
    public HideAndSeekNPC npcAgent; // Assign NPC
    // public KeyCode hideKey = KeyCode.JoystickButton0; // Keep for real gameplay input
    // OR Input System Actions

    private bool isHiding = false;
    // private CharacterController characterController; // Simulator handles movement disabling now

    // Call this from the simulator script to change state
    public void SetPlayerHidingStatus(bool shouldHide)
    {
        if (isHiding == shouldHide) return; // No change needed

        isHiding = shouldHide;
        Debug.Log("Player Hiding Script: Status set to " + isHiding);

        // Tell the NPC about the player's state
        if (npcAgent != null)
        {
            npcAgent.SetPlayerHidingStatus(isHiding);
        }

        // Optional: Visual/Audio Feedback for hiding state
        // Optional: Disable player *input* script here if needed for real game
    }

    // Keep your input handling for when a human plays
    /*
    void Update()
    {
        // --- Input Handling ---
        if (Input.GetKeyDown(hideKey)) // Or your Input System check
        {
            // Toggle hiding based on input ONLY if the simulator isn't running
            // (You might need a check here for Training vs Real Gameplay)
            // SetPlayerHidingStatus(!isHiding);
        }
    }
    */
}