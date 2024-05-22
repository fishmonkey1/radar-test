using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// This is causing:
// 'Can't remove NavMeshAgent because move_random (Script) depends on it' error
// when trying to destroy itself
[RequireComponent(typeof(NavMeshAgent))]

public class Enemy : MonoBehaviour
{
    [SerializeField] private float Radius = 20;
    [SerializeField] private bool Debug_Bool;

    NavMeshAgent Agent;

    public Dictionary<string, float> status;

    Vector3 next_pos;

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();

        next_pos = transform.position;

        status = new Dictionary<string, float>();

        status.Add("color", 0f); // all are green, unless they are red
        if (GetComponent<Renderer>().material.name == "red (Instance)") status["color"] = 1f;

        status.Add("isVisible", 1f); // so we can set it as invisible to radar
    }

    private void Update()
    {   
        // move randomly
        if (Vector3.Distance(next_pos, transform.position) <= 1.5f)
        {
            next_pos = randomPointsGen.randomPoint(transform.position, Radius);
            Agent.SetDestination(next_pos);
        }
    }
}