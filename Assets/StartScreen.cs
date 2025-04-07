using UnityEngine;
using UnityEngine.UI;  // Import to access UI elements

public class StartScreenManager : MonoBehaviour
{
    public GameObject startScreen;  // Reference to the Start Screen UI
    public Button startButton;      // Reference to the Start Button

    // Start is called before the first frame update
    void Start()
    {
        // Show the start screen initially
        startScreen.SetActive(true);

        // Add listener for the start button click
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        // Check for raycast click directly using mouse button
        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Cast ray from mouse position

            if (Physics.Raycast(ray, out hit))
            {
                // Check if raycast hit the Start Button
                if (hit.transform.gameObject == startButton.gameObject)
                {
                    OnStartButtonClicked();  // Trigger the button click event
                }
            }
        }
    }

    // Handle Start Button Click
    void OnStartButtonClicked()
    {
        // Disable the start screen panel
        startScreen.SetActive(false);

        // Now, you can enable other game elements or allow interaction to begin here
    }
}
