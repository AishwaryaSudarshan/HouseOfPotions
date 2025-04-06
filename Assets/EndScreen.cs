using UnityEngine;

public class EndScreenManager : MonoBehaviour
{
    public GameObject endScreenPanel;    // Reference to the End Screen Panel
    public GameObject characterObject;   // Reference to the Character Object (can be used to disable movement)
    private SimpleTimer simpleTimer;     // Reference to the Timer script

    // Start is called before the first frame update
    void Start()
    {
        // Initially, the end screen panel should be disabled
        endScreenPanel.SetActive(false);

        // Get the SimpleTimer script attached to the Character object
        simpleTimer = characterObject.GetComponent<SimpleTimer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the timer has ended (targetTime is 0)
        if (simpleTimer.targetTime <= 0f && !endScreenPanel.activeSelf)
        {
            // Show the end screen when the timer reaches 0
            ShowEndScreen();
        }
    }

    // Function to show the end screen
    void ShowEndScreen()
    {
        // Disable the character's movement or other gameplay functionality
        if (characterObject != null)
        {
            // Assuming the character movement is a script attached to the character, disable it
            var characterMovementScript = characterObject.GetComponent<CharacterMovement>();
            if (characterMovementScript != null)
            {
                characterMovementScript.enabled = false; // Disables movement
            }
        }

        // Enable the end screen panel
        endScreenPanel.SetActive(true);

        // Optionally, disable other UI elements or gameplay functionality here
        // You can also add a blur effect here, but it's not necessary for now.
    }
}
