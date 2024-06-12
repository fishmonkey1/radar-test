using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// This is causing:
// 'Can't remove NavMeshAgent because move_random (Script) depends on it' error
// when trying to destroy itself


public class MoveRandom : MonoBehaviour
{
    [SerializeField] private float NextMoveRadius = 20;
    [SerializeField] private float speed;

    Vector3 nextPosition;

    private void Start()
    {
        nextPosition = transform.position;
    }

    private void Update()
    {
        // move randomly
        if (Vector3.Distance(nextPosition, transform.position) <= 1.5f)
        {
            nextPosition = randomPointsGen.randomPoint(transform.position, NextMoveRadius);
            var step = speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, step);
        }
    }
}