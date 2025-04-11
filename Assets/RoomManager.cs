using UnityEngine;

public class RoomManager : MonoBehaviour
{
    // Your RoomData implementation and array of rooms
    [System.Serializable]
    public class RoomData
    {
        public string roomName;
        public Vector3 center;
        public float detectionRadius = 5f;
        public GameObject roomParent;
    }

    public RoomData[] rooms;
    public Transform player;

    [HideInInspector] public string currentRoomName;
    public float checkInterval = 0.2f;
    private float nextCheckTime;

    private static RoomManager _instance;
    public static RoomManager Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    // Automatically check room every interval (optional)
    private void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            CheckPlayerRoom();
            nextCheckTime = Time.time + checkInterval;
        }
    }

    // Checks which room the player is in by comparing distances.
    private void CheckPlayerRoom()
    {
        if (player == null)
        {
            Debug.LogWarning("Player transform not assigned to RoomManager.");
            return;
        }

        foreach (RoomData room in rooms)
        {
            if (Vector3.Distance(player.position, room.center) <= room.detectionRadius)
            {
                if (currentRoomName != room.roomName)
                {
                    currentRoomName = room.roomName;
                    UpdateRoomScene(room);
                }
                return;
            }
        }
    }

    // Call this to force a check now.
    public void ForceRoomUpdate()
    {
        CheckPlayerRoom();
    }

    // Updates the room scene by disabling the broken version and enabling the fixed version.
    private void UpdateRoomScene(RoomData room)
    {
        if (room.roomParent == null)
        {
            Debug.LogWarning("Room parent not assigned for room: " + room.roomName);
            return;
        }

        Transform broken = room.roomParent.transform.Find("Broken" + room.roomName);
        Transform fixedRoom = room.roomParent.transform.Find("Fixed" + room.roomName);

        if (broken == null || fixedRoom == null)
        {
            Debug.LogWarning($"Room objects not found for {room.roomName}.");
            return;
        }

        broken.gameObject.SetActive(false);
        fixedRoom.gameObject.SetActive(true);
        Debug.Log($"Room {room.roomName} updated: {broken.name} disabled, {fixedRoom.name} enabled.");
    }
}
