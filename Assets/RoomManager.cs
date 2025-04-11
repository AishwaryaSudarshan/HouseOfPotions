using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;
    [HideInInspector] public string currentRoomName;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetCurrentRoom(string roomName)
    {
        currentRoomName = roomName;
        Debug.Log("Current room: " + currentRoomName);
    }

    public void ReplaceRoom()
    {
        if (string.IsNullOrEmpty(currentRoomName)) return;

        GameObject roomParent = GameObject.Find(currentRoomName);
        if (roomParent == null)
        {
            Debug.LogWarning("Room parent not found: " + currentRoomName);
            return;
        }

        Transform broken = roomParent.transform.Find("Broken" + currentRoomName);
        Transform fixedRoom = roomParent.transform.Find("Fixed" + currentRoomName);

        if (broken != null && fixedRoom != null)
        {
            broken.gameObject.SetActive(false);
            fixedRoom.gameObject.SetActive(true);
        }
    }
}
