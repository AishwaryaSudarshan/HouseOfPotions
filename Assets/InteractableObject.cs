using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string storeKey;
    [SerializeField]
    private Sprite inventoryIcon;

    private RaycastSelector raycastSelector;
    private InventoryManager inventoryManager; 

    private void Start()
    {
        #if UNITY_STANDALONE_OSX    
            storeKey = "js10";
        #elif UNITY_STANDALONE_WIN
            storeKey = "js10";
        #elif UNITY_ANDROID
            storeKey = "js5";
        #else
            storeKey = "js5"; 
        #endif

        raycastSelector = Object.FindFirstObjectByType<RaycastSelector>();
        inventoryManager = Object.FindFirstObjectByType<InventoryManager>();
    }

    private void Update()
    {
        if (Input.GetButtonDown(storeKey))
        {
            TryStoreInteractableObject();
        }
    }

    private void TryStoreInteractableObject()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || raycastSelector == null)
        {
            return;
        }

      
        Vector3 rayOrigin = mainCamera.transform.position +
                            (mainCamera.transform.forward * 0.3f) +
                            (mainCamera.transform.up * -0.2f);
        Ray ray = new(rayOrigin, mainCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastSelector.rayLength))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("InteractableObject") || hitObject.CompareTag("Potions"))
            {
                if (inventoryManager != null)
                {
                    
                    InteractableObject hitInteractable = hitObject.GetComponent<InteractableObject>();
                    
                   
                    Sprite spriteToUse = hitInteractable != null ? hitInteractable.GetInventoryIcon() : null;

                   
                    bool stored = inventoryManager.AddToInventory(hitObject, spriteToUse);
                    if (stored)
                    {
                        Debug.Log($"Stored {hitObject.name} in inventory with sprite: {(spriteToUse != null ? spriteToUse.name : "none")}");
                    }
                    else
                    {
                        Debug.Log("Inventory full. Could not store item.");
                    }
                }
                else
                {
                    Debug.LogWarning("InventoryManager not found.");
                }
            }
        }
    }

    
    public float GetDistanceFromMainCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main camera not found.");
            return 0f;
        }
        return Vector3.Distance(mainCamera.transform.position, transform.position);
    }


    public Sprite GetInventoryIcon()
    {
        return inventoryIcon;
    }
}
