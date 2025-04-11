using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public string roomName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RoomManager.Instance.SetCurrentRoom(roomName);
        }
    }
}
