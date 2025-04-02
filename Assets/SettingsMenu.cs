using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Canvas settingsMenuCanvas;
    public Button resumeButton;
    public Button inventoryButton;

    public Canvas inventoryCanvas;
    public Transform inventoryItemsContainer;
    public GameObject inventoryItemPrefab;
    public int maxInventoryItems = 3;

    public string openMenuButton = "js3";
    public string selectButton = "js10";
    public string navigateAxis = "Vertical";
    public string inventoryAxis = "Horizontal";
    public string dropButton = "js8";

    public RaycastSelector raycastSelector;
    public CharacterMovement characterMovement;

    private readonly List<Button> menuButtons = new();
    private readonly List<GameObject> inventoryObjects = new();
    private readonly List<GameObject> inventoryUIItems = new();

    private int currentSelectedIndex = 0;
    private bool menuActive = false;
    private bool inventoryActive = false;
    private GameObject currentlyGrabbedObject = null;

    private float nextNavigationTime = 0f;
    private readonly float navigationDelay = 0.3f;
    private float inventoryNextNavigationTime = 0f;

    public TextMeshProUGUI inventoryFullMessage;

    private void Start()
    {
        if (settingsMenuCanvas != null)
        {
            settingsMenuCanvas.gameObject.SetActive(false);
        }

        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(false);
        }

        if (raycastSelector == null)
        {
            raycastSelector = Object.FindFirstObjectByType<RaycastSelector>();
        }

        if (characterMovement == null)
        {
            characterMovement = Object.FindFirstObjectByType<CharacterMovement>();
        }

        if (resumeButton != null)
        {
            menuButtons.Add(resumeButton);
        }


        if (inventoryButton != null)
        {
            menuButtons.Add(inventoryButton);
        }

        SetupButtonEvents();
    }

    private void Update()
    {
        if (Input.GetButtonDown(openMenuButton) && !menuActive && !inventoryActive)
        {
            OpenSettingsMenu();
        }

        if (menuActive)
        {
            HandleMenuNavigation();
        }

        if (inventoryActive)
        {
            HandleInventoryNavigation();
        }

        if (currentlyGrabbedObject != null && Input.GetButtonDown(dropButton))
        {
            DropObject();
        }

        if (inventoryFullMessage != null && inventoryFullMessage.gameObject.activeSelf)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                inventoryFullMessage.transform.position = mainCam.transform.position + (mainCam.transform.forward * 3f);
            }
        }
    }

    private void HandleMenuNavigation()
    {
        float verticalInput = Input.GetAxisRaw(navigateAxis);

        if (Time.time >= nextNavigationTime)
        {
            if (verticalInput > 0.5f)
            {
                currentSelectedIndex = (currentSelectedIndex - 1 + menuButtons.Count) % menuButtons.Count;
                nextNavigationTime = Time.time + navigationDelay;
                HighlightCurrentButton();
            }
            else if (verticalInput < -0.5f)
            {
                currentSelectedIndex = (currentSelectedIndex + 1) % menuButtons.Count;
                nextNavigationTime = Time.time + navigationDelay;
                HighlightCurrentButton();
            }
        }

        if (Input.GetButtonDown(selectButton))
        {
            menuButtons[currentSelectedIndex].onClick.Invoke();
        }
    }

    private void HighlightCurrentButton()
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            Image btnImage = menuButtons[i].GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = (i == currentSelectedIndex) ? Color.yellow : Color.white;
            }
        }
    }

    private void HandleInventoryNavigation()
    {
        if (inventoryUIItems.Count == 0)
        {
            return;
        }

        float horizontalInput = Input.GetAxisRaw(inventoryAxis);

        if (Time.time >= inventoryNextNavigationTime)
        {
            if (horizontalInput > 0.5f)
            {
                currentSelectedIndex = (currentSelectedIndex - 1 + inventoryUIItems.Count) % inventoryUIItems.Count;
                inventoryNextNavigationTime = Time.time + navigationDelay;
                HighlightInventoryItem();
            }
            else if (horizontalInput < -0.5f)
            {
                currentSelectedIndex = (currentSelectedIndex + 1) % inventoryUIItems.Count;
                inventoryNextNavigationTime = Time.time + navigationDelay;
                HighlightInventoryItem();
            }

            if (Input.GetButtonDown(selectButton))
            {
                GrabObjectFromInventory(currentSelectedIndex);
            }
        }
    }

    private void HighlightInventoryItem()
    {
        for (int i = 0; i < inventoryUIItems.Count; i++)
        {
            Image image = inventoryUIItems[i].GetComponent<Image>();
            if (image != null)
            {
                image.color = (i == currentSelectedIndex) ? Color.yellow : Color.white;
            }
        }
    }

    private void SetupButtonEvents()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(CloseSettingsMenu);
        }

        if (inventoryButton != null)
        {
            inventoryButton.onClick.RemoveAllListeners();
            inventoryButton.onClick.AddListener(OpenInventory);
        }
    }

private void OpenSettingsMenu()
{
    menuActive = true;

    if (raycastSelector != null && raycastSelector.lineRenderer != null)
    {
        raycastSelector.lineRenderer.enabled = false;
        raycastSelector.enabled = false;
    }

    InteractableObjectMenu[] objectMenus = Object.FindObjectsByType<InteractableObjectMenu>(FindObjectsSortMode.None);
    foreach (InteractableObjectMenu menu in objectMenus)
    {
        if (menu != null && menu.isMenuOpen)
        {
            menu.CloseMenu();
        }
    }

    if (characterMovement != null)
    {
        characterMovement.enabled = false;
    }

    if (settingsMenuCanvas != null)
    {
        settingsMenuCanvas.gameObject.SetActive(true);

        if (settingsMenuCanvas.renderMode == RenderMode.WorldSpace)
        {
            Camera mainCam = Camera.main;
            // Position menu directly in front of the camera
            settingsMenuCanvas.transform.position = mainCam.transform.position + (mainCam.transform.forward * 2f);
            settingsMenuCanvas.transform.rotation = mainCam.transform.rotation;
            Debug.Log("Settings menu location: " + settingsMenuCanvas.transform.position);
        }
    }

    currentSelectedIndex = 0;
    HighlightCurrentButton();
}



    private void CloseSettingsMenu()
    {
        menuActive = false;

        if (settingsMenuCanvas != null)
        {
            settingsMenuCanvas.gameObject.SetActive(false);
        }

        if (characterMovement != null)
        {
            characterMovement.enabled = true;
        }

        if (raycastSelector != null && raycastSelector.lineRenderer != null)
        {
            raycastSelector.enabled = true;
            raycastSelector.lineRenderer.enabled = true;
        }
    }

    private void OpenInventory()
    {
        if (inventoryObjects.Count == 0)
        {
            Debug.Log("Inventory is empty");
            return;
        }

        Debug.Log("Opening Inventory");
        menuActive = false;
        inventoryActive = true;

        if (settingsMenuCanvas != null)
        {
            settingsMenuCanvas.gameObject.SetActive(false);
        }

        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(true);
            Debug.Log("Inventory canvas activated");

            if (inventoryCanvas.renderMode == RenderMode.WorldSpace && settingsMenuCanvas != null)
            {
                inventoryCanvas.transform.position = settingsMenuCanvas.transform.position;
                inventoryCanvas.transform.rotation = settingsMenuCanvas.transform.rotation;
            }
        }

        RefreshInventoryUI();

        inventoryNextNavigationTime = Time.time + 0.5f;

        currentSelectedIndex = 0;
        HighlightInventoryItem();
    }

    private void RefreshInventoryUI()
    {
        Debug.Log("Refreshing inventory UI - Items count: " + inventoryObjects.Count);

        foreach (GameObject item in inventoryUIItems)
        {
            Destroy(item);
        }
        inventoryUIItems.Clear();

        HorizontalLayoutGroup layoutGroup = inventoryItemsContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = inventoryItemsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.spacing = 15f;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
        }

        for (int i = 0; i < inventoryObjects.Count; i++)
        {
            GameObject itemUI = Instantiate(inventoryItemPrefab, inventoryItemsContainer);

            RectTransform rt = itemUI.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;
                rt.sizeDelta = new Vector2(80, 80);
            }

            Image image = itemUI.GetComponent<Image>();
            if (image != null)
            {
                InventoryItem invItem = inventoryObjects[i].GetComponent<InventoryItem>();
                if (invItem != null && invItem.inventorySprite != null)
                {
                    image.sprite = invItem.inventorySprite;
                    image.color = Color.white;
                }
                else
                {
                    image.color = new Color(0.8f, 0.8f, 0.8f);
                }
                image.enabled = true;
            }

            itemUI.SetActive(true);
            inventoryUIItems.Add(itemUI);
            Debug.Log($"Created inventory UI item {i + 1} for: {inventoryObjects[i].name}");
        }

        currentSelectedIndex = 0;
        HighlightInventoryItem();
    }


    private void GrabObjectFromInventory(int index)
    {
        if (index < 0 || index >= inventoryObjects.Count)
        {
            return;
        }

        GameObject obj = inventoryObjects[index];
        inventoryObjects.RemoveAt(index);

        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(false);
        }

        inventoryActive = false;

        obj.SetActive(true);

        GrabObj grabComponent = obj.GetComponent<GrabObj>();
        if (grabComponent == null)
        {
            grabComponent = obj.AddComponent<GrabObj>();
        }

        grabComponent.isGrabbed = true;
        currentlyGrabbedObject = obj;

        if (raycastSelector != null && raycastSelector.lineRenderer != null)
        {
            raycastSelector.enabled = true;
            raycastSelector.lineRenderer.enabled = true;
        }
    }


    private void DropObject()
    {
        if (currentlyGrabbedObject == null)
        {
            return;
        }

        GrabObj grabComponent = currentlyGrabbedObject.GetComponent<GrabObj>();
        if (grabComponent != null)
        {
            grabComponent.isGrabbed = false;
        }

        currentlyGrabbedObject = null;

        if (characterMovement != null && !characterMovement.enabled)
        {
            characterMovement.enabled = true;
        }
    }

    public bool AddToInventory(GameObject obj)
    {
        if (inventoryObjects.Count >= maxInventoryItems)
        {
            InteractableObjectMenu[] objectMenus = Object.FindObjectsByType<InteractableObjectMenu>(FindObjectsSortMode.None);
            foreach (InteractableObjectMenu menu in objectMenus)
            {
                if (menu != null && menu.isMenuOpen)
                {
                    menu.CloseMenu();
                }
            }

            if (characterMovement != null)
            {
                characterMovement.enabled = true;
            }

            if (raycastSelector != null && raycastSelector.lineRenderer != null)
            {
                raycastSelector.enabled = true;
                raycastSelector.lineRenderer.enabled = true;
            }

            if (inventoryFullMessage != null)
            {
                inventoryFullMessage.gameObject.SetActive(true);
                inventoryFullMessage.text = "Inventory is full";
                Invoke("HideInventoryFullMessage", 2f);
            }

            return false;
        }

        inventoryObjects.Add(obj);
        obj.SetActive(false);
        Debug.Log($"Added {obj.name} to inventory. Total items: {inventoryObjects.Count}");
        return true;
    }

    private void HideInventoryFullMessage()
    {
        if (inventoryFullMessage != null)
        {
            inventoryFullMessage.gameObject.SetActive(false);
        }
    }
}

public class InventoryItem : MonoBehaviour
{
    public Sprite inventorySprite;
}


