using UnityEngine;
using UnityEngine.UI;

public class InteractableObjectMenu : MonoBehaviour
{
    public Canvas objectMenuCanvas;
    public Button grabButton;
    public Button storeButton;
    public Button exitButton;

    public string activateMenuButton = "js0";
    public string grabButton_Bluetooth = "js10";
    public string dropButton_Bluetooth = "js8";

    private RaycastSelector raycastSelector;
    private CharacterMovement characterMovement;

    private GameObject currentTarget;
    public bool isMenuOpen = false;
    private GrabObj grabComponent;

    public Sprite inventorySprite;

    private void Start()
    {
        if (objectMenuCanvas != null)
        {
            objectMenuCanvas.gameObject.SetActive(false);
        }

        characterMovement = Object.FindFirstObjectByType<CharacterMovement>();
        raycastSelector = Object.FindFirstObjectByType<RaycastSelector>();
    }

    private void Update()
    {
        if (Input.GetButtonDown(activateMenuButton) && !isMenuOpen)
        {
            CheckForInteractableObjects();
        }

        if (isMenuOpen)
        {
            CheckButtonInteractions();
        }
    }

    private void CheckForInteractableObjects()
    {
        Camera mainCamera = Camera.main;

        // Use the same ray origin calculation as in RaycastSelector
        Vector3 rayOrigin = mainCamera.transform.position +
                            mainCamera.transform.forward * 0.3f + // Move forward
                            mainCamera.transform.up * -0.2f;      // Move down

        Ray ray = new(rayOrigin, mainCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastSelector.rayLength))
        {
            if (hit.collider.gameObject == gameObject && gameObject.CompareTag("InteractableObject"))
            {
                OpenMenuForObject(gameObject);
            }
        }
    }

    private void CheckButtonInteractions()
    {
        if (!objectMenuCanvas.gameObject.activeSelf)
        {
            return;
        }

        Camera mainCamera = Camera.main;

        // Use the same ray origin calculation as in RaycastSelector
        Vector3 rayOrigin = mainCamera.transform.position +
                            mainCamera.transform.forward * 0.3f + // Move forward
                            mainCamera.transform.up * -0.2f;      // Move down

        Ray ray = new(rayOrigin, mainCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastSelector.rayLength))
        {
            if (hit.collider.gameObject == grabButton.gameObject)
            {
                if (Input.GetButtonDown(grabButton_Bluetooth) && grabComponent != null)
                {
                    grabComponent.isGrabbed = !grabComponent.isGrabbed;
                    CloseMenu();
                    return;
                }
            }
            else if (hit.collider.gameObject == exitButton.gameObject)
            {
                if (Input.GetButtonDown(grabButton_Bluetooth))
                {
                    CloseMenu();
                    return;
                }
            }
            else if (hit.collider.gameObject == storeButton.gameObject)
            {
                if (Input.GetButtonDown(grabButton_Bluetooth))
                {
                    SettingsMenu settingsMenu = Object.FindFirstObjectByType<SettingsMenu>();

                    if (settingsMenu != null && currentTarget != null)
                    {
                        bool stored = settingsMenu.AddToInventory(currentTarget);

                        if (stored)
                        {
                            Debug.Log($"Successfully stored {currentTarget.name} in inventory");
                            grabComponent = null;
                        }
                        else
                        {
                            Debug.Log("Could not store object - inventory full");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Settings menu not found or no target to store");
                    }

                    CloseMenu();
                    return;
                }
            }
        }
    }

    private void OpenMenuForObject(GameObject targetObj)
    {
        InteractableObjectMenu[] allMenus = Object.FindObjectsByType<InteractableObjectMenu>(FindObjectsSortMode.None);
        foreach (InteractableObjectMenu menu in allMenus)
        {
            if (menu != this && menu.isMenuOpen)
            {
                menu.CloseMenu();
            }
        }

        currentTarget = targetObj;
        isMenuOpen = true;

        grabComponent = currentTarget.GetComponent<GrabObj>();
        if (grabComponent == null)
        {
            grabComponent = currentTarget.AddComponent<GrabObj>();
        }

        InventoryItem invItem = currentTarget.GetComponent<InventoryItem>();
        if (invItem == null)
        {
            invItem = currentTarget.AddComponent<InventoryItem>();
        }
        invItem.inventorySprite = inventorySprite;

        if (characterMovement != null)
        {
            characterMovement.enabled = false;
        }

        objectMenuCanvas.gameObject.SetActive(true);
    }

    public void CloseMenu()
    {
        objectMenuCanvas.gameObject.SetActive(false);
        isMenuOpen = false;
        currentTarget = null;

        if (characterMovement != null)
        {
            characterMovement.enabled = true;
        }
    }
}
