using UnityEngine;

public class DropParticleEffectTrigger : MonoBehaviour
{
    [SerializeField] private ParticleSystem dropParticleEffect;
    [SerializeField] private float delayBeforeReplace = 0.5f;

    public void TriggerDropEffect()
    {
        if (dropParticleEffect != null)
        {
            dropParticleEffect.Play();
            Invoke(nameof(ReplaceRoom), delayBeforeReplace);
        }
        else
        {
            Debug.LogWarning("No ParticleSystem assigned in the Inspector for drop effect.");
        }
    }

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
    }
}
