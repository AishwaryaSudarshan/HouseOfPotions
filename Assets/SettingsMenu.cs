using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Canvas settingsMenuCanvas;
    public Button resumeButton;
    public Button inventoryButton;

    public string openMenuButton = "js4"; // ok
    public string openPotionMenu = "js9"; //hamburger menu
    public string selectButton = "js10"; // b
    public string navigateAxis = "Vertical";

    public RaycastSelector raycastSelector;
    public CharacterMovement characterMovement;
    public InventoryManager inventoryManager;

    // Reference to the TMP object
    public GameObject potionMenuTMPObject;

    // Reference to the IngredientPot script
    public IngredientPot ingredientPot;


    private readonly List<Button> menuButtons = new();
    private int currentSelectedIndex = 0;
    private bool menuActive = false;
    private float nextNavigationTime = 0f;
    private readonly float navigationDelay = 0.3f;

    private void Start()
    {
        #if UNITY_STANDALONE_OSX
            openMenuButton = "js7";
            selectButton = "js10";
            openPotionMenu = "js13";
        #elif UNITY_STANDALONE_WIN
            openMenuButton = "js4";
            selectButton = "js10";
        #elif UNITY_ANDROID
            openMenuButton = "js0"; 
            selectButton = "js5";
            openPotionMenu = "js11";
        #else
            openMenuButton = "js0"; 
            selectButton = "js5";
        #endif

        if (settingsMenuCanvas != null)
        {
            settingsMenuCanvas.gameObject.SetActive(false);
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
        //if (Input.GetButtonDown(openMenuButton) && !menuActive)
        //{
        //    OpenSettingsMenu();
        //}
        if (Input.GetButtonDown("js9"))
        {
            OpenInventoryMenu();
        }
        if (Input.GetButtonDown(openPotionMenu))
        {
            OpenPotionMenu();
        }
        if (menuActive)
        {
            HandleMenuNavigation();
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

    private void SetupButtonEvents()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(CloseSettingsMenu);
        }
        if (inventoryButton != null && inventoryManager != null)
        {
            inventoryButton.onClick.RemoveAllListeners();
            inventoryButton.onClick.AddListener(() =>
            {
                CloseSettingsMenu();
                inventoryManager.OpenInventory();
            });
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

    private void OpenPotionMenu()
    {
        if (potionMenuTMPObject != null)
        {
            bool isActive = potionMenuTMPObject.activeSelf;
            potionMenuTMPObject.SetActive(!isActive);

            if (!isActive)
            {
                TMP_Text potionMenuText = potionMenuTMPObject.GetComponentInChildren<TMP_Text>();
                if (potionMenuText != null && ingredientPot != null)
                {
                    potionMenuText.text = "Required Ingredients:\n";
                    foreach (GameObject ingredient in ingredientPot.requiredIngredients)
                    {
                        if (ingredient != null)
                        {
                            potionMenuText.text += ingredient.name + "\n";
                        }
                    }
                }
            }
        }
    }
    private void OpenInventoryMenu()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OpenInventory();
        }
    }
}