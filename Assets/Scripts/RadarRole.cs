using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class RadarRole : NetworkBehaviour, IRoleNeeded 
{
    public Role RoleNeeded => CrewRoles.Radar;
    PlayerInfo playerInfo;
    private Camera currentCam;

    private Vector2 zoomInput;

    [SerializeField] GameObject radarCamera;
    [SerializeField] Transform radar;

    [Tooltip("Y val limit close to ground")] [SerializeField] public float closeZoomLevel = 75;
    [Tooltip("Y val limit far from ground")] [SerializeField] public float farZoomLevel = 125;

    /*
     * radar zoom w/ scroll wheel, change cameras FOV to sim zoom, or move camera
     */


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
        radarCamera.transform.position = new Vector3(radarCamera.transform.position.x, midZoomLevel, radarCamera.transform.position.z);

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        radarCamera.transform.position += new Vector3(0, zoomInput.y, 0);
    }

    [Command(requiresAuthority = false)]
    public void CmdSendInputs(Vector2 input)
    {
        //Set the inputs on the server and then let it replicate the tank's position for everyone else
        zoomInput.x = input.x;
        zoomInput.y = input.y;
    }

    public void OnZoomScroll(InputValue value)
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
}
