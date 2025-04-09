using UnityEngine;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{
    public GameObject startScreen;
    public Button startButton;
    public GameObject characterObject;

    private SimpleTimer simpleTimer;

    private string selectButton;

    void Start()
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

        startScreen.SetActive(true);
        startButton.onClick.AddListener(OnStartButtonClicked);

        if (characterObject != null)
        {
            simpleTimer = characterObject.GetComponent<SimpleTimer>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(selectButton))
        {
            OnStartButtonClicked();
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
