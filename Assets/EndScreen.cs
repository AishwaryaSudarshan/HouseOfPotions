using UnityEngine;

public class EndGameManager : MonoBehaviour
{
    public GameObject characterObject;        
    public GameObject endPanel;                
    private SimpleTimer simpleTimer;           
    private bool hasEnded = false;

    void Start()
    {
        if (characterObject != null)
        {
            simpleTimer = characterObject.GetComponent<SimpleTimer>();
        }

        if (endPanel != null)
        {
            endPanel.SetActive(false);  
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

        Time.timeScale = 0f;
        Application.Quit();
    }
}
