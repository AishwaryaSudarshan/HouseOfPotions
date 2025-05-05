using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.AI;
using System.Collections.Generic;
using TMPro;
using System;

public class HideAndSeekNPC : Agent
{
    public Transform playerTransform;
    public float detectionDistance = 10f;
    public float loseSightDistance = 15f;
    public LayerMask obstacleLayer;
    public float patrolSpeed = 2.0f;
    public float chaseSpeed = 4.0f;
    public Transform[] patrolPoints;

    public TMP_Text distanceText;

    private NavMeshAgent navMeshAgent;
    private Vector3 lastKnownPlayerPosition;
    private bool knowsPlayerLocation = false;
    private int currentPatrolIndex = 0;

    public bool IsPlayerHidden { get; set; } = false;

    private List<int> visitedPatrolPoints = new List<int>();

    public override void Initialize()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
            else
                Debug.LogError("Player Transform not found or assigned on NPC Agent!");
        }
        navMeshAgent.speed = patrolSpeed;
    }

    public override void OnEpisodeBegin()
    {
        knowsPlayerLocation = false;
        navMeshAgent.speed = patrolSpeed;
        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.ResetPath();
        }
        GoToNextPatrolPoint();
        Debug.Log("NPC Episode Begin.");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        bool canSeePlayer = CanSeePlayer();

        sensor.AddObservation(canSeePlayer);
        sensor.AddObservation(knowsPlayerLocation);

        Vector3 targetDirection = Vector3.zero;
        if (canSeePlayer)
        {
            targetDirection = (playerTransform.position - transform.position).normalized;
            lastKnownPlayerPosition = playerTransform.position;
            knowsPlayerLocation = true;
        }
        else if (knowsPlayerLocation)
        {
            targetDirection = (lastKnownPlayerPosition - transform.position).normalized;
            if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 1.5f)
            {
                knowsPlayerLocation = false;
            }
        }
        else
        {
            if (patrolPoints.Length > 0 && navMeshAgent.hasPath)
            {
                targetDirection = (navMeshAgent.steeringTarget - transform.position).normalized;
            }
        }
        sensor.AddObservation(transform.InverseTransformDirection(targetDirection));
        sensor.AddObservation(navMeshAgent.velocity.normalized);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        bool canSeePlayer = CanSeePlayer();
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (canSeePlayer)
        {
            AddReward(0.01f);
            navMeshAgent.speed = chaseSpeed;
            navMeshAgent.SetDestination(playerTransform.position);

            if (distanceToPlayer < 1.5f)
            {
                Debug.Log("NPC caught the player based on distance and vision!");
                EndGameManager endGame = FindFirstObjectByType<EndGameManager>();
                if (endGame != null)
                {
                    endGame.hasEnded = true;
                    endGame.ShowEndScreen();
                }
                else
                {
                    Debug.LogWarning("EndGameManager not found in scene.");
                }
                return;
            }
        }
        else
        {
            navMeshAgent.speed = patrolSpeed;

            if (knowsPlayerLocation)
            {
                AddReward(-0.001f);
                navMeshAgent.SetDestination(lastKnownPlayerPosition);
                if (Vector3.Distance(transform.position, lastKnownPlayerPosition) < 2.0f && !CanSeePlayer())
                {
                    knowsPlayerLocation = false;
                    AddReward(-0.05f);
                }
            }
            else
            {
                if (patrolPoints.Length > 0 && (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.5f))
                {
                    GoToNextPatrolPoint();
                }
                AddReward(-0.0005f);
            }
        }

        if (navMeshAgent.velocity.sqrMagnitude < 0.01f && navMeshAgent.hasPath && navMeshAgent.remainingDistance > 0.1f)
        {
            AddReward(-0.005f);
        }
    }

    private void Update()
    {
        if (distanceText != null && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            distanceText.text = $"NPC is {Math.Ceiling(distance - 1.5)} m away";
            float t = Mathf.InverseLerp(1.5f, detectionDistance, distance);
            distanceText.color = Color.Lerp(Color.red, Color.white, t);
        }
    }


    bool CanSeePlayer()
    {
        if (playerTransform == null || IsPlayerHidden) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > detectionDistance) return false;

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > 60f) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f,
                            directionToPlayer,
                            out hit,
                            detectionDistance,
                            ~obstacleLayer))
        {
            if (hit.transform == playerTransform || hit.transform.IsChildOf(playerTransform))
            {
                return true;
            }
        }

        return false;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.Space))
        {
            discreteActionsOut[0] = 1;
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (visitedPatrolPoints.Count >= patrolPoints.Length)
        {
            visitedPatrolPoints.Clear();
            Debug.Log("All patrol points visited, resetting patrol cycle");
        }

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (!visitedPatrolPoints.Contains(i))
            {
                availableIndices.Add(i);
            }
        }

        if (availableIndices.Count == 0)
        {
            visitedPatrolPoints.Clear();
            currentPatrolIndex = UnityEngine.Random.Range(0, patrolPoints.Length);
            Debug.LogWarning("No available patrol points found, selecting random point");
        }
        else
        {
            int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
            currentPatrolIndex = availableIndices[randomIndex];
        }

        visitedPatrolPoints.Add(currentPatrolIndex);
        Vector3 targetPoint = patrolPoints[currentPatrolIndex].position;

        if (NavMesh.SamplePosition(targetPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            if (!navMeshAgent.SetDestination(hit.position))
            {
                Debug.LogWarning($"SetDestination failed for point {currentPatrolIndex}");
                visitedPatrolPoints.Remove(currentPatrolIndex);
                GoToNextPatrolPoint();
            }
        }
        else
        {
            Debug.LogWarning($"Patrol point index {currentPatrolIndex} is not on or near NavMesh!");
            visitedPatrolPoints.Remove(currentPatrolIndex);
            if (visitedPatrolPoints.Count < patrolPoints.Length - 1)
            {
                GoToNextPatrolPoint();
            }
            else
            {
                int randomPoint = UnityEngine.Random.Range(0, patrolPoints.Length);
                if (NavMesh.SamplePosition(patrolPoints[randomPoint].position, out hit, 2.0f, NavMesh.AllAreas))
                {
                    navMeshAgent.SetDestination(hit.position);
                    Debug.Log($"Falling back to random patrol point {randomPoint} as other points were invalid");
                }
            }
        }
    }

    public void SetPlayerHidingStatus(bool isHiding)
    {
        IsPlayerHidden = isHiding;
        if (isHiding)
        {
            knowsPlayerLocation = false;
        }
        Debug.Log("NPC knows player hiding status: " + isHiding);
    }
}
