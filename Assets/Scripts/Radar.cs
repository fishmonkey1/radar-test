using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    [SerializeField] Transform radar;
    [SerializeField] public GameObject Player_MinimapIcon;
    [SerializeField] GameObject MinimapIcon;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float rotationsPerMinute = 10.0f;
    [SerializeField] public float disappearTimerMax = 3f;

    // List to track what the Raycast has collided with
    // so that we do not get multiple hits on the same object
    // as it passes over it.
    private List<Collider> collidedList;

    // Dictionary containing current radar blips
    // and when placed on radar
    // so we can fade them
    public Dictionary<GameObject, float> currentBlipsDict;

    // PlayerMove.cs grabs these at runtime.
    [Header("radar camera Y value zoom settings")]
    [Tooltip("Default 40")] [SerializeField] public float zoom1_y = 40;
    [Tooltip("Default 50")] [SerializeField] public float zoom2_y = 50;
    [Tooltip("Default 60")] [SerializeField] public float zoom3_y = 60;

    public float blipScale = 10; // The prefab has a scale of 10.
                                 // I probs wouldn't go lower than that

    private void Awake()
    {
        collidedList = new List<Collider>();
        currentBlipsDict = new Dictionary<GameObject, float>();
    }

    void FixedUpdate()
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(radar.position, radar.TransformDirection(Vector3.forward), 100f, layerMask);
        Debug.DrawRay(radar.position, radar.TransformDirection(Vector3.forward) * 100f, Color.black);

        // Raycasts, gets ship info, pings radar blip if applicable
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if (hit.collider != null)
            {
                // we're checking this so we don't
                // get multiple hits per object as it passes
                if (!collidedList.Contains(hit.collider)) 
                {
                    collidedList.Add(hit.collider);
                    var ship = hit.collider.GetComponent<MoveRandom>(); //this will be Enemy not MoveRandom
                    if (ship != null)
                    {
                        Dictionary<string, float> status = ship.statusDict;

                        if (status["isVisible"] == 1f) // ship may be hidden from radar temporarily
                        {
                            Vector3 location = hit.collider.transform.position;
                            pingOnRadar(status, location);
                        }
                    }
                    
                    StartCoroutine(collidedList_WaitThenRemove(hit.collider)); //I don't love this
                }
            }
        }


        List<GameObject> toDeleteFromDict = new List<GameObject>(); 

        // This handles fading for each ping
        foreach (KeyValuePair<GameObject, float> blip in currentBlipsDict)
        {
            GameObject blipObject = blip.Key;
            float ageInSeconds = Time.fixedTime - blip.Value;

            // Set new opacity based on age
            Color color = blipObject.GetComponent<SpriteRenderer>().color;                        // get current color
            float newAlpha = Mathf.Lerp(disappearTimerMax, 0f, ageInSeconds / disappearTimerMax); // calculate new Alpha value (opacity)
            color.a = newAlpha;                                                                   // set new Alpha to color
            blipObject.GetComponent<SpriteRenderer>().color = color;                              // set the blip's color to new color

            // Destroy GameObject once it has disappeared, and remove from Dict
            if (ageInSeconds > disappearTimerMax)
            {
                Destroy(blipObject);
                toDeleteFromDict.Add(blipObject);
            }
        }

        // Delete destroyed blip objects from Dictionary (can't remove while iterating Dict)
        foreach (GameObject blipObject in toDeleteFromDict)
        {
            currentBlipsDict.Remove(blipObject);
            //can also delete from collidedList here too but brain no worky rn
            //collidedList.Remove(???);
        }

        // Rotate the Radar for next frame's raycast
        radar.Rotate(0, 6.0f * rotationsPerMinute * Time.deltaTime, 0); 
    }

    private void pingOnRadar(Dictionary<string, float> status, Vector3 location)
    {
        GameObject blip = (GameObject)Instantiate(MinimapIcon, location, Quaternion.Euler(new Vector3(90, 0, 0)));
        
        // Set Color
        if (status["color"] == 0f)
        {
            blip.GetComponent<SpriteRenderer>().color = Color.green;
        }
        else blip.GetComponent<SpriteRenderer>().color = Color.red;

        // Set Scale
        blip.transform.localScale = new Vector3(blipScale, blipScale, blipScale);

        // Set to Visible when ready
        blip.SetActive(true);

        // Add to currently tracked blips dict
        currentBlipsDict.Add(blip, Time.fixedTime);
    }

    private IEnumerator collidedList_WaitThenRemove(Collider collider) 
    {
        yield return new WaitForSeconds(disappearTimerMax);
        collidedList.Remove(collider);
    }

}


