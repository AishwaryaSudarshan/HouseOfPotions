using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 
using System.Collections;
using TMPro; 

public class InventoryManager : MonoBehaviour
{
    public Canvas inventoryCanvas;
    public Transform inventoryItemsContainer;
    public GameObject inventoryItemPrefab;
    public Sprite defaultItemSprite;
    public int maxInventoryItems = 3;

    public string inventoryAxis = "Horizontal";
    public string navigateAxis = "Vertical";
    public string selectButton = "js10";
    public string dropButton = "js8";
    private bool isHeadNodDrop = false;

    public RaycastSelector raycastSelector;
    public CharacterMovement characterMovement;

    private GameObject[] inventorySlots;
    private GameObject[] inventoryObjects;
    private Sprite[] inventorySprites;

    private int currentSelectedIndex = 0;
    private bool inventoryActive = false;
    private GameObject currentlyGrabbedObject = null;

    private float inventoryNextNavigationTime = 0f;
    private readonly float navigationDelay = 0.3f;

    [SerializeField] private ParticleSystem dropParticleSystem;
    [SerializeField] private DropParticleEffectTrigger dropEffectTrigger;

    private bool gamePausedBeforeInventory = false; 
    public Button dropAllButton; 

    [Header("UI Messages")]
    public GameObject fullInventoryMessageObject; 
    public TextMeshProUGUI fullInventoryMessageText;
    public float messageDuration = 5f;
    private Coroutine hideMessageCoroutine;
    private bool wasInventoryFullLastFrame = false;

    private void Start()
    {
        #if UNITY_STANDALONE_OSX
                    dropButton = "js11";
                    selectButton = "js10";
        #elif UNITY_STANDALONE_WIN
                    dropButton = "js8";
                    selectButton = "js10";
        #elif UNITY_ANDROID
                dropButton = "js10";
                selectButton = "js5";
        #else
                    dropButton = "js10"; 
                    selectButton = "js5";
        #endif

        inventorySlots = new GameObject[maxInventoryItems];
        inventoryObjects = new GameObject[maxInventoryItems];
        inventorySprites = new Sprite[maxInventoryItems];
        if (fullInventoryMessageObject != null)
        {
            fullInventoryMessageObject.SetActive(false);
        }

        InitializeInventoryUI();
    }

    private void InitializeInventoryUI()
    {
        GridLayoutGroup existingGrid = inventoryItemsContainer.GetComponent<GridLayoutGroup>();
        if (existingGrid == null)
        {
            existingGrid = inventoryItemsContainer.gameObject.AddComponent<GridLayoutGroup>();
        }

        existingGrid.cellSize = new Vector2(80, 80);
        existingGrid.spacing = new Vector2(20, 15);
        existingGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        existingGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
        existingGrid.childAlignment = TextAnchor.MiddleCenter;
        existingGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        existingGrid.constraintCount = maxInventoryItems;

        RectTransform containerRect = inventoryItemsContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            containerRect.sizeDelta = new Vector2(maxInventoryItems * 100, 100);
        }

        for (int i = 0; i < maxInventoryItems; i++)
        {
            GameObject slotUI = Instantiate(inventoryItemPrefab, inventoryItemsContainer);
            slotUI.name = "InventorySlot_" + i; 

            RectTransform rt = slotUI.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;
            }

            Image image = slotUI.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = defaultItemSprite;
                image.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }

            
            Outline outline = slotUI.GetComponent<Outline>();
            if (outline == null)
            {
                outline = slotUI.AddComponent<Outline>();
                outline.enabled = false; 
            }

            inventorySlots[i] = slotUI;
        }

        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        bool isInventoryFull = IsInventoryFull();
        if (isInventoryFull && !wasInventoryFullLastFrame)
        {
            ShowMessage("Go to the alchemy room and drop ingredients into the pot!");
            fullInventoryMessageObject.SetActive(true);
        }
        wasInventoryFullLastFrame = isInventoryFull;
        if (inventoryActive)
        {
            HandleInventoryNavigation();
        }
        if (currentlyGrabbedObject != null && Input.GetButtonDown(dropButton))
        {
            DropObject();
        }
      
    }

    public void OpenInventory()
    {
        bool hasItems = false;
        for (int i = 0; i < maxInventoryItems; i++)
        {
            if (inventoryObjects[i] != null)
            {
                hasItems = true;
                break;
            }
        }

        if (!hasItems)
        {
            Debug.Log("Inventory is empty");
            return;
        }

        Debug.Log("Opening Inventory");
        inventoryActive = true;

       
        gamePausedBeforeInventory = Time.timeScale == 0;

        
        Time.timeScale = 1;

        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(true);

        
            dropAllButton = inventoryCanvas.transform.Find("DropAllButton")?.GetComponent<Button>();
            if (dropAllButton != null)
            {
                dropAllButton.gameObject.SetActive(true);
                HighlightDropAllButton(); 
            }
        }

       
        if (characterMovement != null)
        {
            characterMovement.enabled = false;
        }
       
        inventoryNextNavigationTime = Time.time + 0.5f;
    }

    private int FindFirstOccupiedSlot()
    {
        for (int i = 0; i < maxInventoryItems; i++)
        {
            if (inventoryObjects[i] != null)
            {
                return i;
            }
        }
        return 0;
    }

    private void HandleInventoryNavigation()
    {
        float verticalInput = Input.GetAxisRaw(navigateAxis);
        float horizontalInput = Input.GetAxisRaw(inventoryAxis);

        if (verticalInput > 0.5f) 
        {
            if (!IsDropAllButtonHighlighted())
            {
                UnhighlightInventoryItem();
                HighlightDropAllButton();
                inventoryNextNavigationTime = Time.time + navigationDelay;
                return;
            }
        }
        else if (verticalInput < -0.5f) 
        {
            if (IsDropAllButtonHighlighted())
            {
                UnhighlightDropAllButton();
                currentSelectedIndex = FindFirstOccupiedSlot();
                HighlightInventoryItem();
                inventoryNextNavigationTime = Time.time + navigationDelay;
                return;
            }
        }
        else if (horizontalInput > 0.5f || horizontalInput < -0.5f) 
        {
            if (!IsDropAllButtonHighlighted())
            {
                if (Time.time >= inventoryNextNavigationTime)
                {
                    if (horizontalInput > 0.5f)
                    {
                        int startIndex = currentSelectedIndex;
                        do
                        {
                            currentSelectedIndex = (currentSelectedIndex + 1) % maxInventoryItems;
                            if (inventoryObjects[currentSelectedIndex] != null || currentSelectedIndex == startIndex)
                            {
                                break;
                            }
                        } while (true);

                        inventoryNextNavigationTime = Time.time + navigationDelay;
                        HighlightInventoryItem();
                    }
                    else if (horizontalInput < -0.5f)
                    {
                        int startIndex = currentSelectedIndex;
                        do
                        {
                            currentSelectedIndex = (currentSelectedIndex - 1 + maxInventoryItems) % maxInventoryItems;
                            if (inventoryObjects[currentSelectedIndex] != null || currentSelectedIndex == startIndex)
                            {
                                break;
                            }
                        } while (true);

                        inventoryNextNavigationTime = Time.time + navigationDelay;
                        HighlightInventoryItem();
                    }
                }
                return;
            }
        }

        if (IsDropAllButtonHighlighted())
        {
            if (Input.GetButtonDown(selectButton))
            {
                DropAllObjectsIntoPot(); 
                return;
            }
        }

        if (Input.GetButtonDown(selectButton) && IsInventoryItemSelected())
        {
            GrabObjectFromInventory(currentSelectedIndex);
        }
    }
    
    private void HighlightInventoryItem()
     {
         for (int i = 0; i < maxInventoryItems; i++)
         {
             GameObject slot = inventorySlots[i];
             if (slot == null) continue;
 
             Image image = slot.GetComponent<Image>();
             Outline outline = slot.GetComponent<Outline>();
 
             if (image != null && outline != null)
             {
                 if (i == currentSelectedIndex && inventoryObjects[i] != null)
                 {
                     image.color = Color.yellow;
                     outline.enabled = true;
                     outline.OutlineColor = Color.red;
                     outline.OutlineWidth = 10f;
                 }
                 else if (inventoryObjects[i] != null)
                 {
                     image.color = Color.white;
                     outline.enabled = false;
                 }
                 else
                 {
                     image.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                     outline.enabled = false;
                 }
             }
             else
             {
                 Debug.LogWarning("Image or Outline component missing on inventory slot: " + slot.name);
             }
         }
     }
 

    private void UnhighlightInventoryItem()
    {
        for (int i = 0; i < maxInventoryItems; i++)
        {
            GameObject slot = inventorySlots[i];
            if (slot == null) continue;

            Image image = slot.GetComponent<Image>();
            Outline outline = slot.GetComponent<Outline>();

            if (image != null && outline != null)
            {
                if (inventoryObjects[i] != null)
                {
                    image.color = Color.white;
                }
                else
                {
                    image.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
                outline.enabled = false;
            }
            else
            {
                Debug.LogWarning("Image or Outline component missing on inventory slot: " + slot.name);
            }
        }
    }

    private bool IsInventoryItemSelected()
    {
        for (int i = 0; i < maxInventoryItems; i++)
        {
            Image image = inventorySlots[i].GetComponent<Image>();
            if (image != null && image.color == Color.yellow)
            {
                return true;
            }
        }
        return false;
    }

    private void GrabObjectFromInventory(int index)
    {
        if (index < 0 || index >= maxInventoryItems || inventoryObjects[index] == null)
        {
            return;
        }

        GameObject obj = inventoryObjects[index];

        Image slotImage = inventorySlots[index].GetComponent<Image>();
        if (slotImage != null)
        {
            slotImage.sprite = defaultItemSprite;
            slotImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        inventoryObjects[index] = null;
        inventorySprites[index] = null;

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

    public bool AddToInventory(GameObject obj, Sprite icon = null)
    {
        int emptySlotIndex = -1;
        for (int i = 0; i < maxInventoryItems; i++)
        {
            if (inventoryObjects[i] == null)
            {
                emptySlotIndex = i;
                break;
            }
        }

        if (emptySlotIndex == -1)
        {
            Debug.Log("Inventory is full");
            ShowMessage("Go to the alchemy room and drop ingredients into the pot!");
            return false;
        }

        if (icon == null)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                icon = sr.sprite;
            }
            else
            {
                InteractableObject interactable = obj.GetComponent<InteractableObject>();
                if (interactable != null)
                {
                    icon = interactable.GetInventoryIcon();
                }
            }
        }

        inventoryObjects[emptySlotIndex] = obj;
        inventorySprites[emptySlotIndex] = icon;

        Image slotImage = inventorySlots[emptySlotIndex].GetComponent<Image>();
        if (slotImage != null)
        {
            slotImage.sprite = icon != null ? icon : defaultItemSprite;
            slotImage.color = icon != null ? Color.white : new Color(1f, 0.7f, 0.7f, 1f);
        }
        obj.SetActive(false);
        Debug.Log($"Added {obj.name} to inventory slot {emptySlotIndex} with sprite: {(icon != null ? icon.name : "default")}");
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentlyGrabbedObject != null && other.CompareTag("Pot"))
        {
            Debug.Log("Collided with pot!");
            IngredientPot pot = other.GetComponentInParent<IngredientPot>();
            if (pot != null)
            {
                Debug.Log("Pot is detected!");
                pot.AddIngredient(currentlyGrabbedObject);
                Destroy(currentlyGrabbedObject);
                currentlyGrabbedObject = null;
                if (characterMovement != null && !characterMovement.enabled)
                    characterMovement.enabled = true;
            }
        }
    }
    public void DropObject()
{
    if (currentlyGrabbedObject == null)
        return;

    
    if (currentlyGrabbedObject.CompareTag("Potions"))
    {
        if (!isHeadNodDrop)
        {
            Debug.Log("Potions can only be dropped with a head nod!");
            return;
        }
        
        if (dropEffectTrigger != null)
        {
            dropEffectTrigger.TriggerDropEffect();
        }
        else if (dropParticleSystem != null)
        {
            dropParticleSystem.gameObject.SetActive(true);
            dropParticleSystem.Play();
        }

        // Trigger room update for the current room
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.ForceRoomUpdate();
        }

        Destroy(currentlyGrabbedObject);
        currentlyGrabbedObject = null;
        if (characterMovement != null && !characterMovement.enabled)
            characterMovement.enabled = true;
        return;
    }
   
    else if (isHeadNodDrop)
    {
        Debug.Log("Regular objects can only be dropped with the drop button!");
        return;
    }

       
        Ray ray = raycastSelector.CurrentRay;
        RaycastHit hit;
        float rayDistance = raycastSelector.rayLength;
        if (Physics.Raycast(ray, out hit, rayDistance))
        {
            if (hit.collider.CompareTag("Pot"))
            {
                IngredientPot pot = hit.collider.GetComponentInParent<IngredientPot>();
                if (pot != null)
                {
                    Debug.Log("Pot is detected!");
                    pot.AddIngredient(currentlyGrabbedObject);
                    Destroy(currentlyGrabbedObject);
                    currentlyGrabbedObject = null;
                    if (characterMovement != null && !characterMovement.enabled)
                        characterMovement.enabled = true;
                    return;
                }
            }
        }

        GrabObj grabComponent = currentlyGrabbedObject.GetComponent<GrabObj>();
        if (grabComponent != null)
            grabComponent.isGrabbed = false;
        Rigidbody rb = currentlyGrabbedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
        }
        currentlyGrabbedObject.transform.parent = null;
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 dropPosition = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;
            currentlyGrabbedObject.transform.position = dropPosition;
            if (dropParticleSystem != null)
            {
                dropParticleSystem.Play();
            }
        }
        currentlyGrabbedObject = null;
        if (characterMovement != null && !characterMovement.enabled)
            characterMovement.enabled = true;
    }



    public void DebugInventoryContents()
    {
        for (int i = 0; i < maxInventoryItems; i++)
        {
            if (inventoryObjects[i] != null)
            {
                Debug.Log($"Slot {i}: {inventoryObjects[i].name} - Sprite: {(inventorySprites[i] != null ? inventorySprites[i].name : "none")}");
            }
            else
            {
                Debug.Log($"Slot {i}: Empty");
            }
        }
    }
    public void DropObjectWithHeadNod()
    {
        isHeadNodDrop = true;
        DropObject();
        isHeadNodDrop = false;
    }

    public void ResetSelection()
    {
        currentSelectedIndex = 0;
    }

    public void CloseInventory()
    {
        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(false);
            inventoryActive = false;

        
            if (dropAllButton != null)
            {
                dropAllButton.gameObject.SetActive(false);
            }

          
            if (gamePausedBeforeInventory)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }

      
        if (characterMovement != null)
        {
            characterMovement.enabled = true;
        }
    }

    public void DropAllObjectsIntoPot()
    {
       
        GameObject visiblePot = FindVisiblePot();

        if (visiblePot == null)
        {
            Debug.LogWarning("No visible pot found to drop ingredients into. Make sure a pot is in view.");
            return;
        }

        IngredientPot pot = visiblePot.GetComponentInParent<IngredientPot>();
        if (pot == null)
        {
            Debug.LogWarning("Visible pot does not have an IngredientPot component.");
            return;
        }

        StartCoroutine(AnimateAndDropAll(pot));
    }

    private IEnumerator AnimateAndDropAll(IngredientPot pot)
    {
        for (int i = 0; i < maxInventoryItems; i++)
        {
            if (inventoryObjects[i] != null)
            {
                GameObject obj = inventoryObjects[i];
                inventoryObjects[i] = null; 
                inventorySprites[i] = null;

              
                Vector3 startPosition = inventorySlots[i].transform.position;
                Vector3 potPosition = pot.transform.position;
                Vector3 endPosition = potPosition + Vector3.up * 0.25f; 

               
                GameObject tempObject = Instantiate(obj);
                tempObject.SetActive(true); 
                tempObject.transform.position = startPosition;

                
                obj.SetActive(false);

                
                float animationDuration = 0.75f;
                float time = 0;
                while (time < animationDuration)
                {
                    time += Time.deltaTime;
                    float fraction = time / animationDuration;

                 
                    float height = Mathf.Sin(fraction * Mathf.PI) * 1.0f; 
                    Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, fraction);
                    currentPosition.y += height;

                    tempObject.transform.position = currentPosition;

                    yield return null;
                }

                
                pot.AddIngredient(tempObject);
                pot.addedIngredients.Add(tempObject); 

             
                Image slotImage = inventorySlots[i].GetComponent<Image>();
                if (slotImage != null)
                {
                    slotImage.sprite = defaultItemSprite;
                    slotImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
    
        
        if (pot.IsRecipeComplete())
        {
           
            if (dropAllButton != null)
            {
                dropAllButton.gameObject.SetActive(false);
            }
        }

        
        CloseInventory();
    }

    private GameObject FindVisiblePot()
    {
        
        if (raycastSelector != null)
        {
            Ray ray = raycastSelector.CurrentRay;
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, raycastSelector.rayLength))
            {
                if (hit.collider.CompareTag("Pot"))
                {
                    return hit.collider.gameObject;
                }
            }
        }
        
    
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main camera not found");
            return null;
        }
        
        GameObject[] pots = GameObject.FindGameObjectsWithTag("Pot");
        if (pots.Length == 0)
        {
            return null;
        }
        
       
        GameObject closestVisiblePot = null;
        float closestDistance = Mathf.Infinity;
        
        foreach (GameObject pot in pots)
        {
           
            Vector3 screenPoint = mainCamera.WorldToViewportPoint(pot.transform.position);
            bool isVisible = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
            
            if (isVisible)
            {
                float distance = Vector3.Distance(mainCamera.transform.position, pot.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestVisiblePot = pot;
                }
            }
        }
        
        return closestVisiblePot;
    }

    private void HighlightDropAllButton()
    {
        if (dropAllButton != null)
        {
            Image buttonImage = dropAllButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = Color.yellow; 
            }
        }
    }

    private void UnhighlightDropAllButton()
    {
        if (dropAllButton != null)
        {
            Image buttonImage = dropAllButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = Color.white; 
            }
        }
    }

    private bool IsDropAllButtonHighlighted()
    {
        if (dropAllButton != null)
        {
            Image buttonImage = dropAllButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                return buttonImage.color == Color.yellow;
            }
        }
        return false;
    }
    private void ShowMessage(string message)
    {
        if (fullInventoryMessageObject == null)
        {
            Debug.LogError("fullInventoryMessageObject is not assigned!");
            return;
        }
        if (fullInventoryMessageText != null)
        {
            fullInventoryMessageText.text = message;
        }
        else
        {
            Debug.LogWarning("fullInventoryMessageText is not assigned, but continuing to show message object");
        }

        fullInventoryMessageObject.SetActive(true);
        Debug.Log("Message object activated: " + fullInventoryMessageObject.activeSelf);
        
    
        if (hideMessageCoroutine != null)
        {
            StopCoroutine(hideMessageCoroutine);
        }
        

        hideMessageCoroutine = StartCoroutine(HideMessageAfterDelay());
    }
    
    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        if (fullInventoryMessageObject != null)
        {
            fullInventoryMessageObject.SetActive(false);
            Debug.Log("Message hidden after delay");
        }
        hideMessageCoroutine = null;
    }
    private bool IsInventoryFull()
    {
        for (int i = 0; i < maxInventoryItems; i++)
        {
            if (inventoryObjects[i] == null)
            {
                return false;
            }
        }
        return true;
    }

}