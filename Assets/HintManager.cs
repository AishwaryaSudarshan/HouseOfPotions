using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Finds the closest object with a specified tag and does something (highlight, UI, etc.)
/// Attach this to a manager object or the same Main Camera.
/// </summary>
public class HintManager : MonoBehaviour
{
    [Tooltip("Which tag to search for when looking for objects?")]
    public string objectTag = "Interactable";  // or "PotionObject" or whatever your objects have

    [Tooltip("Reference to the camera or player transform.")]
    public Transform playerTransform;

    // Suppose we have a method that gets called when a head shake is detected:
    public void ShowHint()
    {
        GameObject closestObj = FindClosestObject(objectTag);
        if (closestObj != null)
        {
            // Example: highlight it, or show a UI marker
            Debug.Log("Closest object is " + closestObj.name);
            Highlight(closestObj);
        }
        else
        {
            Debug.Log("No objects found with tag: " + objectTag);
        }
    }

    private GameObject FindClosestObject(string tag)
    {
        GameObject[] candidates = GameObject.FindGameObjectsWithTag(tag);
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        if (playerTransform == null)
        {
            Debug.LogWarning("Player Transform not assigned to HintManager!");
            return null;
        }

        Vector3 currentPos = playerTransform.position;
        foreach (GameObject obj in candidates)
        {
            float dist = Vector3.Distance(obj.transform.position, currentPos);
            if (dist < minDist)
            {
                closest = obj;
                minDist = dist;
            }
        }
        return closest;
    }

    private void Highlight(GameObject target)
    {
        // This is an example method. 
        // You could create an outline, change color, or show an arrow pointing at it.
        // For demonstration, just log:
        Debug.Log("Highlighting object: " + target.name);
        // If you had a highlight component or UI element, you'd enable it here.
    }
}
