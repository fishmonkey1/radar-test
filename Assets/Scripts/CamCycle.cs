using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Placed on a HorniTank so the local player can change which camera they're using.
/// </summary>
public class CamCycle : MonoBehaviour
{
    [SerializeField]
    List<Camera> gunnerCameras = new List<Camera>();
    [SerializeField]
    List<Camera> driverCameras = new List<Camera>();
    [SerializeField]
    List<Camera> radarCameras = new List<Camera>();

    Dictionary<Role, List<Camera>> roleCameras = new Dictionary<Role, List<Camera>>();

    static CamCycle instance; //hopefully this won't need to be a singleton later. ;o;
    /// <summary>
    /// There should only be one active CamCycle script in a gameplay scene.
    /// </summary>
    public static CamCycle Instance => instance;

    private void Awake()
    {
        instance = this;
        roleCameras.Add(CrewRoles.Driver, driverCameras);
        roleCameras.Add(CrewRoles.Gunner, gunnerCameras);
        roleCameras.Add(CrewRoles.Radar, radarCameras);
    }

    /// <summary>
    /// Move on to the next camera in the role's list.
    /// </summary>
    /// <param name="role">Player's current role.</param>
    /// <param name="currentCam">The last cam they were using.</param>
    /// <returns>The next camera in the list.</returns>
    /// <exception cref="System.NotImplementedException">If there is no list for the passed role, you get an error.</exception>
    public Camera GetNextCamera(Role role, Camera currentCam)
    {
        if (!roleCameras.ContainsKey(role)) //We haven't added that role's cameras, so we catch bugs and oopsies here
            throw new System.NotImplementedException();
        List<Camera> cams = roleCameras[role];
        int index = cams.IndexOf(currentCam);
        index++;
        if (index >= cams.Count)
            index = 0;
        if (currentCam != null)
            currentCam.gameObject.SetActive(false); //Turn off the previous cam
        Camera nextCam = cams[index];
        nextCam.gameObject.SetActive(true); //And turn on the new one
        return nextCam; //Send back the next camera in the list
    }

    /// <summary>
    /// Find the first camera in a role's list.
    /// </summary>
    /// <param name="role">The role you want the first camera for.</param>
    /// <returns>The first camera in a role's list.</returns>
    /// <exception cref="System.NotImplementedException">If there is no list for the passed role, you get an error.</exception>
    public Camera GetFirstCamera(Role role)
    {
        if (!roleCameras.ContainsKey(role))
            throw new System.NotImplementedException();
        List<Camera> cams = roleCameras[role];
        Camera newCam = cams[0];
        newCam.gameObject.SetActive(true);
        return newCam;

    }

    /// <summary>
    /// Fetch all of the cameras for a role.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public List<Camera> GetCamerasByRole(Role role)
    {
        foreach (var entry in roleCameras)
        {
            if (entry.Key.Name == role.Name) //We found a match
                return entry.Value;
        }
        return null;
    }
    /// <summary>
    /// Tell the local script that the local player has changed their current role.
    /// </summary>
    /// <param name="lastRole"></param>
    /// <param name="newRole"></param>
    public void ChangeRoles(Role lastRole, Role newRole)
    {
        List<Camera> lastCams = GetCamerasByRole(lastRole);
        List<Camera> newCams = GetCamerasByRole(newRole);
        if (newCams == null)
        {
            Debug.Log($"newRole is not in roleCamera dictionary. newRole is {newRole.Name} and lastRole is {lastRole.Name}");
            return;
        }
        if (lastCams != null)
        {
            foreach (Camera cam in lastCams)
            {
                cam.gameObject.SetActive(false);
            }
            Debug.Log($"Disabled cameras for {lastRole.Name} role");
        }

        newCams[0].gameObject.SetActive(true); //Turn on the first cam for the role
    }

}
