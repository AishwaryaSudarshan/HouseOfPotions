using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;  // For TextMesh Pro

public class IngredientPot : MonoBehaviour
{
    [Header("Required Items for Recipe")]
    public List<GameObject> requiredIngredients = new List<GameObject>();

    [Header("Pot Settings")]
    public ParticleSystem potionParticles;
    public TMP_Text completionText;  // Assign a TextMeshProUGUI object in the Inspector
    public GameObject pourPotions;

    public HashSet<GameObject> addedIngredients = new HashSet<GameObject>();

    private void Start()
    {
        if (completionText != null)
        {
            completionText.gameObject.transform.parent.gameObject.SetActive(false);
        }
        if (potionParticles != null)
        {
            potionParticles.Stop();
        }
        if (pourPotions != null)
        {
            pourPotions.SetActive(false);
        }
    }

    public void AddIngredient(GameObject ingredient)
    {
        addedIngredients.Add(ingredient);

        if (potionParticles != null && !potionParticles.isPlaying)
        {
            potionParticles.Play();
        }

        if (IsRecipeComplete())
        {
            if (completionText != null)
            {
                completionText.gameObject.transform.parent.gameObject.SetActive(true);
                completionText.text = "POTION COMPLETE!";
                // Start the coroutine to hide the message after 3 seconds
                StartCoroutine(HideCompletionMessage());
            }

            if (pourPotions != null)
            {
                pourPotions.SetActive(true);
            }
        }
    }

    private bool IsRecipeComplete()
    {
        foreach (GameObject required in requiredIngredients)
        {
            if (!addedIngredients.Contains(required))
            {
                return false;
            }
        }
        return true;
    }

    // Coroutine to hide the completion message after 3 seconds
    private IEnumerator HideCompletionMessage()
    {
        yield return new WaitForSeconds(3f);
        if (completionText != null)
        {
            completionText.gameObject.transform.parent.gameObject.SetActive(false);
        }
    }
}
