using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    private Dictionary<GameObject, Sprite> inventorySprites = new Dictionary<GameObject, Sprite>();

    public Canvas settingsMenuCanvas;
    public Button resumeButton;
    public Button inventoryButton;

    public Canvas inventoryCanvas;
    public Transform inventoryItemsContainer;
    public GameObject inventoryItemPrefab;
    public int maxInventoryItems = 3;

    public string openMenuButton = "js4"; //ok
    public string selectButton = "js10"; //b
    public string navigateAxis = "Vertical";
    public string inventoryAxis = "Horizontal";
    public string dropButton = "js8"; //a

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

    private void Start()
    {
#if UNITY_STANDALONE_OSX
            openMenuButton = "js7";
            selectButton = "js10";
            dropButton = "js11";
#elif UNITY_STANDALONE_WIN
        openMenuButton = "js4";
        selectButton = "js10";
        dropButton = "js8";
#elif UNITY_ANDROID
            openMenuButton = "js0"; 
            selectButton = "js5";
            dropButton = "js10";
#else
            openMenuButton = "js0"; 
            selectButton = "js5";
            dropButton = "js10";
#endif
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
        float verticalInput = Input.GetAxisRaw(navigateAxis); // Use your existing navigation axis

        if (Time.time >= inventoryNextNavigationTime)
        {
            // Horizontal navigation
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

            // Vertical navigation (move up/down by 7 items)
            if (verticalInput > 0.5f)
            {
                // Move up a row (subtract 7 if possible)
                int newIndex = currentSelectedIndex - 7;
                if (newIndex >= 0)
                {
                    currentSelectedIndex = newIndex;
                    inventoryNextNavigationTime = Time.time + navigationDelay;
                    HighlightInventoryItem();
                }
            }
            else if (verticalInput < -0.5f)
            {
                // Move down a row (add 7 if possible)
                int newIndex = currentSelectedIndex + 7;
                if (newIndex < inventoryUIItems.Count)
                {
                    currentSelectedIndex = newIndex;
                    inventoryNextNavigationTime = Time.time + navigationDelay;
                    HighlightInventoryItem();
                }
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

        GridLayoutGroup existingGrid = inventoryItemsContainer.GetComponent<GridLayoutGroup>();
        if (existingGrid != null)
        {
            DestroyImmediate(existingGrid);
        }

        GridLayoutGroup gridLayout = inventoryItemsContainer.gameObject.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(80, 80); // Size of each item
        gridLayout.spacing = new Vector2(20, 15); // Spacing between items (horizontal, vertical)
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 7;

        RectTransform containerRect = inventoryItemsContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            containerRect.sizeDelta = new Vector2(750, 250);
        }

        for (int i = 0; i < inventoryObjects.Count; i++)
        {
            GameObject itemUI = Instantiate(inventoryItemPrefab, inventoryItemsContainer);

            RectTransform rt = itemUI.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;
            }

            Image image = itemUI.GetComponent<Image>();
            if (image != null)
            {
                // Use the dictionary instead of InventoryItem component
                if (inventorySprites.TryGetValue(inventoryObjects[i], out Sprite sprite) && sprite != null)
                {
                    image.sprite = sprite;
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
    public bool AddToInventory(GameObject obj, Sprite icon = null)
    {
        if (inventoryObjects.Count >= maxInventoryItems)
        {
            if (characterMovement != null)
            {
                characterMovement.enabled = true;
            }

            if (raycastSelector != null && raycastSelector.lineRenderer != null)
            {
                raycastSelector.enabled = true;
                raycastSelector.lineRenderer.enabled = true;
            }

            return false;
        }

        // Store the sprite in the dictionary if provided
        if (icon != null)
        {
            inventorySprites[obj] = icon;
        }

        inventoryObjects.Add(obj);
        obj.SetActive(false);
        Debug.Log($"Added {obj.name} to inventory. Total items: {inventoryObjects.Count}");
        return true;
    }
}


