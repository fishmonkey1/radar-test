using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class RadarRole : NetworkBehaviour, IRoleNeeded 
{
    [Header("Radar Dependencies")]
    [SerializeField] Transform radar;
    [SerializeField] GameObject Player_MinimapIcon;
    [SerializeField] LineRenderer radarSweepLine;
    [SerializeField] GameObject SweepLine;
    
    // This is a Dict of current blips on screen and when it was discovered
    public Dictionary<GameObject, float> currentBlipsDict = new Dictionary<GameObject, float>();

    // List of collided obj, so we don't get multiple hits on same obj
    private List<Collider> collidedList = new List<Collider>();

    [Tooltip("The mask the radar is on. Should stay on Minimap")]
    [SerializeField] LayerMask layerMask;

    [Header("Radar Settings")]
    [Range(1f, 45f)] [SerializeField] float rotationsPerMinute = 10.0f;
    [Range(.02f, 5f)] [SerializeField] public float disappearTimerMax = 3f;
    [Range(.01f, 1f)] [SerializeField] public float sweepLineWidth = .5f;
    [SerializeField] private Color sweepLineColor;
    [Range(.1f, 1f)] [SerializeField] private float sweepLineOpacity = 1f;

    // role stuff
    public Role RoleNeeded => CrewRoles.Radar;
    PlayerInfo playerInfo;

    // cam zoom stuff
    private Camera currentCam;
    private Vector2 zoomInput;
    [Tooltip("Y val limit close to ground")] [SerializeField] public float closeZoomLevel = 75;
    [Tooltip("Y val limit far from ground")] [SerializeField] public float farZoomLevel = 125;


    public void OnRoleChange(Role oldRole, Role newRole)
    {
        if (newRole != RoleNeeded) return;

        //Otherwise we do any setup in here
        currentCam = CamCycle.Instance.GetFirstCamera(RoleNeeded); //Fetch the camera for the driver so it's active
        Debug.Log($"Got first camera for {RoleNeeded.Name} role");
    }

    // Start is called before the first frame update
    void Start()
    {
        // set default Y to close/far midpoint
        var midZoomLevel = closeZoomLevel + ((farZoomLevel-closeZoomLevel)/2);
        currentCam.transform.position = new Vector3(currentCam.transform.position.x, midZoomLevel, currentCam.transform.position.z);

    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        // don't think we're ever using this array lol
        // see:  OnTriggerEnter()
        RaycastHit[] hits = Physics.RaycastAll(radar.position, 
                                               radar.TransformDirection(Vector3.forward), 
                                               100f, 
                                               layerMask);

        // change zoom if needed
        if (zoomInput.y != 0) { currentCam.transform.position += new Vector3(0, zoomInput.y, 0); }
        
        // Update SweepLine and Blips
        DrawSweepLine();
        FadeBlips();

        // Rotate the Radar for next frame's raycast ( see OnTriggerEnter() )
        radar.Rotate(0, 6.0f * rotationsPerMinute * Time.deltaTime, 0);

    }


    /// <summary>
    /// Uses LineRenderer to draw line on Radar canvas
    /// </summary>
    private void DrawSweepLine()
    {
        // TODO: get a calculated  ength for the line
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

    private void FadeBlips()
    {
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

        // Delete invisible blip icon GameObjects
        if (toDeleteFromDict.Count > 0)
        {
            foreach (GameObject blipObject in toDeleteFromDict)
            {
                currentBlipsDict.Remove(blipObject);
                //can also delete from collidedList here too but brain no worky rn
                //collidedList.Remove(???);
            }
        }
    }
    [Command(requiresAuthority = false)]
    public void CmdSendInputs(Vector2 input)
    {
        //Set the inputs on the server and then let it replicate the tank's position for everyone else
        zoomInput.x = input.x;
        zoomInput.y = input.y;
    }

    public void OnRadarZoomScroll(InputValue value)
    {
        if (playerInfo == null)
            return; //This role is unused, so do nothing
        if (!((IRoleNeeded)this).HaveRole(playerInfo.CurrentRole))
            return; //Don't allow radar inputs if you don't have the radar role selected
        if (isServer) //Only apply locally if you are the host
            zoomInput = value.Get<Vector2>();
        else
            CmdSendInputs(value.Get<Vector2>()); //Otherwise send to the server
    }

    void OnTriggerEnter(Collider collider)
    {
        /*
         * Because the radar is rotating, we can get multiple hits on same obj as it passes over
         * 
         * 1) Add obj to collidedList if it's not there already
         * 2) Start coroutine to delete obj from collidedList after 3 seconds
         * 3) on subsequent raycasts, ignore if in collidedList
         * 
         * TODO?
         * We can probably have this done using the currentBlipsDict, 
         * checking the age of the blip and not counting it unless it's older than X time
         */

        if (!collidedList.Contains(collider))
        {
            collidedList.Add(collider);
            var radarTarget = collider.GetComponent<RadarTarget>();
            if (radarTarget != null)
            {
                if (radarTarget.IsRadarVisible) // ship may be hidden from radar temporarily
                {
                    Vector3 location = collider.transform.position;

                    GameObject blip = (GameObject)Instantiate(radarTarget.MinimapIconPrefab, location, Quaternion.Euler(new Vector3(90, 0, 0)));

                    // Set Color
                    blip.GetComponent<SpriteRenderer>().color = radarTarget.MinimapIconColor;

                    // Set Scale
                    //blip.transform.localScale = new Vector3(blipScale, blipScale, blipScale);

                    // Add to currently tracked blips dict
                    currentBlipsDict.Add(blip, Time.fixedTime);

                }
            }

            StartCoroutine(collidedList_WaitThenRemove(collider)); //I don't love this
        }
    }

    private IEnumerator collidedList_WaitThenRemove(Collider collider)
    {
        yield return new WaitForSeconds(3.0f);
        collidedList.Remove(collider);
    }

}
