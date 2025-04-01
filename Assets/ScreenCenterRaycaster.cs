// using UnityEngine;
// using System.Collections;

// public class ScreenCenterRaycaster : MonoBehaviour
// {
//     public float rayLength = 100f;
//     public float outlineDisableDelay = 0.3f;
//     public LineRenderer lineRenderer;
//     private GameObject currentTarget;
//     private float lastHitTime;
//     private Camera mainCamera;
//     private bool isGrab = false;

//     private string aButton;    
//     private string bButton;
//     private string xButton;
    
//     void Start()
//     {
//         #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
//         {
//             bButton = "js10";
//             xButton = "js13";
//             aButton = "js11";
//         }
//         #elif UNITY_ANDROID
//         {
//             bButton = "js5";
//             xButton = "js2";
//             aButton = "js10";
//         }
//         #else
//         {
//             bButton = "js10";
//             xButton = "js2"; 
//             aButton = "js11";
//         }
//         #endif
//         mainCamera = Camera.main;
//         InitializeLineRenderer();
//     }

//     void Update()
//     {
//         Ray ray = new Ray(transform.position, transform.forward);
//         RaycastHit hit;
//         lineRenderer.SetPosition(0, transform.position);

//         bool isHit = Physics.Raycast(ray, out hit, rayLength);

//         if (isHit)
//         {
//             lineRenderer.SetPosition(1, hit.point);
//             lastHitTime = Time.time;

//             if (currentTarget != hit.collider.gameObject)
//             {
//                 currentTarget = hit.collider.gameObject;
//             }
            
//             if (hit.collider.CompareTag("InteractableObject"))
//             {
//                 SetOutline(currentTarget, true);
//                 Debug.Log("Grab target updated: " + hit.collider.gameObject.name);
//             }
//         }
//         else
//         {
//             lineRenderer.SetPosition(1, transform.position + transform.forward * rayLength);
//         }
//     }
//     private void SetOutline(GameObject target, bool enabled)
//     {
//         if (target == null) return;
        
//         Outline outline = target.GetComponent<Outline>();
//         if (outline == null)
//         {
//             outline = target.AddComponent<Outline>();
//             outline.OutlineMode = Outline.Mode.OutlineAll;
//             outline.OutlineColor = Color.green;
//             outline.OutlineWidth = 5f;
//         }
//         outline.enabled = enabled;
//     }
//     private void InitializeLineRenderer()
//     {
//         if (lineRenderer == null)
//             lineRenderer = GetComponent<LineRenderer>();
//         lineRenderer.positionCount = 2;
//         lineRenderer.startWidth = lineRenderer.endWidth = 0.02f;
//         lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
//         lineRenderer.startColor = lineRenderer.endColor = Color.red;
//         lineRenderer.material.renderQueue = 3000; 
//         lineRenderer.sortingOrder = 10;
//     }
// }

