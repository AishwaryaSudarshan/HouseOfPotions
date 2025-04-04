using UnityEngine;
using TMPro;
using System.Collections;

public class SimpleTimer : MonoBehaviour
{
    // Set to 300 seconds (5 minutes) by default.
    public float targetTime = 300.0f;
    
    // Reference to the TextMeshPro text element.
    // Ensure this TMP_Text element is anchored at the top center of your Canvas.
    public TMP_Text timerText;
    
    // Flag to pause the timer when the global menu is active.
    // Set this flag via your global menu logic.
    public bool isPaused = false;
    
    void Update()
    {
        // Only decrement time if the game isn't paused.
        if (!isPaused)
        {
            targetTime -= Time.deltaTime;
        }
        
        // Clamp targetTime to avoid negative values.
        if (targetTime < 0.0f)
        {
            targetTime = 0.0f;
        }
        
        // Update the on-screen timer.
        UpdateTimerUI();
        
        // End the game when the timer reaches zero.
        if (targetTime <= 0.0f)
        {
            timerEnded();
        }
    }
    
    // Converts targetTime into minutes and seconds and updates the UI text.
    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(targetTime / 60f);
        int seconds = Mathf.FloorToInt(targetTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    // Called when time runs out.
    void timerEnded()
    {
        Debug.Log("game over out of time!");
        Application.Quit();
    }
}
