using UnityEngine;
using System.Collections.Generic;

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
        [HideInInspector]
        public bool isFixed = false; // Track whether room has been fixed
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

    private void Start()
    {
        // Initialize all rooms to "broken" state at start
        foreach (RoomData room in rooms)
        {
            UpdateRoomVisibility(room, false);
        }
    }

    // Automatically check room every interval
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
                    // Just update visibility based on current state, don't change the state
                    UpdateRoomVisibility(room, room.isFixed);
                }
                return;
            }
        }
    }

    // Call this when a potion is dropped to fix the current room
    public void ForceRoomUpdate()
    {
        foreach (RoomData room in rooms)
        {
            if (room.roomName == currentRoomName)
            {
                room.isFixed = true;
                UpdateRoomVisibility(room, true);
                Debug.Log($"Room {room.roomName} has been fixed with a potion!");
                return;
            }
        }
    }

    // Updates room visibility based on its fixed state without changing the state
    private void UpdateRoomVisibility(RoomData room, bool showFixed)
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

        broken.gameObject.SetActive(!showFixed);
        fixedRoom.gameObject.SetActive(showFixed);

        if (showFixed)
            Debug.Log($"Room {room.roomName} updated: {broken.name} disabled, {fixedRoom.name} enabled.");
        else
            Debug.Log($"Room {room.roomName} updated: {broken.name} enabled, {fixedRoom.name} disabled.");
    }
}
