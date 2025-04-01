using UnityEngine;
using System.Collections;

public class ScreenCenterRaycaster : MonoBehaviour
{
    public float rayLength = 100f;
    public float outlineDisableDelay = 0.3f;
    public LineRenderer lineRenderer;
    private Coroutine resetCoroutine;
    private GameObject currentTarget;
    private float lastHitTime;
    private Camera mainCamera;
    private bool isGrab = false;

    private GrabAction grabAction;
    private StoreAction storeAction;

    private string aButton;    
    private string bButton;
    private string xButton;
    [SerializeField] public GameObject grabMgr;
    [SerializeField] public GameObject storeMgr;
    
    void Start()
    {
        #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        {
            bButton = "js10";
            xButton = "js13";
            aButton = "js11";
        }
        #elif UNITY_ANDROID
        {
            bButton = "js5";
            xButton = "js2";
            aButton = "js10";
        }
        #else
        {
            bButton = "js10";
            xButton = "js2"; 
            aButton = "js11";
        }
        #endif
        mainCamera = Camera.main;
        InitializeLineRenderer();
        grabAction = grabMgr.GetComponent<GrabAction>();
        storeAction = storeMgr.GetComponent<StoreAction>();
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        lineRenderer.SetPosition(0, transform.position);

        bool isHit = Physics.Raycast(ray, out hit, rayLength);

        if (isHit)
        {
            lineRenderer.SetPosition(1, hit.point);
            lastHitTime = Time.time;

            if (currentTarget != hit.collider.gameObject)
            {
                ClearCurrentOutline();
                currentTarget = hit.collider.gameObject;
            }
            
            if (hit.collider.CompareTag("TargetTag"))
            {
                SetOutline(currentTarget, true);
                Debug.Log("Grab target updated: " + hit.collider.gameObject.name);
            }
            
            if (hit.collider.CompareTag("GrabButton") ||
                hit.collider.CompareTag("StoreButton") ||
                hit.collider.CompareTag("ExitButton"))
            {
                // ButtonHoverEffect hoverEffect = hit.collider.gameObject.GetComponent<ButtonHoverEffect>();
                // if (hoverEffect != null)
                // {
                //     hoverEffect.SetHover(true);
                //     Debug.Log("ButtonHoverEffect executed: Hover activated for " + hit.collider.gameObject.name);
                // }
                // else
                // {
                //     Debug.LogWarning("ButtonHoverEffect component not found on " + hit.collider.gameObject.name);
                // }
            }
            
            if (Input.GetButtonDown(bButton))
            {
                if (hit.collider.CompareTag("GrabButton"))
                {
                    grabAction.objectToGrab = hit.collider.gameObject.transform.parent.gameObject.transform.parent.gameObject.transform.parent.gameObject;
                    if (grabAction != null)
                    {
                        grabAction.ExecuteAction();
                        isGrab = true;
                    }
                    else
                    {
                        Debug.Log("GrabAction component not found on " + hit.collider.gameObject.name);
                    }
                }
                else if (hit.collider.CompareTag("StoreButton"))
                {
                    storeAction.objectToStore = hit.collider.gameObject.transform.parent.gameObject.transform.parent.gameObject.transform.parent.gameObject;
                    if (storeAction != null)
                    {
                        storeAction.Store();
                    }
                    else
                        Debug.LogWarning("StoreAction component not found on " + hit.collider.gameObject.name);
                }
                // else if (hit.collider.CompareTag("ExitButton"))
                // {
                //     ExitAction exitAction = hit.collider.gameObject.GetComponent<ExitAction>();
                //     if (exitAction != null)
                //         exitAction.ExecuteAction();
                //     else
                //         Debug.LogWarning("ExitAction component not found on " + hit.collider.gameObject.name);
                // }
            }

            if (Input.GetButtonDown(xButton))
            {
                if (hit.collider.CompareTag("TargetTag"))
                {
                    HideAllMenus();
                    if (hit.collider.transform.childCount > 0)
                    {
                        Transform menuChild = hit.collider.transform.GetChild(0);
                        menuChild.gameObject.SetActive(true);
                    }
                }
            }
            
            if (isGrab && Input.GetButtonDown(aButton))
            {
                grabAction.ReleaseObject();
                isGrab = false;
            }
        }
        else
        {
            lineRenderer.SetPosition(1, transform.position + transform.forward * rayLength);
            if (currentTarget != null && Time.time - lastHitTime > outlineDisableDelay)
            {
                ClearCurrentOutlineDelayed();
            }
        }
    }
    private void SetOutline(GameObject target, bool enabled)
    {
        if (target == null) return;
        
        Outline outline = target.GetComponent<Outline>();
        if (outline == null)
        {
            outline = target.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = Color.green;
            outline.OutlineWidth = 5f;
        }
        outline.enabled = enabled;
    }
    private void ClearCurrentOutline()
    {
        if (currentTarget != null)
        {
            Outline outline = currentTarget.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false;
            }
            
            // ButtonHoverEffect hoverEffect = currentTarget.GetComponent<ButtonHoverEffect>();
            // if (hoverEffect != null)
            // {
            //     hoverEffect.SetHover(false);
            // }
            
            currentTarget = null;
        }
    }
    private void InitializeLineRenderer()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineRenderer.endColor = Color.red;
        lineRenderer.material.renderQueue = 3000; 
    }

    void OnDestroy()
    {
        ClearCurrentOutline();
    }

    private void ClearCurrentOutlineDelayed()
    {
        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);

        resetCoroutine = StartCoroutine(DelayedClear());
    }

    private IEnumerator DelayedClear()
    {
        yield return new WaitForSeconds(outlineDisableDelay);
        ClearCurrentOutline();
    }

    private void HideAllMenus()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("TargetTag");
        foreach (GameObject target in targets)
        {
            if (target.transform.childCount > 0)
            {
                target.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}

