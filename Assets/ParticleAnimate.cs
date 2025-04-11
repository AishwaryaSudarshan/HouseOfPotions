using UnityEngine;
using System.Collections; // Needed for IEnumerator and WaitForSeconds

public class DropParticleEffectTrigger : MonoBehaviour
{
    [SerializeField] private ParticleSystem dropParticleEffect;

    public void TriggerDropEffect()
    {
        if (dropParticleEffect != null)
        {
            // Ensure the particle system's GameObject is active.
            if (!dropParticleEffect.gameObject.activeSelf)
            {
                dropParticleEffect.gameObject.SetActive(true);
            }
            
            dropParticleEffect.Play();
            Debug.Log("Drop particle effect played.");
            
            // Start the coroutine to stop the effect after 5 seconds.
            StartCoroutine(StopEffectAfterDelay(5f));
        }
        else
        {
            Debug.LogWarning("No ParticleSystem assigned in the Inspector for drop effect.");
        }
    }

    private IEnumerator StopEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Stop the particle effect.
        dropParticleEffect.Stop();
        Debug.Log("Drop particle effect stopped after 5 seconds.");
    }
}