using UnityEngine;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{
    public GameObject startScreen;  
    public string selectButton;     
    private RaycastSelector raycastSelector; 
    
    [SerializeField] private MonoBehaviour npcMovementScript;

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
        
        raycastSelector = FindFirstObjectByType<RaycastSelector>();
        
        startScreen.SetActive(true);
        
        if (npcMovementScript != null)
        {
            npcMovementScript.gameObject.SetActive(false);
        }
        
        if (raycastSelector != null)
        {
            raycastSelector.enabled = false;
            if (raycastSelector.lineRenderer != null)
            {
                raycastSelector.lineRenderer.enabled = false;
            }
        }
    }
    
    private void Update()
    {
        if (Input.GetButtonDown(selectButton))
        {
            OnStartButtonClicked();
        }
    }
    private void OnStartButtonClicked()
    {
        startScreen.SetActive(false);
        if (npcMovementScript != null)
        {
            npcMovementScript.gameObject.SetActive(true);
        }
        
        if (raycastSelector != null)
        {
            raycastSelector.enabled = true;
            if (raycastSelector.lineRenderer != null)
            {
                raycastSelector.lineRenderer.enabled = true;
            }
        }
    }
}