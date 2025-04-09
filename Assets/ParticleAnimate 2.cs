using UnityEngine;

public class DropParticleEffectTrigger : MonoBehaviour
{
    // Reference to the Particle System component that should play.
    // Drag your Particle System GameObject (or its ParticleSystem component)
    // from the Hierarchy into this field in the Inspector.
    [SerializeField] private ParticleSystem dropParticleEffect;

    // This method triggers the particle effect when called.
    public void TriggerDropEffect()
    {
        if (dropParticleEffect != null)
        {
            dropParticleEffect.Play();
        }
        else
        {
            Debug.LogWarning("No ParticleSystem assigned in the Inspector for drop effect.");
        }
    }
}
