using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{

    [SerializeField] Transform radar;
    [SerializeField] GameObject MinimapIcon;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float rotationsPerMinute = 10.0f;
    [SerializeField] public float secondsShownOnMap = 3f;
    [SerializeField] public float disappearTimerMax = 3f;

    // List to track what the Raycast has collided with
    // so that we do not get multiple hits on the same object
    // as it passes over it.
    private List<Collider> collidedList;

    // Dictionary containing current radar blips
    // and when placed on radar
    // so we can fade them
    private Dictionary<GameObject, float> currentBlipsDict;

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
        
        // this does the pinging 
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

             if (hit.collider != null)
             {
                if (!collidedList.Contains(hit.collider)) 
                {
                    collidedList.Add(hit.collider);
                    var ship = hit.collider.GetComponent<Enemy>();
                    if (ship != null)
                    {
                        Dictionary<string, float> status = ship.status;
                        
                        if (status["isVisible"] == 1f) // may be hidden from radar temporarily
                        {
                            Vector3 location = hit.collider.transform.position;
                            pingOnRadar(status, location);
                        }
                    }

                    StartCoroutine(collidedList_WaitThenRemove(hit.collider));
                }
            }

            
        }

        // This handles fading for each ping
        foreach (KeyValuePair<GameObject,float> blip in currentBlipsDict)
        {   

        }
        
        radar.Rotate(0, 6.0f * rotationsPerMinute * Time.deltaTime, 0);
    }

    private void pingOnRadar(Dictionary<string, float> status, Vector3 location)
    {
        GameObject icon;
        icon = (GameObject)Instantiate(MinimapIcon, location, Quaternion.Euler(new Vector3(90, 0, 0)));
        if (status["color"] == 0f)
        {
            icon.GetComponent<SpriteRenderer>().color = Color.green;
        } else icon.GetComponent<SpriteRenderer>().color = Color.red;

        icon.SetActive(true);
        currentBlipsDict.Add(icon,Time.fixedTime);

        StartCoroutine(icon_WaitThenRemove(icon));

    }

    private IEnumerator collidedList_WaitThenRemove(Collider collider)
    {
        yield return new WaitForSeconds(disappearTimerMax);
        collidedList.Remove(collider);
    }

    private IEnumerator icon_WaitThenRemove(GameObject blip)
    {
        yield return new WaitForSeconds(disappearTimerMax);
        Destroy(blip);
    }

}

/* TODO:
 * This is yoinked from Enemy.cs to be implemented later
 * 
  if (isPinged) // if currently showing on radar
    { 
        // increment timer for the ping's duration
        disappearTimer += Time.deltaTime;

        color.a = Mathf.Lerp(status["fadeTime"], 0f, disappearTimer / status["fadeTime"]);
        spriteRenderer.color = color; // set new color for ping, based on duration

        // clean up once it is gone
        if (disappearTimer >= status["fadeTime"])
        {
            Destroy(clone);
            status["isPinged"] = 0f;
            disappearTimer = 0f;
        }

    }*/
