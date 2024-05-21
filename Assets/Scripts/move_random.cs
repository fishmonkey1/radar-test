using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public class move_random : MonoBehaviour
{
    [SerializeField] private float Radius = 20;
    [SerializeField] private bool Debug_Bool;

    [SerializeField] NavMeshAgent Agent;

    Vector3 next_pos;
    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        next_pos = transform.position;
    }

    private void Update()
    {
        if (Vector3.Distance(next_pos, transform.position) <= 1.5f)
        {
            next_pos = randomPointsGen.randomPoint(transform.position, Radius);
            Agent.SetDestination(next_pos);
        }
    }

    private void OnDrawGizmos()
    {
        if (Debug_Bool)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, next_pos);
        }
    }




}