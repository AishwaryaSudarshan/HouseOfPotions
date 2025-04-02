using UnityEngine;

public class GrabObj : MonoBehaviour
{
    public bool isGrabbed = false;
    private Transform cameraTransform;
    private Rigidbody rigidBody;
    private string grabButton;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        rigidBody = GetComponent<Rigidbody>();
        if (rigidBody == null)
        {
            rigidBody = gameObject.AddComponent<Rigidbody>();
        }

        rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

#if UNITY_STANDALONE_WIN
        grabButton = "js8";
#elif UNITY_ANDROID
        grabButton = "js10";
#else
               grabButton = "js8"; // Default to js8 for other platforms
#endif
    }

    private void Update()
    {
        if (isGrabbed)
        {
            transform.position = cameraTransform.position + (cameraTransform.forward * 4f);
            rigidBody.isKinematic = true;
        }

        if (Input.GetButtonDown(grabButton))
        {
            if (isGrabbed)
            {
                isGrabbed = false;
                rigidBody.isKinematic = false;
            }
            else
            {
                Ray ray = new(cameraTransform.position, cameraTransform.forward);

                if (Physics.Raycast(ray, out RaycastHit hit, 10f))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        isGrabbed = true;
                    }
                }
            }
        }
    }
}
