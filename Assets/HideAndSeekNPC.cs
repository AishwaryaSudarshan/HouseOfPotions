using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.AI;
using System.Linq;
using System.Collections.Generic; // Need this for NavMeshAgent

public class HideAndSeekNPC : Agent
{
    public Transform playerTransform; // Assign your Player object (XRCardboardRig) in the Inspector
    public float detectionDistance = 10f;
    public float loseSightDistance = 15f; // Distance beyond which the agent might lose the player
    public LayerMask obstacleLayer; // Set this in inspector to layers that block vision (walls, furniture)
    public float patrolSpeed = 2.0f;
    public float chaseSpeed = 4.0f;
    public Transform[] patrolPoints; // Optional: Assign empty GameObjects as patrol points

    private NavMeshAgent navMeshAgent;
    private Vector3 lastKnownPlayerPosition;
    private bool knowsPlayerLocation = false;
    private int currentPatrolIndex = 0;

    // Player Hiding State Reference (needs to be set by your player script)
    public bool IsPlayerHidden { get; set; } = false;

    private List<int> visitedPatrolPoints = new List<int>();

    public TrainingPlayerSimulator playerSimulator; // Assign in Inspector

    public void RegisterPlayerSimulator(TrainingPlayerSimulator simulator)
    {
        playerSimulator = simulator;
        Debug.Log("NPC registered player simulator.");
    }

    public override void Initialize()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (playerTransform == null)
        {
            // Try to find player automatically if not assigned
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); // Make sure your player has the "Player" tag
            if (playerObj != null) playerTransform = playerObj.transform;
            else Debug.LogError("Player Transform not found or assigned on NPC Agent!");
        }
        navMeshAgent.speed = patrolSpeed;
    }

    public override void OnEpisodeBegin()
    {
        // --- ADD THIS AT THE VERY BEGINNING ---
        // Reset the player's position and state via the simulator script
        if (playerSimulator != null)
        {
            playerSimulator.ResetPlayer();
        }
        else
        {
            // Warn if training likely won't be dynamic
            Debug.LogWarning("Player Simulator not assigned to NPC. Player position will not reset between episodes.");
        }
        // --- END OF ADDED CODE ---


        // Existing NPC reset logic:
        // transform.localPosition = new Vector3(0, 0, 0); // Reset NPC position if needed
        knowsPlayerLocation = false;
        navMeshAgent.speed = patrolSpeed;
        // Reset NPC destination
        if (navMeshAgent.isOnNavMesh) // Check if agent is on mesh before setting destination
        {
            navMeshAgent.ResetPath(); // Clear current path
        }
        GoToNextPatrolPoint(); // Start patrolling
        Debug.Log("NPC Episode Begin.");
    }

    // How the Agent Perceives the Environment
    public override void CollectObservations(VectorSensor sensor)
    {
        // --- Observation Vector Size = 1 (CanSeePlayer) + 1 (KnowsLocation) + 3 (RelativePlayerPos) + 3 (NPC Velocity) = 8 ---
        // Note: RayPerceptionSensor adds its own observations automatically if attached! Adjust size accordingly.

        bool canSeePlayer = CanSeePlayer();

        // 1. Can the agent currently see the player? (1 observation)
        sensor.AddObservation(canSeePlayer);

        // 2. Does the agent "remember" where the player might be? (1 observation)
        sensor.AddObservation(knowsPlayerLocation);

        // 3. Relative position to player (if known/seen), otherwise relative pos to last known or target (3 observations)
        Vector3 targetDirection = Vector3.zero;
        if (canSeePlayer)
        {
            targetDirection = (playerTransform.position - transform.position).normalized;
            lastKnownPlayerPosition = playerTransform.position; // Update last known position
            knowsPlayerLocation = true;
        }
        else if (knowsPlayerLocation) // If lost sight, head towards last known pos
        {
            targetDirection = (lastKnownPlayerPosition - transform.position).normalized;
            // Check if reached last known position area
            if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1.5f)
            {
                knowsPlayerLocation = false; // Arrived, lost the player
            }
        }
        else // Patrolling, direction to next patrol point
        {
            if (patrolPoints.Length > 0 && navMeshAgent.hasPath)
            {
                targetDirection = (navMeshAgent.steeringTarget - transform.position).normalized;
            }
        }
        sensor.AddObservation(transform.InverseTransformDirection(targetDirection)); // Local direction

        // 4. Agent's current velocity (normalized) (3 Observations)
        sensor.AddObservation(navMeshAgent.velocity.normalized);


        // --- Update Behavior Parameters ---
        // In the Inspector on the NPC > Behavior Parameters component:
        // Space Size: Update this based on your total observations.
        // If using RayPerceptionSensor3D: Its observations count + 8 (from above)
        // If *not* using RaySensor: Just 8 (but detection becomes harder)
    }


    // What the Agent Does based on Decisions
    public override void OnActionReceived(ActionBuffers actions)
    {
        // --- Actions ---
        // We'll use Discrete Actions for simplicity with NavMeshAgent
        // Action 0: Continue current NavMesh path (patrol or towards last known position)
        // Action 1: Chase Player (Set destination to current player position)

        int action = actions.DiscreteActions[0];
        bool canSeePlayer = CanSeePlayer(); // Check again based on current state

        // --- Reward Logic ---
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (canSeePlayer)
        {
            // Positive reward for seeing the player
            AddReward(0.01f);
            navMeshAgent.speed = chaseSpeed;
            navMeshAgent.SetDestination(playerTransform.position); // Action 1: Chase!

            // Caught Player Logic:
            if (distanceToPlayer < 1.5f) // Adjust catch distance as needed
            {
                Debug.Log("NPC Caught Player!");
                SetReward(1.0f); // Strong positive reward for catching
                EndEpisode(); // End the training episode
            }
        }
        else
        {
            // Agent cannot currently see the player
            navMeshAgent.speed = patrolSpeed; // Slow down if not chasing

            if (knowsPlayerLocation) // Move towards last known spot
            {
                // Small negative reward for *losing* sight while knowing location? Optional.
                // AddReward(-0.001f);
                navMeshAgent.SetDestination(lastKnownPlayerPosition);
                // Check if near last known pos and give up if still can't see
                if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 2.0f && !CanSeePlayer())
                {
                    knowsPlayerLocation = false;
                    AddReward(-0.05f); // Penalty for reaching last known pos and not finding player
                }
            }
            else // Doesn't know location, Patrol (Action 0)
            {
                // If reached patrol point or path is invalid, get next one
                if (patrolPoints.Length > 0 && (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f))
                {
                    GoToNextPatrolPoint();
                }
                // Small negative reward for time passing without finding player (encourages exploration)
                AddReward(-0.0005f);
            }
        }

        // Small penalty for staying still for too long (optional)
        if (navMeshAgent.velocity.sqrMagnitude < 0.01f && navMeshAgent.hasPath && navMeshAgent.remainingDistance > 0.1f)
        {
            AddReward(-0.005f);
        }
    }

    // Player Detection Helper
    bool CanSeePlayer()
    {
        if (playerTransform == null || IsPlayerHidden) return false; // Cannot see if player doesn't exist or is hidden

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > detectionDistance) return false; // Too far away

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Check Field of View (e.g., 120 degrees)
        if (angleToPlayer > 60f) return false; // Adjust FOV angle as needed

        // Raycast to check for obstructions
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, // Start ray slightly above ground
                            directionToPlayer,
                            out hit,
                            detectionDistance,
                            ~obstacleLayer)) // Raycast hits anything *except* the obstacle layer
        {
            // Check if the hit object is the player
            if (hit.transform == playerTransform || hit.transform.IsChildOf(playerTransform))
            {
                return true; // Can see the player!
            }
        }

        return false; // Obstacle in the way or other checks failed
    }

    // Manual Input for Testing (Optional)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0; // Default to patrol/continue path

        if (Input.GetKey(KeyCode.Space)) // Example: Press Space to manually trigger "Chase"
        {
            discreteActionsOut[0] = 1; // Chase
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return; // No points defined

        // If we've visited all patrol points, reset our tracking
        if (visitedPatrolPoints.Count >= patrolPoints.Length)
        {
            visitedPatrolPoints.Clear();
            Debug.Log("All patrol points visited, resetting patrol cycle");
        }

        // Get a list of unvisited points
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (!visitedPatrolPoints.Contains(i))
            {
                availableIndices.Add(i);
            }
        }

        // If no unvisited points (shouldn't happen due to the clear above, but just in case)
        if (availableIndices.Count == 0)
        {
            visitedPatrolPoints.Clear();
            currentPatrolIndex = Random.Range(0, patrolPoints.Length);
            Debug.LogWarning("No available patrol points found, selecting random point");
        }
        else
        {
            // Select randomly from unvisited points
            int randomIndex = Random.Range(0, availableIndices.Count);
            currentPatrolIndex = availableIndices[randomIndex];
        }

        // Add to visited list
        visitedPatrolPoints.Add(currentPatrolIndex);
        Vector3 targetPoint = patrolPoints[currentPatrolIndex].position;

        // Check if the point is on the NavMesh before setting destination
        if (NavMesh.SamplePosition(targetPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            if (navMeshAgent.SetDestination(hit.position))
            {
                Debug.Log($"NPC patrolling towards point index {currentPatrolIndex} at {hit.position} (Visit {visitedPatrolPoints.Count} of {patrolPoints.Length})");
            }
            else
            {
                Debug.LogWarning($"SetDestination failed for point {currentPatrolIndex}");
                visitedPatrolPoints.Remove(currentPatrolIndex); // Remove from visited since we couldn't go there
                GoToNextPatrolPoint(); // Try another point
            }
        }
        else
        {
            Debug.LogWarning($"Patrol point index {currentPatrolIndex} is not on or near NavMesh!");
            visitedPatrolPoints.Remove(currentPatrolIndex); // Remove from visited since it's not valid

            // Try another point without getting stuck in recursion
            if (visitedPatrolPoints.Count < patrolPoints.Length - 1)
            {
                GoToNextPatrolPoint();
            }
            else
            {
                // If we can't find valid points, just try random ones
                int randomPoint = Random.Range(0, patrolPoints.Length);
                if (NavMesh.SamplePosition(patrolPoints[randomPoint].position, out hit, 2.0f, NavMesh.AllAreas))
                {
                    navMeshAgent.SetDestination(hit.position);
                    Debug.Log($"Falling back to random patrol point {randomPoint} as other points were invalid");
                }
            }
        }
    }


    // Public method for Player script to call when hiding/unhiding
    public void SetPlayerHidingStatus(bool isHiding)
    {
        IsPlayerHidden = isHiding;
        if (isHiding)
        {
            knowsPlayerLocation = false; // If player hides, agent instantly loses track
        }
        Debug.Log("NPC knows player hiding status: " + isHiding);
    }
}