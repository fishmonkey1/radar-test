using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    [Header("Radar Dependencies")]
    [SerializeField] Transform radar;
    [SerializeField] GameObject playerCamera;
    [SerializeField] GameObject radarCamera;
    [SerializeField] GameObject Player_MinimapIcon;
    [SerializeField] LineRenderer radarSweepLine;
    [SerializeField] GameObject SweepLine;

    [Tooltip("The mask the radar is on. Should stay on Minimap")]
    [SerializeField] LayerMask layerMask;

    [Header("Radar Settings")]
    [Range(1f, 45f)] [SerializeField] float rotationsPerMinute = 10.0f;
    [Range(.02f, 5f)] [SerializeField] public float disappearTimerMax = 3f;
    [Range(.01f, 1f)] [SerializeField] public float sweepLineWidth = .5f;
    [SerializeField] private Color sweepLineColor;
    [Range(.1f, 1f)] [SerializeField] private float sweepLineOpacity = 1f;

    [Header("radar camera Y value zoom settings")]
    [Header("(just for debug till we find good values)")]
    [Tooltip("Default 75")] [SerializeField] public float zoom1_y = 75;
    [Tooltip("Default 125")] [SerializeField] public float zoom2_y = 125;
    [Tooltip("Default 175")] [SerializeField] public float zoom3_y = 175;

    // List to track what the Raycast has collided with
    // so that we do not get multiple hits on the same object
    // as it passes over it.
    private List<Collider> collidedList;

    // Dictionary containing current radar blips
    // and when placed on radar
    // so we can fade them
    public Dictionary<GameObject, float> currentBlipsDict;

    public float blipScale = 10; // The prefab has a scale of 10. I wouldn't go lower than that


    private float zoom1_scale; // these are set automatically
    private float zoom2_scale; // based on initial blipScale and zoom_y
    private float zoom3_scale; // value at runtime

    private int radarZoomLevel; // 1-3, 3 most zoomed out

    private void Awake()
    {
        collidedList = new List<Collider>();
        currentBlipsDict = new Dictionary<GameObject, float>();

        zoom1_scale = blipScale;
        zoom2_scale = zoom1_scale + (zoom1_scale * (((zoom2_y - zoom1_y) / zoom1_y)));
        zoom3_scale = zoom1_scale + (zoom1_scale * (((zoom3_y - zoom1_y) / zoom1_y)));

        radarCamera.transform.position = new Vector3(radarCamera.transform.position.x, zoom1_y, radarCamera.transform.position.z);
    }

    void FixedUpdate()
    {
        changeZoom();

        RaycastHit[] hits;
        hits = Physics.RaycastAll(radar.position, radar.TransformDirection(Vector3.forward), 100f, layerMask);

        // use for Debug
        //Debug.DrawRay(radar.position, radar.TransformDirection(Vector3.forward) * 100f, Color.black);
        DrawSweepLine(); // draws line on radar screen


        // This handles fading for each ping
        List<GameObject> toDeleteFromDict = new List<GameObject>();
        if (currentBlipsDict.Count > 0)
        {
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
        }


        // Delete destroyed blip objects from Dictionary (can't remove while iterating Dict)
        if (toDeleteFromDict.Count > 0)
        {
            foreach (GameObject blipObject in toDeleteFromDict)
            {
                currentBlipsDict.Remove(blipObject);
                //can also delete from collidedList here too but brain no worky rn
                //collidedList.Remove(???);
            }
        }
        // Rotate the Radar for next frame's raycast
        radar.Rotate(0, 6.0f * rotationsPerMinute * Time.deltaTime, 0);
    }

    void OnTriggerEnter(Collider collider)
    {
        // we're checking this so we don't
        // get multiple hits per object as it passes
        if (!collidedList.Contains(collider))
        {
            collidedList.Add(collider);
            var radarTarget = collider.GetComponent<RadarTarget>();
            if (radarTarget != null)
            {
                if (radarTarget.IsRadarVisible) // ship may be hidden from radar temporarily
                {
                    Vector3 location = collider.transform.position;
                    pingOnRadar(radarTarget, location);
                }
            }

            StartCoroutine(collidedList_WaitThenRemove(collider)); //I don't love this
        }
    }


    private void pingOnRadar(RadarTarget radarTarget, Vector3 location)
    {
        GameObject blip = (GameObject)Instantiate(radarTarget.MinimapIconPrefab, location, Quaternion.Euler(new Vector3(90, 0, 0)));

        // Set Color
        blip.GetComponent<SpriteRenderer>().color = radarTarget.MinimapIconColor;

        // Set Scale
        blip.transform.localScale = new Vector3(blipScale, blipScale, blipScale);

        // Add to currently tracked blips dict
        currentBlipsDict.Add(blip, Time.fixedTime);
    }

    private void changeZoom()
    {
        /* Get % increase of start zoom (Y40) to new zoom.
           So Y50 would be a 25% increase from Y40, 
           so we would scale up icons by that much.

           radar icons start scaled at 10 for Y40.
           Then need to be scaled at 12.5 for Y50.
           Then need to be scaled at 15 for Y60. */

        // scroll radar w/ mouse wheel
        radarCamera.transform.position += new Vector3(0, Input.mouseScrollDelta.y *10, 0);
 
        if (Input.GetKey(KeyCode.Alpha1))
        {
            if (radarZoomLevel != 1)
            {
                radarCamera.transform.position = new Vector3(radarCamera.transform.position.x, zoom1_y, radarCamera.transform.position.z);
                radarZoomLevel = 1;
                changeRadarIconScale(zoom1_scale);
            }
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            if (radarZoomLevel != 2)
            {
                radarCamera.transform.position = new Vector3(radarCamera.transform.position.x, zoom2_y, radarCamera.transform.position.z);
                radarZoomLevel = 2;
                changeRadarIconScale(zoom2_scale);
            }
        }

        if (Input.GetKey(KeyCode.Alpha3))
        {
            if (radarZoomLevel != 3)
            {
                radarCamera.transform.position = new Vector3(radarCamera.transform.position.x, zoom3_y, radarCamera.transform.position.z); ;
                radarZoomLevel = 3;
                changeRadarIconScale(zoom3_scale);
            }
        }
    }

    private void changeRadarIconScale(float newScale)
    {
        // Tell radar to start placing w/ new scale
        blipScale = newScale;

        // Change player scale
        Player_MinimapIcon.transform.localScale = new Vector3(newScale, newScale, newScale);

        // Change each blip scale to new scale
        foreach (KeyValuePair<GameObject, float> blip in currentBlipsDict)
        {
            blip.Key.transform.localScale = new Vector3(newScale, newScale, newScale);
        }
    }

    private void DrawSweepLine()
    {
        // TODO: get a calculated  kength for the line
        //       I just have it going for 50 units

        // set width
        radarSweepLine.startWidth = sweepLineWidth;
        radarSweepLine.endWidth = sweepLineWidth;

        //set opacity
        sweepLineColor.a = sweepLineOpacity;

        // set color
        radarSweepLine.startColor = sweepLineColor;
        radarSweepLine.endColor = sweepLineColor;

        // draw the damn thing
        /*  Gonna set it to under the camera instead of on the boat.
         Vector3 linepos = radarCamera.transform.position + new Vector3(0, sweepLineUnderCameraOffset, 0); //offset to under the radar camera
         radarSweepLine.SetPosition(0, linepos);
         radarSweepLine.SetPosition(1, linepos + radar.TransformDirection(Vector3.forward * 50));
         */
        radarSweepLine.SetPosition(0, radar.transform.position);
        radarSweepLine.SetPosition(1, radar.transform.position + radar.TransformDirection(Vector3.forward * 50));
    }

    private IEnumerator collidedList_WaitThenRemove(Collider collider)
    {
        yield return new WaitForSeconds(disappearTimerMax);
        collidedList.Remove(collider);
    }

}