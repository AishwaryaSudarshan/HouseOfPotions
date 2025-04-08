using System.Collections.Generic;
using UnityEngine;
using TMPro;  // For TextMeshPro

public class IngredientPot : MonoBehaviour
{
    [Header("Required Items for Recipe")]
    // List the required ingredient GameObjects in the Inspector (or assign via code)
    public List<GameObject> requiredIngredients = new List<GameObject>();

    [Header("Pot Settings")]
    public ParticleSystem potionParticles;
    public TMP_Text completionText;  // Assign a TextMeshProUGUI object in the Inspector

    // Internally track the GameObjects that have been placed in the pot
    public HashSet<GameObject> addedIngredients = new HashSet<GameObject>();

    private void Start()
    {
        // Ensure the completion text is initially hidden
        if (completionText != null)
        {
            completionText.gameObject.SetActive(false);
        }
        
        // Optionally, stop the particle system if it is playing on Start
        if (potionParticles != null)
        {
            potionParticles.Stop();
        }
    }

    // Updated AddIngredient method that now takes a GameObject parameter
    public void AddIngredient(GameObject ingredient)
    {
        // Add the ingredient GameObject to the HashSet (duplicates are automatically ignored)
        addedIngredients.Add(ingredient);

        // Play the particle system once an ingredient is added
        if (potionParticles != null && !potionParticles.isPlaying)
        {
            potionParticles.Play();
        }

        // Check if the recipe is complete
        if (IsRecipeComplete())
        {
            if (completionText != null)
            {
                completionText.gameObject.SetActive(true);
                completionText.text = "POTION COMPLETE!";
            }
            // Optionally, trigger additional effects here
        }
    }

    private bool IsRecipeComplete()
    {
        // The recipe is complete if every required ingredient is present in the addedIngredients set.
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
