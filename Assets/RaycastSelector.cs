
using UnityEngine;
public class RaycastSelector : MonoBehaviour
{
    public float rayLength = 10f;
    public Color outlineColor = Color.red;
    public float outlineWidth = 5f;
    public LineRenderer lineRenderer;
    public Vector3 rayOriginOffset = new Vector3(0, 0, 0.1f);

    private Camera mainCamera;
    private GameObject currentHighlightedObject;
    private Outline currentOutline;
    private DropParticleEffectTrigger dropParticleTrigger;

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
        dropParticleTrigger = mainCamera.GetComponent<DropParticleEffectTrigger>();
        if (dropParticleTrigger == null)
        {
            Debug.LogWarning("No DropParticleEffectTrigger component found on the main camera.");
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


