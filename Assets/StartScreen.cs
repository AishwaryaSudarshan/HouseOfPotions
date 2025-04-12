using UnityEngine;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{
    public GameObject startScreen;  // Reference to the Start Screen UI
    public string selectButton;     // Input mapping for selecting the button
    private RaycastSelector raycastSelector; // Reference to the RaycastSelector component
    
    // Start is called before the first frame update
    private void Start()
    {
        #if UNITY_STANDALONE_OSX
            selectButton = "js10";
        #elif UNITY_STANDALONE_WIN
            selectButton = "js10";
        #elif UNITY_ANDROID
            selectButton = "js5";
        #else
            selectButton = "js5";
        #endif
        
        // Find the RaycastSelector in the scene
        raycastSelector = FindFirstObjectByType<RaycastSelector>();
        
        // Show the start screen initially
        startScreen.SetActive(true);
        
        // Disable the raycast when start screen is active
        if (raycastSelector != null)
        {
            raycastSelector.enabled = false;
            if (raycastSelector.lineRenderer != null)
            {
                raycastSelector.lineRenderer.enabled = false;
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Check for select button press
        if (Input.GetButtonDown(selectButton))
        {
            OnStartButtonClicked();
        }
    }

    // Handle Start Button Click
    private void OnStartButtonClicked()
    {
        // Disable the start screen panel
        startScreen.SetActive(false);

        // Re-enable the raycast when start screen is dismissed
        if (raycastSelector != null)
        {
            raycastSelector.enabled = true;
            if (raycastSelector.lineRenderer != null)
            {
                raycastSelector.lineRenderer.enabled = true;
            }
        }

        // Now, you can enable other game elements or allow interaction to begin here
    }
}