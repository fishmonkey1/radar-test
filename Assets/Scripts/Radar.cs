using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{

    [SerializeField] Transform radar;
    [SerializeField] float rotationsPerMinute = 10.0f;
    [SerializeField] public float secondsShownOnMap = 3;

    private List<Collider> colliderList;

    private void Awake()
    {
        colliderList = new List<Collider>();
    }

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

        // Does the ray intersect any objects excluding the player layer
        RaycastHit[] hits;
        hits = Physics.RaycastAll(radar.position, radar.TransformDirection(Vector3.forward), 100f, layerMask);
        Debug.DrawRay(radar.position, radar.TransformDirection(Vector3.forward) * 100f, Color.black);
        
        // this does the pinging 
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

             if (hit.collider != null)
             {
                if (!colliderList.Contains(hit.collider))
                {
                    colliderList.Add(hit.collider);
                    hit.collider.gameObject.SendMessage("pingOnRadar");
                    StartCoroutine(colliderList_WaitThenRemove(hit.collider));
                }
            }

            
        }
        
            

        radar.Rotate(0, 6.0f * rotationsPerMinute * Time.deltaTime, 0);


    }

    private IEnumerator colliderList_WaitThenRemove(Collider collider)
    {
        yield return new WaitForSeconds(secondsShownOnMap);
        colliderList.Remove(collider);
    }

}
