using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string storeKey;
    [SerializeField]
    private Sprite inventoryIcon;

    private RaycastSelector raycastSelector;
    private SettingsMenu settingsMenu;

    private void Start()
    {
#if UNITY_STANDALONE_WIN
        storeKey = "js1";
#elif UNITY_ANDROID
        storeKey = "js2";
#else
        storeKey = "js1"; // Default to js1 for other platforms
#endif

        raycastSelector = Object.FindFirstObjectByType<RaycastSelector>();
        settingsMenu = Object.FindFirstObjectByType<SettingsMenu>();
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

        // Use the same ray origin calculation as before.
        Vector3 rayOrigin = mainCamera.transform.position +
                            (mainCamera.transform.forward * 0.3f) +
                            (mainCamera.transform.up * -0.2f);
        Ray ray = new(rayOrigin, mainCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastSelector.rayLength))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("InteractableObject"))
            {
                if (settingsMenu != null)
                {
                    // Get sprite from this object or from the hit object
                    Sprite spriteToUse = inventoryIcon;

                    // If we don't have a sprite, check if the hit object has its own InteractableObject
                    if (spriteToUse == null && hitObject != gameObject)
                    {
                        InteractableObject hitInteractable = hitObject.GetComponent<InteractableObject>();
                        if (hitInteractable != null)
                        {
                            // Get sprite from hit object's InteractableObject if available
                            spriteToUse = hitInteractable.GetInventoryIcon();
                        }
                    }

                    // Pass the sprite directly to AddToInventory
                    bool stored = settingsMenu.AddToInventory(hitObject, spriteToUse);
                    if (stored)
                    {
                        Debug.Log($"Stored {hitObject.name} in inventory");
                    }
                    else
                    {
                        Debug.Log("Inventory full. Could not store item.");
                    }
                }
                else
                {
                    Debug.LogWarning("SettingsMenu not found.");
                }
            }
        }
    }

    // Add a getter for the inventory icon
    public Sprite GetInventoryIcon()
    {
        return inventoryIcon;
    }
}
