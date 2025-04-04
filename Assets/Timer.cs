using UnityEngine;
using TMPro;
using System.Collections;

public class SimpleTimer : MonoBehaviour
{
    public float targetTime = 300.0f;
    public TMP_Text timerText;
    public bool isPaused = false;
    
    void Update()
    {
        if (!isPaused)
        {
            targetTime -= Time.deltaTime;
        }
    
        if (targetTime < 0.0f)
        {
            targetTime = 0.0f;
        }
        
        UpdateTimerUI();
    
        if (targetTime <= 0.0f)
        {
            timerEnded();
        }
    }
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
