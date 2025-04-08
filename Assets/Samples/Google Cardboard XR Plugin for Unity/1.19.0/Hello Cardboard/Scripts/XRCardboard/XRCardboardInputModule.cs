using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if !UNITY_EDITOR
using UnityEngine.XR;
#endif

public class XRCardboardInputModule : PointerInputModule
{
    [SerializeField]
    private XRCardboardInputSettings settings = default;
    [SerializeField]
    private UnityFloatEvent onStartHover = default;
    [SerializeField]
    private UnityEvent onEndHover = default;
    [SerializeField]
    private UnityEvent onClick = default;

    private PointerEventData pointerEventData;
    private GameObject currentTarget;
    private float currentTargetClickTime = float.MaxValue;
    private bool hovering;

    public override void Process()
    {
        HandleLook();
        HandleSelection();
    }

    private void HandleLook()
    {
        pointerEventData ??= new PointerEventData(eventSystem);
#if UNITY_EDITOR
        pointerEventData.position = new Vector2(Screen.width / 2, Screen.height / 2);
#else
        pointerEventData.position = new Vector2(XRSettings.eyeTextureWidth / 2, XRSettings.eyeTextureHeight / 2);
#endif
        pointerEventData.delta = Vector2.zero;
        List<RaycastResult> raycastResults = new();
        eventSystem.RaycastAll(pointerEventData, raycastResults);
        raycastResults = raycastResults.OrderBy(r => !r.module.GetComponent<GraphicRaycaster>()).ToList();
        pointerEventData.pointerCurrentRaycast = FindFirstRaycast(raycastResults);
        ProcessMove(pointerEventData);
    }

    private void HandleSelection()
    {
        GameObject handler;
        try
        {
            handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(pointerEventData.pointerEnter);
            Selectable selectable = handler.GetComponent<Selectable>();
            if (selectable && selectable.interactable == false)
            {
                throw new NullReferenceException();
            }
        }
        catch (NullReferenceException)
        {
            currentTarget = null;
            StopHovering();
            return;
        }

        if (currentTarget != handler)
        {
            float gazeTime = settings.GazeTime;
            currentTarget = handler;
            currentTargetClickTime = Time.realtimeSinceStartup + gazeTime;
            if (hovering)
            {
                StopHovering();
            }

            hovering = true;
            onStartHover?.Invoke(gazeTime);
        }

        if ((Time.realtimeSinceStartup > currentTargetClickTime && settings.ClickOnHover) || Input.GetButtonDown(settings.ClickInput))
        {
            _ = ExecuteEvents.ExecuteHierarchy(currentTarget, pointerEventData, ExecuteEvents.pointerClickHandler);
            currentTargetClickTime = float.MaxValue;
            onClick?.Invoke();
            StopHovering();
        }
    }

    private void StopHovering()
    {
        if (!hovering)
        {
            return;
        }

        hovering = false;
        onEndHover?.Invoke();
    }
}