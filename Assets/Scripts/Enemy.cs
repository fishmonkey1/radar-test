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
    [SerializeField] private float NextMoveRadius = 20;

    NavMeshAgent Agent;

    public Dictionary<string, float> statusDict;

    Vector3 nextPosition;

    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();

        nextPosition = transform.position;

        statusDict = new Dictionary<string, float>();

        statusDict.Add("color", 0f); // all are green, unless they are red
        if (GetComponent<Renderer>().material.name == "red (Instance)") statusDict["color"] = 1f;

        statusDict.Add("isVisible", 1f); // so we can set it as invisible to radar in the future
    }

    private void Update()
    {
        // move randomly
        if (Vector3.Distance(nextPosition, transform.position) <= 1.5f)
        {
            nextPosition = randomPointsGen.randomPoint(transform.position, NextMoveRadius);
            Agent.SetDestination(nextPosition);
        }
    }
}