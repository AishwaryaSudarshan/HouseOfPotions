using UnityEngine;
using System.Collections;

public class DropParticleEffectTrigger : MonoBehaviour
{
    [SerializeField] private ParticleSystem dropParticleEffect;

    public void TriggerDropEffect()
    {
        if (dropParticleEffect != null)
        {
            if (!dropParticleEffect.gameObject.activeSelf)
            {
                dropParticleEffect.gameObject.SetActive(true);
            }
            
            dropParticleEffect.Play();
            Debug.Log("Drop particle effect played.");
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
        dropParticleEffect.Stop();
        Debug.Log("Drop particle effect stopped after 5 seconds.");
    }
}