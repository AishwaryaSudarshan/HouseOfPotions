// RaycastSelector.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RaycastSelector : MonoBehaviour
{
    public float rayLength = 2f; // Shorten the ray length
    public Color outlineColor = Color.red;
    public float outlineWidth = 5f;
    public LineRenderer lineRenderer;
    public Vector3 rayOriginOffset = new Vector3(0, 0, 0.1f);

    private Camera mainCamera;
    private GameObject currentHighlightedObject;
    private Outline currentOutline;
    private DropParticleEffectTrigger dropParticleTrigger;

    public Ray CurrentRay { get; private set; }

    [Header("UI Prompt")]
    public Canvas uiCanvas; // Canvas with Screen Space Camera rendering mode
    public TextMeshProUGUI promptText; // TMP text component
    public string interactPrompt = "Press B to store object";
    

    private void Start()
    {
        mainCamera = Camera.main;

        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        dropParticleTrigger = mainCamera.GetComponent<DropParticleEffectTrigger>();
        if (dropParticleTrigger == null)
        {
            Debug.LogWarning("No DropParticleEffectTrigger component found on the main camera.");
        }
         if (uiCanvas == null || promptText == null)
        {
            SetupUIPrompt();
        }
        
        // Hide prompt initially
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    private void SetupUIPrompt()
    {
        // Create canvas if needed
        if (uiCanvas == null)
        {
            GameObject canvasObj = new GameObject("PromptCanvas");
            uiCanvas = canvasObj.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            uiCanvas.worldCamera = mainCamera;
            canvasObj.AddComponent<CanvasScaler>();
        }
        
        // Create text if needed
        if (promptText == null)
        {
            GameObject textObj = new GameObject("PromptText");
            textObj.transform.SetParent(uiCanvas.transform, false);
            promptText = textObj.AddComponent<TextMeshProUGUI>();
            
            // Set text properties
            promptText.text = interactPrompt;
            promptText.fontSize = 36;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.color = Color.white;
            
            // Set position (center bottom of screen)
            RectTransform rt = promptText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.1f);
            rt.anchorMax = new Vector2(0.5f, 0.1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(500, 50);
            rt.anchoredPosition = Vector2.zero;
        }
    }
    
    private void Update()
    {
        Vector3 rayOrigin = mainCamera.transform.position +
                            (mainCamera.transform.forward * 0.3f) +
                            (mainCamera.transform.up * -0.2f);
        Ray ray = new Ray(rayOrigin, mainCamera.transform.forward);
        CurrentRay = ray;
        lineRenderer.SetPosition(0, rayOrigin);
        lineRenderer.enabled = true;

        if (Physics.Raycast(ray, out RaycastHit hit, rayLength))
        {
            lineRenderer.SetPosition(1, hit.point);
            if (hit.collider.CompareTag("InteractableObject") || hit.collider.CompareTag("Potions"))
            {
                if (promptText != null)
                {
                    promptText.gameObject.SetActive(true);
                }
                GameObject targetObject = hit.collider.gameObject;
                if (currentHighlightedObject != targetObject)
                {
                    currentHighlightedObject = targetObject;
                    currentOutline = targetObject.GetComponent<Outline>() ?? targetObject.AddComponent<Outline>();
                    currentOutline.OutlineMode = Outline.Mode.OutlineVisible;
                    currentOutline.OutlineColor = outlineColor;
                    currentOutline.OutlineWidth = outlineWidth;
                    currentOutline.enabled = true;
                }
            }
            else if (hit.collider.CompareTag("Pot"))
            {
                GameObject targetObject = hit.collider.gameObject;
                if (currentHighlightedObject != targetObject)
                {
                    currentHighlightedObject = targetObject;
                    currentOutline = targetObject.GetComponent<Outline>() ?? targetObject.AddComponent<Outline>();
                    currentOutline.OutlineMode = Outline.Mode.OutlineVisible;
                    currentOutline.OutlineColor = outlineColor;
                    currentOutline.OutlineWidth = outlineWidth;
                    currentOutline.enabled = true;
                }
                if (promptText != null)
                {
                    promptText.gameObject.SetActive(false);
                }
            }
            else
            {
                if (currentHighlightedObject != null && currentOutline != null)
                {
                    currentOutline.OutlineColor = Color.red;
                    currentHighlightedObject = null;
                }
                if (promptText != null)
                {
                    promptText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            lineRenderer.SetPosition(1, ray.origin + (ray.direction * rayLength));
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }

    public void CallDropParticleEffect()
    {
        if (dropParticleTrigger != null)
        {
            dropParticleTrigger.TriggerDropEffect();
        }
        else
        {
            Debug.LogWarning("Drop particle effect trigger component not found on main camera.");
        }
    }
}