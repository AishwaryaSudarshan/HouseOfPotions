using UnityEngine;

public class RaycastTeleport : MonoBehaviour
{
    public Transform playerTransform;
    public RaycastSelector raycaster;
    private string teleportButton;

    private void Start()
    {
#if UNITY_STANDALONE_WIN
        teleportButton = "js0";
#elif UNITY_ANDROID
        teleportButton = "js3";
#else
                      teleportButton = "js0"; // Default to js0 for other platforms
#endif

        if (raycaster == null)
        {
            raycaster = Object.FindFirstObjectByType<RaycastSelector>();
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown(teleportButton))
        {
            TeleportToSpot();
        }
    }

    private void TeleportToSpot()
    {
        Camera mainCamera = Camera.main;

        Vector3 rayOrigin = mainCamera.transform.position;

        if (raycaster != null)
        {
            rayOrigin += mainCamera.transform.TransformDirection(raycaster.rayOriginOffset);
        }

        Ray teleportRay = new(rayOrigin, mainCamera.transform.forward);

        if (Physics.Raycast(teleportRay, out RaycastHit hitInfo, raycaster.rayLength) && hitInfo.collider.CompareTag("floorTag"))
        {
            CharacterController characterController = playerTransform.GetComponent<CharacterController>();
            Vector3 teleportPosition = new(hitInfo.point.x, 1.08f, hitInfo.point.z);

            if (characterController != null)
            {
                characterController.enabled = false;
                playerTransform.position = teleportPosition;
                characterController.enabled = true;
            }
        }
    }
}
