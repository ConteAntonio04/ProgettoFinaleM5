using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State
    {
        Idle,
        Patrol,
        Chase,
        Search
    }

    private State state;

    NavMeshAgent agent;
    Transform player;

    [Header("Enemy Settngs")]
    [SerializeField]
    private Transform[] patrolPoints;
    private int currentPoint = 0;

    [SerializeField]
    private float viewDistance = 10f;
    [SerializeField]
    private float viewAngle = 60f;

    private Vector3 lastKnownPlayerPos;

    private float idleTimer = 0f;
    [SerializeField]
    private float timeToRotate = 3f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (patrolPoints.Length > 0)
            state = State.Patrol;
        else
            state = State.Idle;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) < 1.5f)
        {
            FindObjectOfType<GameManager>().GameOver();
        }
        switch (state)
        {
            case State.Idle:
                Idle();
                break;

            case State.Patrol:
                Patrol();
                break;

            case State.Chase:
                Chase();
                break;

            case State.Search:
                Search();
                break;
        }
    }

    void Idle()
    {
        idleTimer += Time.deltaTime;

        if (idleTimer >= timeToRotate)
        {
            transform.Rotate(0, 90, 0);
            idleTimer = 0f;
        }

        if (CanSeePlayer())
        {
            state = State.Chase;
        }
    }

    void Patrol()
    {
        agent.SetDestination(patrolPoints[currentPoint].position);

        if (agent.remainingDistance < 0.5f)
        {
            currentPoint++;

            if (currentPoint >= patrolPoints.Length)
                currentPoint = 0;
        }

        if (CanSeePlayer())
        {
            state = State.Chase;
        }
    }

    void Chase()
    {
        agent.SetDestination(player.position);

        if (!CanSeePlayer())
        {
            state = State.Search;
        }
    }

    void Search()
    {
        agent.SetDestination(lastKnownPlayerPos);

        if (agent.remainingDistance < 0.5f)
        {
            if (patrolPoints.Length > 0)
                state = State.Patrol;
            else
                state = State.Idle;
        }
    }

    /*void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            FindObjectOfType<GameManager>().GameOver();
        }
    }*/

    bool CanSeePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;

        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            float angle = Vector3.Angle(transform.forward, direction);

            if (angle < viewAngle / 2)
            {
                Ray ray = new Ray(transform.position, direction);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, viewDistance))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        lastKnownPlayerPos = player.position;
                        return true;
                    }
                }
            }
        }

        return false;
    }

}
