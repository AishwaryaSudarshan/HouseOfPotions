using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    [Tooltip("Which tag to search for when looking for objects?")]
    public string objectTag = "Interactable"; 

    [Tooltip("Reference to the camera or player transform.")]
    public Transform playerTransform;

    public void ShowHint()
    {
        GameObject closestObj = FindClosestObject(objectTag);
        if (closestObj != null)
        {
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
        Debug.Log("Highlighting object: " + target.name);
    }
}
