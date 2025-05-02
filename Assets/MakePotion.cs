using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IngredientPot : MonoBehaviour
{
    [Header("Required Items for Recipe")]
    public List<GameObject> requiredIngredients = new List<GameObject>();

    [Header("Pot Settings")]
    public ParticleSystem potionParticles;
    public GameObject pourPotions;  
    public ParticleSystem particleDust1;
    public ParticleSystem particleDust2;
    public GameObject particleContainer;
    public HashSet<GameObject> addedIngredients = new HashSet<GameObject>();
    private bool recipeComplete = false;
    private bool isMixed = false;
    
    [Header("UI Elements")]
    public GameObject instructionTextObject; // GameObject containing the TMP_Text component

    private void Start()
    {
        if (potionParticles != null)
        {
            potionParticles.Stop();
        }
        if (pourPotions != null)
        {
            pourPotions.SetActive(false);
        }
        if (particleContainer != null)
        {
            particleContainer.SetActive(false);
        }
        else
        {
            if (particleDust1 != null)
            {
                particleDust1.Stop();
            }
            if (particleDust2 != null)
            {
                particleDust2.Stop();
            }
        }
        
        if (instructionTextObject != null)
        {
            instructionTextObject.SetActive(false);
        }
    }
    private void Update()
    {
        if (Input.GetButtonDown("js11"))
        {
            GetClosestRequiredIngredient();
        }
    }
    public GameObject GetClosestRequiredIngredient()
    {
        if (requiredIngredients.Count == 0)
        {
            Debug.Log("No required ingredients in the list.");
            return null;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not found.");
            return null;
        }

        float minDistance = float.MaxValue;
        GameObject closestIngredient = null;

        foreach (GameObject ingredient in requiredIngredients)
        {
            if (ingredient == null || addedIngredients.Contains(ingredient) || !ingredient.activeInHierarchy)
            {
                continue;
            }

            float distance = Vector3.Distance(mainCamera.transform.position, ingredient.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIngredient = ingredient;
            }
        }

        if (closestIngredient != null)
        {
            return closestIngredient;
        }
        else
        {
            Debug.Log("None of the required ingredients are available.");
            return null;
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
            Debug.Log("All ingredients added. Ready to mix the potion with a triangle gesture!");
            recipeComplete = true;
            ShowInstructionText("Shake your head LEFT and RIGHT really fast to mix the potion!");
        }
    }
    private bool IsRecipeComplete()
    {
        foreach (GameObject required in requiredIngredients)
        {
            bool found = false;
            foreach (GameObject added in addedIngredients)
            {
                if (added.name.Contains(required.name))
                {
                    found = true;
                    break;
                }
            }
            if (!found) return false;
        }
        return true;
    }
    public void MixPotion()
    {
        if (!recipeComplete)
        {
            Debug.Log("Cannot mix potion: not all ingredients have been added.");
            return;
        }
        if (isMixed)
        {
            Debug.Log("Potion already mixed.");
            return;
        }
        
        Debug.Log("Mixing the potion (triggered by triangle gesture)!");
        if (particleContainer != null)
        {
            particleContainer.SetActive(true);
        }
        if (particleDust1 != null && !particleDust1.isPlaying)
        {
            particleDust1.Play();
        }
        if (particleDust2 != null && !particleDust2.isPlaying)
        {
            particleDust2.Play();
        }
        isMixed = true;
        StartCoroutine(StopAnimation());
    }
    public void ReleasePotion()
    {
        if (!isMixed)
        {
            Debug.Log("Cannot release potion: potion is not mixed yet.");
            return;
        }
        
        Debug.Log("Releasing the potion (triggered by nod gesture)!");
    
        if (pourPotions != null)
        {
            pourPotions.SetActive(true);
        }
    }
    private IEnumerator StopAnimation()
    {
        yield return new WaitForSeconds(5f);

        if (potionParticles != null && potionParticles.isPlaying)
        {
            potionParticles.Stop();
        }
        if (particleContainer != null)
        {
            particleContainer.SetActive(false);
        }
        if (particleDust1 != null && particleDust1.isPlaying)
        {
            particleDust1.Stop();
        }
        if (particleDust2 != null && particleDust2.isPlaying)
        {
            particleDust2.Stop();
        }
    }
    
    private void ShowInstructionText(string message)
    {
        if (instructionTextObject != null)
        {
            TMP_Text textComponent = instructionTextObject.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = message;
            }
            instructionTextObject.SetActive(true);
            StartCoroutine(HideInstructionTextAfterDelay(5f));
        }
    }
    
    private IEnumerator HideInstructionTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (instructionTextObject != null)
        {
            instructionTextObject.SetActive(false);
        }
    }
}
