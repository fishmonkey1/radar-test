using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

/// <summary>
/// This class handles all of the controls for the Radar role. See <see cref="IRoleNeeded"/> for how the PlayerProfile is used to limit which controls are read.
/// TODO: The controls will be separated into different mappings later, which means this will also need an update.
/// </summary>
public class RadarRole : NetworkBehaviour, IRoleNeeded 
{
    [Header("Radar Dependencies")]
    [SerializeField] Transform radar;
    [SerializeField] GameObject Player_MinimapIcon;
    [SerializeField] LineRenderer radarSweepLine;
    [SerializeField] GameObject SweepLine;
    
    /// <summary>
    /// Tracks which icons are on screen and when the blip was added.
    /// </summary>
    public Dictionary<GameObject, float> currentBlipsDict = new Dictionary<GameObject, float>();// This is a Dict of current blips on screen and when it was discovered

    /// <summary>
    /// All objects that have already been displayed on the radar to prevent multiple hits.
    /// </summary>
    private List<Collider> collidedList = new List<Collider>();// List of collided obj, so we don't get multiple hits on same obj

    /// <summary>
    /// Defines which layers the radar will collide on. Default is the Minimap layer.
    /// </summary>
    [Tooltip("The mask the radar is on. Should stay on Minimap")]
    [SerializeField] LayerMask layerMask;

    [Header("Radar Settings")]
    [Range(1f, 45f)] [SerializeField] float rotationsPerMinute = 10.0f;
    [Range(.02f, 5f)] [SerializeField] public float disappearTimerMax = 3f;
    [Range(.01f, 1f)] [SerializeField] public float sweepLineWidth = .5f;
    [SerializeField] private Color sweepLineColor;
    [Range(.1f, 1f)] [SerializeField] private float sweepLineOpacity = 1f;

    /// <summary>
    /// Implementation of <see cref="IRoleNeeded"/> that only allows a PlayerProfile with the Radar role to send inputs
    /// </summary>
    public Role RoleNeeded => CrewRoles.Radar;
    /// <summary>
    /// The PlayerProfile that this script is assigned to
    /// </summary>
    PlayerProfile playerProfile;

    // cam zoom stuff
    private Camera currentCam;
    private Vector2 zoomInput;
    [Tooltip("Y val limit close to ground")] [SerializeField] public float closeZoomLevel = 75;
    [Tooltip("Y val limit far from ground")] [SerializeField] public float farZoomLevel = 125;

    /// <summary>
    /// Finds the correct camera for this role when the <see cref="PlayerProfile.OnRoleChange"/> delegate gets called.
    /// </summary>
    /// <param name="oldRole">The role the profile had last.</param>
    /// <param name="newRole">The role the profile has now.</param>
    public void OnRoleChange(Role oldRole, Role newRole)
    {
        if (newRole != RoleNeeded) return;

        //Otherwise we do any setup in here
        currentCam = CamCycle.Instance.GetFirstCamera(RoleNeeded); //Fetch the camera for the driver so it's active
        Debug.Log($"Got first camera for {RoleNeeded.Name} role");
    }

    /// <summary>
    /// Assigns the player profile to this script so that inputs are read.
    /// </summary>
    /// <param name="profile">The profile that owns this script.</param>
    public void SetPlayer(PlayerProfile profile)
    {
        Debug.Log("Assigning local player to RadarRole. info's role is " + profile.CurrentRole);
        if (RoleNeeded == profile.CurrentRole)
        {
            Debug.Log("Player's role matches for RadarRole");
            currentCam = CamCycle.Instance.GetFirstCamera(RoleNeeded);
        }
        playerProfile = profile;
        if (playerProfile.OnRoleChange == null)
            playerProfile.OnRoleChange = new PlayerProfile.RoleChangeDelegate(OnRoleChange);
        else
            playerProfile.OnRoleChange += OnRoleChange;
    }

    // Start is called before the first frame update
    void Start()
    {
        // set default Y to close/far midpoint
        var midZoomLevel = closeZoomLevel + ((farZoomLevel-closeZoomLevel)/2);
        currentCam.transform.position = new Vector3(currentCam.transform.position.x, midZoomLevel, currentCam.transform.position.z);

    }

    /// <summary>
    /// Handles drawing the radar sweep line and rotating the collider for the radar. See <see cref="OnTriggerEnter(Collider)"/> for where RadarTargets are acquired
    /// </summary>
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

    /// <summary>
    /// Gradually lerps the alpha of all of the blips that have been found. Blips are destroyed here when they have completely faded.
    /// TODO: Object pooling for the blips so we aren't creating so much garbage.
    /// </summary>
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

    /// <summary>
    /// Sent from clients to the server so it can be executed there and the results sent back to them.
    /// TODO: Verify this is being called correctly, because it looks like input are only sent when the radar is zoomed
    /// </summary>
    /// <param name="input">A Vector2 sent from <see cref="OnRadarZoomScroll(InputValue)"/></param>
    [Command(requiresAuthority = false)]
    public void CmdSendInputs(Vector2 input)
    {
        //Set the inputs on the server and then let it replicate the tank's position for everyone else
        zoomInput.x = input.x;
        zoomInput.y = input.y;
    }

    /// <summary>
    /// Unity InputSystem handler which reads a Vector2 axis for the mouse wheel with default bindings.
    /// </summary>
    /// <param name="value">The Vector2 passed in by the InputSystem</param>
    public void OnRadarZoomScroll(InputValue value)
    {
        if (playerProfile == null)
            return; //This role is unused, so do nothing
        if (!((IRoleNeeded)this).HaveRole(playerProfile.CurrentRole))
            return; //Don't allow radar inputs if you don't have the radar role selected
        if (isServer) //Only apply locally if you are the host
            zoomInput = value.Get<Vector2>();
        else
            CmdSendInputs(value.Get<Vector2>()); //Otherwise send to the server
    }

    /// <summary>
    /// Called when the radar collider encounters an object. See <see cref="layerMask"/> for which layers are checked, and <see cref="FixedUpdate"/> for where the collider is rotated.
    /// </summary>
    /// <param name="collider">The collider of the object we encountered.</param>
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
