// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class InventoryManager : MonoBehaviour
// {
//     public Canvas inventoryCanvas;
//     public Transform inventoryItemsContainer;
//     public GameObject inventoryItemPrefab;
//     public Sprite defaultItemSprite; 
//     public int maxInventoryItems = 3;

//     public string inventoryAxis = "Horizontal";
//     public string navigateAxis = "Vertical";
//     public string selectButton = "js10";
//     public string dropButton = "js8";

//     public RaycastSelector raycastSelector;
//     public CharacterMovement characterMovement;

//     private GameObject[] inventorySlots;
//     private GameObject[] inventoryObjects;
//     private Sprite[] inventorySprites;
    

//     private int currentSelectedIndex = 0;
//     private bool inventoryActive = false;
//     private GameObject currentlyGrabbedObject = null;

//     private float inventoryNextNavigationTime = 0f;
//     private readonly float navigationDelay = 0.3f;

//     // Add this new field near the other member variable declarations
//     [SerializeField] private ParticleSystem dropParticleSystem;


//     private void Start()
//     {
//         #if UNITY_STANDALONE_OSX
//             dropButton = "js11";
//             selectButton = "js10";
//         #elif UNITY_STANDALONE_WIN
//             dropButton = "js8";
//             selectButton = "js10";
//         #elif UNITY_ANDROID
//             dropButton = "js10"; 
//             selectButton = "js5";
//         #else
//             dropButton = "js10"; 
//             selectButton = "js5";
//         #endif
        
//         inventorySlots = new GameObject[maxInventoryItems];
//         inventoryObjects = new GameObject[maxInventoryItems];
//         inventorySprites = new Sprite[maxInventoryItems];
        
//         InitializeInventoryUI();
//     }
    
//     private void InitializeInventoryUI()
//     {
//         GridLayoutGroup existingGrid = inventoryItemsContainer.GetComponent<GridLayoutGroup>();
//         if (existingGrid == null)
//         {
//             existingGrid = inventoryItemsContainer.gameObject.AddComponent<GridLayoutGroup>();
//         }
        
//         existingGrid.cellSize = new Vector2(80, 80);
//         existingGrid.spacing = new Vector2(20, 15);
//         existingGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
//         existingGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
//         existingGrid.childAlignment = TextAnchor.MiddleCenter;
//         existingGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
//         existingGrid.constraintCount = maxInventoryItems;

//         RectTransform containerRect = inventoryItemsContainer.GetComponent<RectTransform>();
//         if (containerRect != null)
//         {
//             containerRect.sizeDelta = new Vector2(maxInventoryItems * 100, 100);
//         }
        
//         for (int i = 0; i < maxInventoryItems; i++)
//         {
//             GameObject slotUI = Instantiate(inventoryItemPrefab, inventoryItemsContainer);
            
//             RectTransform rt = slotUI.GetComponent<RectTransform>();
//             if (rt != null)
//             {
//                 rt.localScale = Vector3.one;
//             }
            
//             Image image = slotUI.GetComponent<Image>();
//             if (image != null)
//             {
//                 image.sprite = defaultItemSprite;
//                 image.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); 
//             }
            
//             inventorySlots[i] = slotUI;
//         }
        
//         if (inventoryCanvas != null)
//         {
//             inventoryCanvas.gameObject.SetActive(false);
//         }
//     }

//     private void Update()
//     {
//         if (inventoryActive)
//         {
//             HandleInventoryNavigation();
//         }

//         if (currentlyGrabbedObject != null && Input.GetButtonDown(dropButton))
//         {
//             DropObject();
//         }
//     }

//     public void OpenInventory()
//     {
//         bool hasItems = false;
//         for (int i = 0; i < maxInventoryItems; i++)
//         {
//             if (inventoryObjects[i] != null)
//             {
//                 hasItems = true;
//                 break;
//             }
//         }

//         if (!hasItems)
//         {
//             Debug.Log("Inventory is empty");
//             return;
//         }

//         Debug.Log("Opening Inventory");
//         inventoryActive = true;

//         if (inventoryCanvas != null)
//         {
//             inventoryCanvas.gameObject.SetActive(true);
//         }

//         currentSelectedIndex = FindFirstOccupiedSlot();
//         HighlightInventoryItem();
//         inventoryNextNavigationTime = Time.time + 0.5f;
//     }
    
//     private int FindFirstOccupiedSlot()
//     {
//         for (int i = 0; i < maxInventoryItems; i++)
//         {
//             if (inventoryObjects[i] != null)
//             {
//                 return i;
//             }
//         }
//         return 0; 
//     }

//     private void HandleInventoryNavigation()
//     {
//         float horizontalInput = Input.GetAxisRaw(inventoryAxis);

//         if (Time.time >= inventoryNextNavigationTime)
//         {
//             if (horizontalInput > 0.5f)
//             {
                
//                 int startIndex = currentSelectedIndex;
//                 do {
//                     currentSelectedIndex = (currentSelectedIndex - 1 + maxInventoryItems) % maxInventoryItems;
//                     if (inventoryObjects[currentSelectedIndex] != null || currentSelectedIndex == startIndex)
//                     {
//                         break;
//                     }
//                 } while (true);
                
//                 inventoryNextNavigationTime = Time.time + navigationDelay;
//                 HighlightInventoryItem();
//             }
//             else if (horizontalInput < -0.5f)
//             {
//                 int startIndex = currentSelectedIndex;
//                 do {
//                     currentSelectedIndex = (currentSelectedIndex + 1) % maxInventoryItems;
//                     if (inventoryObjects[currentSelectedIndex] != null || currentSelectedIndex == startIndex)
//                     {
//                         break;
//                     }
//                 } while (true);
                
//                 inventoryNextNavigationTime = Time.time + navigationDelay;
//                 HighlightInventoryItem();
//             }

//             if (Input.GetButtonDown(selectButton) && inventoryObjects[currentSelectedIndex] != null)
//             {
//                 GrabObjectFromInventory(currentSelectedIndex);
//             }
//         }
//     }

//     private void HighlightInventoryItem()
//     {
//         for (int i = 0; i < maxInventoryItems; i++)
//         {
//             Image image = inventorySlots[i].GetComponent<Image>();
//             if (image != null)
//             {
//                 if (i == currentSelectedIndex && inventoryObjects[i] != null)
//                 {
//                     image.color = Color.yellow; // Highlight selected slot
//                 }
//                 else if (inventoryObjects[i] != null)
//                 {
//                     image.color = Color.white;
//                 }
//                 else
//                 {
//                     image.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); 
//                 }
//             }
//         }
//     }

//     private void GrabObjectFromInventory(int index)
//     {
//         if (index < 0 || index >= maxInventoryItems || inventoryObjects[index] == null)
//         {
//             return;
//         }

//         GameObject obj = inventoryObjects[index];

//         Image slotImage = inventorySlots[index].GetComponent<Image>();
//         if (slotImage != null)
//         {
//             slotImage.sprite = defaultItemSprite;
//             slotImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
//         }
    
//         inventoryObjects[index] = null;
//         inventorySprites[index] = null;

//         if (inventoryCanvas != null)
//         {
//             inventoryCanvas.gameObject.SetActive(false);
//         }
//         inventoryActive = false;
//         obj.SetActive(true);

//         GrabObj grabComponent = obj.GetComponent<GrabObj>();
//         if (grabComponent == null)
//         {
//             grabComponent = obj.AddComponent<GrabObj>();
//         }
//         grabComponent.isGrabbed = true;
//         currentlyGrabbedObject = obj;

//         if (raycastSelector != null && raycastSelector.lineRenderer != null)
//         {
//             raycastSelector.enabled = true;
//             raycastSelector.lineRenderer.enabled = true;
//         }
//     }
//     public bool AddToInventory(GameObject obj, Sprite icon = null)
//     {
//         int emptySlotIndex = -1;
//         for (int i = 0; i < maxInventoryItems; i++)
//         {
//             if (inventoryObjects[i] == null)
//             {
//                 emptySlotIndex = i;
//                 break;
//             }
//         }

//         if (emptySlotIndex == -1)
//         {
//             Debug.Log("Inventory is full");
//             return false;
//         }
      
//         if (icon == null)
//         {

//             SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
//             if (sr != null)
//             {
//                 icon = sr.sprite;
//             }
//             else
//             {
//                 InteractableObject interactable = obj.GetComponent<InteractableObject>();
//                 if (interactable != null)
//                 {
//                     icon = interactable.GetInventoryIcon();
//                 }
//             }
//         }

//         inventoryObjects[emptySlotIndex] = obj;
//         inventorySprites[emptySlotIndex] = icon;
    
//         Image slotImage = inventorySlots[emptySlotIndex].GetComponent<Image>();
//         if (slotImage != null)
//         {
//             slotImage.sprite = icon != null ? icon : defaultItemSprite;
//             slotImage.color = icon != null ? Color.white : new Color(1f, 0.7f, 0.7f, 1f);
//         }
//         obj.SetActive(false);
//         Debug.Log($"Added {obj.name} to inventory slot {emptySlotIndex} with sprite: {(icon != null ? icon.name : "default")}");
//         return true;
//     }
//     private void DropObject()
//     {
//         if (currentlyGrabbedObject == null)
//             return;
//         Ray ray = raycastSelector.CurrentRay;
//         RaycastHit hit;
//         float rayDistance = raycastSelector.rayLength;
//         if (Physics.Raycast(ray, out hit, rayDistance))
//         {
//             if (hit.collider.CompareTag("Pot"))
//             {
//                 IngredientPot pot = hit.collider.GetComponentInParent<IngredientPot>();
//                 if (pot != null)
//                 {
//                     Debug.Log("Pot is detected!");
//                     pot.AddIngredient(currentlyGrabbedObject);
//                     Destroy(currentlyGrabbedObject);
//                     currentlyGrabbedObject = null;
//                     if (characterMovement != null && !characterMovement.enabled)
//                         characterMovement.enabled = true;
//                     return;
//                 }
//             }
//         }
//         GrabObj grabComponent = currentlyGrabbedObject.GetComponent<GrabObj>();
//         if (grabComponent != null)
//             grabComponent.isGrabbed = false;
//         Rigidbody rb = currentlyGrabbedObject.GetComponent<Rigidbody>();
//         if (rb != null)
//         {
//             rb.isKinematic = false;
//             rb.useGravity = true;
//             rb.linearVelocity = Vector3.zero;
//         }
//         currentlyGrabbedObject.transform.parent = null;
//         Camera mainCam = Camera.main;
//         // if (mainCam != null)
//         // {
//         //     Vector3 dropPosition = mainCam.transform.position + mainCam.transform.forward * 1.5f;
//         //     currentlyGrabbedObject.transform.position = dropPosition;
//         // }
//         // In the DropObject() method, update the Camera block as follows:
//         if (mainCam != null)
//         {
//             Vector3 dropPosition = mainCam.transform.position + mainCam.transform.forward * 1.5f;
//             currentlyGrabbedObject.transform.position = dropPosition;
//             if(dropParticleSystem != null)
//             {
//                 dropParticleSystem.Play();
//             }
//         }
//         currentlyGrabbedObject = null;
//         if (characterMovement != null && !characterMovement.enabled)
//             characterMovement.enabled = true;
//     }



//     public void DebugInventoryContents()
//     {
//         for (int i = 0; i < maxInventoryItems; i++)
//         {
//             if (inventoryObjects[i] != null)
//             {
//                 Debug.Log($"Slot {i}: {inventoryObjects[i].name} - Sprite: {(inventorySprites[i] != null ? inventorySprites[i].name : "none")}");
//             }
//             else
//             {
//                 Debug.Log($"Slot {i}: Empty");
//             }
//         }
//     }
// }


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    // Particle system used for normal drop (fallback if no custom effect is found on the main camera).
    [SerializeField] private ParticleSystem dropParticleSystem;
    [SerializeField] private DropParticleEffectTrigger dropEffectTrigger;

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
            
            inventorySlots[i] = slotUI;
        }
        
        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
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

        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(true);
        }

        currentSelectedIndex = FindFirstOccupiedSlot();
        HighlightInventoryItem();
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
        float horizontalInput = Input.GetAxisRaw(inventoryAxis);

        if (Time.time >= inventoryNextNavigationTime)
        {
            if (horizontalInput > 0.5f)
            {
                int startIndex = currentSelectedIndex;
                do {
                    currentSelectedIndex = (currentSelectedIndex - 1 + maxInventoryItems) % maxInventoryItems;
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
                do {
                    currentSelectedIndex = (currentSelectedIndex + 1) % maxInventoryItems;
                    if (inventoryObjects[currentSelectedIndex] != null || currentSelectedIndex == startIndex)
                    {
                        break;
                    }
                } while (true);
                
                inventoryNextNavigationTime = Time.time + navigationDelay;
                HighlightInventoryItem();
            }

            if (Input.GetButtonDown(selectButton) && inventoryObjects[currentSelectedIndex] != null)
            {
                GrabObjectFromInventory(currentSelectedIndex);
            }
        }
    }

    private void HighlightInventoryItem()
    {
        for (int i = 0; i < maxInventoryItems; i++)
        {
            Image image = inventorySlots[i].GetComponent<Image>();
            if (image != null)
            {
                if (i == currentSelectedIndex && inventoryObjects[i] != null)
                {
                    image.color = Color.yellow; // Highlight selected slot
                }
                else if (inventoryObjects[i] != null)
                {
                    image.color = Color.white;
                }
                else
                {
                    image.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); 
                }
            }
        }
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

    // Modified DropObject method to handle "Potions" differently on release.
    // private void DropObject()
    // {
    //     if (currentlyGrabbedObject == null)
    //         return;

    //     // Check for objects with the "Potions" tag.
    //     if (currentlyGrabbedObject.CompareTag("Potions"))
    //     {
    //         // Find the main camera.
    //         Camera mainCam = Camera.main;
    //         if (mainCam != null)
    //         {
    //             // Try to get the DropParticleEffectTrigger component that is attached to the main camera (or one of its children).
    //             DropParticleEffectTrigger effectTrigger = mainCam.GetComponentInChildren<DropParticleEffectTrigger>();
    //             if (effectTrigger != null)
    //             {
    //                 effectTrigger.TriggerDropEffect();
    //             }
    //             else if (dropParticleSystem != null)
    //             {
    //                 // Fallback: use the assigned particle system.
    //                 dropParticleSystem.Play();
    //             }
    //         }
    //         // Remove the potion from the game (or disable it as needed).
    //         Destroy(currentlyGrabbedObject);
    //         currentlyGrabbedObject = null;
    //         if (characterMovement != null && !characterMovement.enabled)
    //             characterMovement.enabled = true;
    //         return;
    //     }
        
    //     // Normal drop handling for non-potion objects
    //     Ray ray = raycastSelector.CurrentRay;
    //     RaycastHit hit;
    //     float rayDistance = raycastSelector.rayLength;
    //     if (Physics.Raycast(ray, out hit, rayDistance))
    //     {
    //         if (hit.collider.CompareTag("Pot"))
    //         {
    //             IngredientPot pot = hit.collider.GetComponentInParent<IngredientPot>();
    //             if (pot != null)
    //             {
    //                 Debug.Log("Pot is detected!");
    //                 pot.AddIngredient(currentlyGrabbedObject);
    //                 Destroy(currentlyGrabbedObject);
    //                 currentlyGrabbedObject = null;
    //                 if (characterMovement != null && !characterMovement.enabled)
    //                     characterMovement.enabled = true;
    //                 return;
    //             }
    //         }
    //     }
    //     GrabObj grabComponent = currentlyGrabbedObject.GetComponent<GrabObj>();
    //     if (grabComponent != null)
    //         grabComponent.isGrabbed = false;
    //     Rigidbody rb = currentlyGrabbedObject.GetComponent<Rigidbody>();
    //     if (rb != null)
    //     {
    //         rb.isKinematic = false;
    //         rb.useGravity = true;
    //         rb.linearVelocity = Vector3.zero;
    //     }
    //     currentlyGrabbedObject.transform.parent = null;
    //     Camera mainCamera = Camera.main;
    //     if (mainCamera != null)
    //     {
    //         Vector3 dropPosition = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;
    //         currentlyGrabbedObject.transform.position = dropPosition;
    //         if(dropParticleSystem != null)
    //         {
    //             dropParticleSystem.Play();
    //         }
    //     }
    //     currentlyGrabbedObject = null;
    //     if (characterMovement != null && !characterMovement.enabled)
    //         characterMovement.enabled = true;
    // }

    private void DropObject()
    {
        if (currentlyGrabbedObject == null)
            return;

        // Special handling for objects tagged as "Potions"
        if (currentlyGrabbedObject.CompareTag("Potions"))
        {
            // Use the assigned drop effect trigger.
            if (dropEffectTrigger != null)
            {
                dropEffectTrigger.TriggerDropEffect();
            }
            else if (dropParticleSystem != null)
            {
                dropParticleSystem.gameObject.SetActive(true);
                dropParticleSystem.Play();
            }
            
            Destroy(currentlyGrabbedObject);
            currentlyGrabbedObject = null;
            if (characterMovement != null && !characterMovement.enabled)
                characterMovement.enabled = true;
            return;
        }
        
        // Normal drop handling for other objects
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
            if(dropParticleSystem != null)
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
}
