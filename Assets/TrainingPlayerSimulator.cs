using UnityEngine;
using System.Collections; // Needed for IEnumerator if using timed hiding

public class TrainingPlayerSimulator : MonoBehaviour
{
    [Header("Movement")]
    public Transform[] movementWaypoints; // Assign empty GameObjects placed on NavMesh
    public float playerSpeed = 3.0f;
    public float waypointReachedThreshold = 1.5f; // How close to get before picking next point

    [Header("Hiding")]
    public float chanceToHidePerSecond = 0.1f; // 10% chance per second to try hiding
    public float minHideDuration = 3.0f;
    public float maxHideDuration = 8.0f;
    public PlayerHiding playerHidingScript; // Assign your PlayerHiding script component in Inspector

    [Header("References")]
    public CharacterController characterController; // Assign CharacterController in Inspector (or get in Start)
    public HideAndSeekNPC npcAgent; // Assign NPC Agent in Inspector

    private Vector3 currentTargetPosition;
    private bool isMoving = false;
    private bool isSimHiding = false;
    private Coroutine hidingCoroutine = null;
    private int currentWaypointIndex = -1;

    void Start()
    {
        // Get components if not assigned
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (playerHidingScript == null) playerHidingScript = GetComponent<PlayerHiding>();

        if (movementWaypoints == null || movementWaypoints.Length == 0)
        {
            Debug.LogError("TrainingPlayerSimulator: Movement Waypoints are not assigned!");
            isMoving = false; // Can't move without waypoints
        }

        if (playerHidingScript == null)
        {
            Debug.LogError("TrainingPlayerSimulator: PlayerHiding script reference not set!");
        }

        // Register this simulator with the NPC agent so it can call ResetPlayer
        if (npcAgent != null)
        {
            npcAgent.RegisterPlayerSimulator(this);
        }
        else
        {
            Debug.LogError("TrainingPlayerSimulator: NPC Agent reference not set! Player won't reset automatically.");
        }

        // Initial state setup moved to ResetPlayer()
        // PickNewWaypoint(); // Now called by ResetPlayer
    }

    // This method will be called by the NPC's OnEpisodeBegin
    public void ResetPlayer()
    {
        Debug.Log("Training Player Resetting...");
        // Stop any current hiding
        if (hidingCoroutine != null)
        {
            StopCoroutine(hidingCoroutine);
            hidingCoroutine = null;
        }
        if (isSimHiding)
        {
            StopSimulatedHiding();
        }

        // Pick a random starting waypoint
        if (movementWaypoints != null && movementWaypoints.Length > 0)
        {
            int randomIndex = Random.Range(0, movementWaypoints.Length);
            // IMPORTANT: Teleport player using CharacterController's method if active
            if (characterController != null && characterController.enabled)
            {
                characterController.enabled = false; // Disable CC temporarily for teleport
                transform.position = movementWaypoints[randomIndex].position;
                transform.rotation = movementWaypoints[randomIndex].rotation; // Optional: Reset rotation
                characterController.enabled = true; // Re-enable
            }
            else
            {
                transform.position = movementWaypoints[randomIndex].position; // Fallback if no CC
                transform.rotation = movementWaypoints[randomIndex].rotation;
            }

            Debug.Log($"Player reset to waypoint {randomIndex} at {transform.position}");

            // Pick a new destination different from start
            PickNewWaypoint();
            isMoving = true;
        }
        else
        {
            isMoving = false; // Cannot move if no waypoints
        }
    }


    void Update()
    {
        if (isMoving && !isSimHiding)
        {
            MoveTowardsTarget();
            // Check chance to hide periodically
            if (Random.value < chanceToHidePerSecond * Time.deltaTime)
            {
                TryStartSimulatedHiding();
            }
        }
    }

    void PickNewWaypoint()
    {
        if (movementWaypoints == null || movementWaypoints.Length == 0) return;

        int newIndex = currentWaypointIndex;
        // Ensure we pick a *different* waypoint if possible
        if (movementWaypoints.Length > 1)
        {
            while (newIndex == currentWaypointIndex)
            {
                newIndex = Random.Range(0, movementWaypoints.Length);
            }
        }
        else
        {
            newIndex = 0; // Only one point
        }

        currentWaypointIndex = newIndex;
        currentTargetPosition = movementWaypoints[currentWaypointIndex].position;
        Debug.Log($"Player heading towards waypoint {currentWaypointIndex}");
    }

    void MoveTowardsTarget()
    {
        if (Vector3.Distance(transform.position, currentTargetPosition) < waypointReachedThreshold)
        {
            // Reached destination, pick a new one
            PickNewWaypoint();
        }
        else
        {
            // Move towards target
            Vector3 moveDirection = (currentTargetPosition - transform.position).normalized;
            // Set Y component to 0 to prevent flying if not using vertical waypoints
            moveDirection.y = 0;

            if (characterController != null && characterController.enabled)
            {
                // Apply gravity manually if needed or use SimpleMove which includes it
                // Add gravity if not using SimpleMove: moveDirection += Physics.gravity * Time.deltaTime;
                characterController.SimpleMove(moveDirection * playerSpeed); // SimpleMove applies gravity
                                                                             // OR characterController.Move(moveDirection * playerSpeed * Time.deltaTime); // Use Move if you handle gravity separately
            }
        }
    }

    void TryStartSimulatedHiding()
    {
        if (!isSimHiding && playerHidingScript != null)
        {
            isSimHiding = true;
            isMoving = false; // Stop moving while hiding
            playerHidingScript.SetPlayerHidingStatus(true); // Tell the hiding script
            Debug.Log("Simulator: Player starting hide.");

            float hideDuration = Random.Range(minHideDuration, maxHideDuration);
            hidingCoroutine = StartCoroutine(HidingTimer(hideDuration));
        }
    }

    IEnumerator HidingTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopSimulatedHiding();
    }

    void StopSimulatedHiding()
    {
        if (isSimHiding && playerHidingScript != null)
        {
            isSimHiding = false;
            playerHidingScript.SetPlayerHidingStatus(false); // Tell the hiding script
            Debug.Log("Simulator: Player stopping hide.");
            isMoving = true; // Resume moving
            PickNewWaypoint(); // Pick a new place to go after hiding
            hidingCoroutine = null;
        }
    }
}