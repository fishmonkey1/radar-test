using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{

    [SerializeField] Transform MinimapIcon;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


    }

    void FixedUpdate()
    {
        // This is just the documentation code I'll get back to this lol

        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;


        // wat?


        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(MinimapIcon.position, MinimapIcon.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(MinimapIcon.position, MinimapIcon.TransformDirection(Vector3.forward) * hit.distance, Color.red);
            Debug.Log("Did Hit");
        }
        else
        {
            Debug.DrawRay(MinimapIcon.position, MinimapIcon.TransformDirection(Vector3.forward) * 1000, Color.black);
            Debug.Log("Did not Hit");
        }

        MinimapIcon.RotateAroundLocal(Vector3.up, .25f * Time.deltaTime);
    }
}
