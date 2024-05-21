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

    private SpriteRenderer spriteRenderer;
    private float disappearTimer;
    private float disappearTimerMax; //length of fade

    private Color color; // current color of the minimap icon

    GameObject clone;


    Vector3 next_pos;
    private void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        
        disappearTimer = 0f;
        disappearTimerMax = radar.disappearTimerMax;

        next_pos = transform.position;
        isPinged = false;
        MinimapIcon.SetActive(false);
    }

    private void Update()
    {   
        // move randomly
        if (Vector3.Distance(next_pos, transform.position) <= 1.5f)
        {
            next_pos = randomPointsGen.randomPoint(transform.position, Radius);
            Agent.SetDestination(next_pos);
        }

        
        
        if (isPinged) // if currently showing on radar
        { 
            // increment timer for the ping's duration
            disappearTimer += Time.deltaTime;

            color.a = Mathf.Lerp(disappearTimerMax, 0f, disappearTimer / disappearTimerMax);
            spriteRenderer.color = color; // set new color for ping
                                          // based on duration
            
            // clean up once it is gone
            if (disappearTimer >= disappearTimerMax)
            {
                Destroy(clone);
                isPinged = false;
                disappearTimer = 0f;
            }

        }
    }

    public void pingOnRadar()
    {   
        if (!isPinged)
        {
            isPinged = true;
            clone = (GameObject)Instantiate(MinimapIcon, transform.position, Quaternion.Euler(new Vector3(90,0,0)));
            spriteRenderer = clone.GetComponent<SpriteRenderer>();
            color = spriteRenderer.color;
            clone.SetActive(true);
   
        }
        
    }



}