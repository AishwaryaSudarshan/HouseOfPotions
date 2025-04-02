using UnityEngine;
using System.Collections;

public class GrabAction : MonoBehaviour
{
    public delegate void ObjectGrabHandler();
    public event ObjectGrabHandler OnObjectGrabbed;
    public event ObjectGrabHandler OnObjectReleased;

    [Header("Object and Menu References")]
    public GameObject objectToGrab;   

    [Header("Hold Settings")]
    public Vector3 sideOffset = new Vector3(-1, 0, 2);
    public float dropForce = 0.5f;     
    public float velocityDamping = 0.5f; 

    private Transform originalParent;   
    private Rigidbody objectRb;         
    private bool isGrabbed = false;     

    private string aButton;

    private void Start()
    {
        #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            aButton = "js11";
        #elif UNITY_ANDROID
            aButton = "js10";
        #else
            aButton = "js11"; 
        #endif
        if (objectToGrab != null)
        {
            objectToGrab.layer = LayerMask.NameToLayer("TargetTag");
        }
    }

    public void ExecuteAction()
    {
        CloseAllMenus();

        if (!isGrabbed)
        {
            Grab();
        }
    }

    private void CloseAllMenus()
    {
        if (objectToGrab != null && objectToGrab.transform.childCount == 1)
        {
            Transform menuChild = objectToGrab.transform.GetChild(0);
            if (menuChild != null && menuChild.gameObject.activeSelf)
            {
                menuChild.gameObject.SetActive(false);
            }
        }
    }
    

    public void Grab()
    {
        if (objectToGrab == null)
        {
            return;
        }

        objectRb = objectToGrab.GetComponent<Rigidbody>();
        if (objectRb == null)
        {
            return;
        }

        originalParent = objectToGrab.transform.parent;
        objectRb.isKinematic = true;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Main camera not found!");
            return;
        }

        Vector3 holdPosition = cam.transform.TransformPoint(sideOffset);
        objectToGrab.transform.position = holdPosition;
        objectToGrab.transform.rotation = cam.transform.rotation;

        objectToGrab.transform.SetParent(cam.transform, true);
        isGrabbed = true;

        OnObjectGrabbed?.Invoke();
    }

    private void Update()
    {
        if (objectToGrab == null)
        {
            isGrabbed = false;
            return;
        }

        if (Input.GetButtonDown(aButton))
        {
            ReleaseObject();
        }
    }

    public void ReleaseObject()
    {
        if (objectToGrab == null)
        {
            isGrabbed = false;
            return;
        }

        if (objectRb == null)
        {
            objectRb = objectToGrab.GetComponent<Rigidbody>();
        }

        if (objectRb == null)
        {
            isGrabbed = false;
            return;
        }

        objectToGrab.transform.SetParent(null);
        
        StartCoroutine(DelayedPhysics());
        isGrabbed = false;
        OnObjectReleased?.Invoke();
    }

    private IEnumerator DelayedPhysics()
    {
        yield return new WaitForFixedUpdate();
        
        if (objectRb != null)
        {
            objectRb.isKinematic = false;
            objectRb.linearVelocity = Vector3.zero;
            objectRb.angularVelocity = Vector3.zero;

            Vector3 post = objectToGrab.transform.position;
            post.y = 0f;
            
            ApplyDropForce();
        }
    }

    private void ApplyDropForce()
    {
        if (objectRb != null)
        {
            objectRb.AddForce(Vector3.down * dropForce, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isGrabbed && objectRb != null && objectRb.linearVelocity.magnitude > 0.1f)
        {
            objectRb.linearVelocity *= velocityDamping;
        }
    }
}
