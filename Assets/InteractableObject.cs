using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string storeKey;
    [SerializeField]
    private Sprite inventoryIcon;

    private RaycastSelector raycastSelector;
    private InventoryManager inventoryManager; // Changed from SettingsMenu to InventoryManager

    private void Start()
    {
        #if UNITY_STANDALONE_OSX    
            storeKey = "js13";
        #elif UNITY_STANDALONE_WIN
            storeKey = "js1";
        #elif UNITY_ANDROID
            storeKey = "js2";
        #else
            storeKey = "js1"; // Default to js1 for other platforms
        #endif

        raycastSelector = Object.FindFirstObjectByType<RaycastSelector>();
        // Get the InventoryManager instance instead of SettingsMenu
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

        // Calculate the ray origin using the camera's position and orientation.
        Vector3 rayOrigin = mainCamera.transform.position +
                            (mainCamera.transform.forward * 0.3f) +
                            (mainCamera.transform.up * -0.2f);
        Ray ray = new(rayOrigin, mainCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastSelector.rayLength))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("InteractableObject"))
            {
                if (inventoryManager != null)
                {
                    // CHANGE HERE: Get the InteractableObject component from the hit object
                    InteractableObject hitInteractable = hitObject.GetComponent<InteractableObject>();
                    
                    // Use the hit object's icon, not this object's icon
                    Sprite spriteToUse = hitInteractable != null ? hitInteractable.GetInventoryIcon() : null;

                    // Use the InventoryManager's AddToInventory method.
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

    // Getter for the inventory icon.
    public Sprite GetInventoryIcon()
    {
        return inventoryIcon;
    }
}
