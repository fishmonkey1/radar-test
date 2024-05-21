using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// This is causing:
// 'Can't remover NavMeshAgent because move_random (Script) depends on it' error
// when trying to destroy itself
//[RequireComponent(typeof(NavMeshAgent))]

public class Enemy : MonoBehaviour
{
    [SerializeField] private float Radius = 20;
    [SerializeField] private bool Debug_Bool;

    [SerializeField] NavMeshAgent Agent;

    [SerializeField] GameObject MinimapIcon;

    [SerializeField] Radar radar;

    public bool isPinged;


    Vector3 next_pos;
    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        next_pos = transform.position;
        isPinged = false;
        MinimapIcon.SetActive(false);
    }

    private void Update()
    {
        if (Vector3.Distance(next_pos, transform.position) <= 1.5f)
        {
            next_pos = randomPointsGen.randomPoint(transform.position, Radius);
            Agent.SetDestination(next_pos);
        }
    }

    public void pingOnRadar()
    {   
        if (!isPinged)
        {
            isPinged = true;
            GameObject clone = (GameObject)Instantiate(MinimapIcon, transform.position, Quaternion.Euler(new Vector3(90,0,0)));
            clone.SetActive(true);
            Destroy(clone, radar.secondsShownOnMap);
            isPinged = false;
        }
        
    }



}