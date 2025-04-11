// using UnityEngine;

// public class DropParticleEffectTrigger : MonoBehaviour
// {
//     [SerializeField] private ParticleSystem dropParticleEffect;

//     public void TriggerDropEffect()
//     {
//         if (dropParticleEffect != null)
//         {
//             // Ensure the particle system's GameObject is active.
//             if (!dropParticleEffect.gameObject.activeSelf)
//             {
//                 dropParticleEffect.gameObject.SetActive(true);
//             }
            
//             dropParticleEffect.Play();
//             Debug.Log("Drop particle effect played.");
//         }
//         else
//         {
//             Debug.LogWarning("No ParticleSystem assigned in the Inspector for drop effect.");
//         }
//     }
// }

using UnityEngine;
using System.Collections; // Needed for IEnumerator and WaitForSeconds

public class DropParticleEffectTrigger : MonoBehaviour
{
    [SerializeField] private ParticleSystem dropParticleEffect;
    [SerializeField] private float delayBeforeReplace = 0.5f;

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
<<<<<<< HEAD
            Debug.Log("Drop particle effect played.");
            
            // Start the coroutine to stop the effect after 5 seconds.
            StartCoroutine(StopEffectAfterDelay(5f));
=======
            Invoke(nameof(ReplaceRoom), delayBeforeReplace);
>>>>>>> b0fa27aea9728d17501ca0e505c0eb4e9ac2ac39
        }
        else
        {
            Debug.LogWarning("No ParticleSystem assigned in the Inspector for drop effect.");
        }
    }

<<<<<<< HEAD
    private IEnumerator StopEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Stop the particle effect.
        dropParticleEffect.Stop();
        Debug.Log("Drop particle effect stopped after 5 seconds.");
=======
    private void ReplaceRoom()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.ReplaceRoom(); 
        }
        else
        {
            Debug.LogWarning("RoomManager.Instance is null.");
        }
>>>>>>> b0fa27aea9728d17501ca0e505c0eb4e9ac2ac39
    }
}
