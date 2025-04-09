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
            if (pourPotions != null)
            {
                pourPotions.SetActive(true);
            }

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
}
