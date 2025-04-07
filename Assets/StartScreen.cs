using UnityEngine;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{
    public GameObject startScreen;   
    public Button startButton;          
    public GameObject characterObject;  

    private SimpleTimer simpleTimer;    

    void Start()
    {
        startScreen.SetActive(true);
        startButton.onClick.AddListener(OnStartButtonClicked);
        if (characterObject != null)
        {
            simpleTimer = characterObject.GetComponent<SimpleTimer>();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject == startButton.gameObject)
                {
                    OnStartButtonClicked();
                }
            }
        }
    }

    void OnStartButtonClicked()
    {
        startScreen.SetActive(false);
        if (simpleTimer != null)
        {
            simpleTimer.isPaused = false;
        }
    }
}
