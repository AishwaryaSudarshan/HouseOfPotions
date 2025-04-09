
using UnityEngine;
public class RaycastSelector : MonoBehaviour
{
    public float rayLength = 100f;
    public Color outlineColor = Color.red;
    public float outlineWidth = 5f;
    public LineRenderer lineRenderer;
    public Vector3 rayOriginOffset = new Vector3(0, 0, 0.1f);

    private Camera mainCamera;
    private GameObject currentHighlightedObject;
    private Outline currentOutline;

    public Ray CurrentRay { get; private set; }

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
            if (hit.collider.CompareTag("InteractableObject"))
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
            }
            else if(hit.collider.CompareTag("Pot"))
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
            }
            else
            {
                if (currentHighlightedObject != null && currentOutline != null)
                {
                    currentOutline.OutlineColor = Color.red;
                    currentHighlightedObject = null;
                }
            }
        }
        else
        {
            lineRenderer.SetPosition(1, ray.origin + (ray.direction * rayLength));
        }
    }
}


