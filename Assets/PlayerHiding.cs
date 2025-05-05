using UnityEngine;
using UnityEngine.UI;

public class PlayerHiding : MonoBehaviour
{
    [Header("References")]
    public HideAndSeekNPC npcAgent;
    public MonoBehaviour playerMovementScript;
    public CharacterController characterController;
    public Camera uiCamera;

    [Header("Hide UI Effect")]
    public Canvas hideUICanvas;
    public Color hideOverlayColor = new Color(0, 0, 0, 0.5f);
    public float planeDistance = 10f;

    [Header("Debug")]
    public bool isTrainingMode = false;

    private string hideButtonName;
    private bool isHiding = false;
    private Image overlayImage;

    void Start()
    {
#if UNITY_STANDALONE_OSX
        hideButtonName = "js13";
#elif UNITY_ANDROID
        hideButtonName = "js2";
#else
        hideButtonName = "js1";
#endif

        if (uiCamera == null)
        {
            uiCamera = Camera.main;
        }

        if (hideUICanvas == null)
        {
            CreateHidingOverlay();
        }
        else
        {
            if (uiCamera != null)
            {
                hideUICanvas.renderMode = RenderMode.ScreenSpaceCamera;
                hideUICanvas.worldCamera = uiCamera;
                hideUICanvas.planeDistance = planeDistance;
            }

            overlayImage = hideUICanvas.GetComponentInChildren<Image>();
            hideUICanvas.gameObject.SetActive(false);
        }

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (playerMovementScript == null)
        {
            playerMovementScript = GetComponent<MonoBehaviour>();
            if (playerMovementScript == null)
                playerMovementScript = GetComponent<CharacterMovement>();
            if (playerMovementScript == null)
                playerMovementScript = GetComponentInChildren<CharacterMovement>();
        }
    }

    void Update()
    {
        if (isTrainingMode) return;

        if (Input.GetButtonDown(hideButtonName))
        {
            ToggleHiding();
        }
    }

    private void CreateHidingOverlay()
    {
        GameObject canvasObj = new GameObject("HidingOverlayCanvas");
        hideUICanvas = canvasObj.AddComponent<Canvas>();

        if (uiCamera != null)
        {
            hideUICanvas.renderMode = RenderMode.ScreenSpaceCamera;
            hideUICanvas.worldCamera = uiCamera;
            hideUICanvas.planeDistance = planeDistance;
        }
        else
        {
            hideUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        hideUICanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject imageObj = new GameObject("HidingOverlay");
        imageObj.transform.SetParent(canvasObj.transform, false);

        overlayImage = imageObj.AddComponent<Image>();
        overlayImage.color = hideOverlayColor;

        RectTransform rectTransform = overlayImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        hideUICanvas.gameObject.SetActive(false);
    }

    public void ToggleHiding()
    {
        SetPlayerHidingStatus(!isHiding);
    }

    public void SetPlayerHidingStatus(bool shouldHide)
    {
        if (isHiding == shouldHide) return;

        isHiding = shouldHide;

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = !isHiding;
        }

        if (characterController != null)
        {
            characterController.enabled = !isHiding;
        }

        if (hideUICanvas != null)
        {
            hideUICanvas.gameObject.SetActive(isHiding);
        }

        if (npcAgent != null)
        {
            npcAgent.SetPlayerHidingStatus(isHiding);
        }
    }

    public bool IsHiding()
    {
        return isHiding;
    }
}
