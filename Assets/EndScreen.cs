using UnityEngine;

public class EndGameManager : MonoBehaviour
{
    public GameObject characterObject;         // Drag the Character object here
    public GameObject endPanel;                // Drag the disabled Panel here
    private SimpleTimer simpleTimer;           // We'll access the timer from character
    private bool hasEnded = false;

    void Start()
    {
        if (characterObject != null)
        {
            simpleTimer = characterObject.GetComponent<SimpleTimer>();
        }

        if (endPanel != null)
        {
            endPanel.SetActive(false);  // make sure it's off at start
        }
    }

    void Update()
    {
        if (!hasEnded && simpleTimer != null && simpleTimer.targetTime <= 0f)
        {
            hasEnded = true;
            ShowEndScreen();
        }
    }

    void ShowEndScreen()
    {
        if (endPanel != null)
        {
            endPanel.SetActive(true);
        }

        // Optional: stop time or add blur later
        Time.timeScale = 0f;
        Application.Quit();
    }
}
