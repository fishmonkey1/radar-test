using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamCycle : MonoBehaviour
{
    [SerializeField]
    List<Camera> allCameras = new List<Camera>();
    [SerializeField]
    List<Camera> gunnerCameras = new List<Camera>();
    [SerializeField]
    List<Camera> driverCameras = new List<Camera>();

    Dictionary<Role, List<Camera>> roleCameras = new Dictionary<Role, List<Camera>>();

    static CamCycle instance; //hopefully this won't need to be a singleton later. ;o;
    public static CamCycle Instance => instance;

    private void Awake()
    {
        instance = this;
        roleCameras.Add(CrewRoles.Driver, driverCameras);
        roleCameras.Add(CrewRoles.Gunner, gunnerCameras);
    }

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

    public Camera GetFirstCamera(Role role)
    {
        if (!roleCameras.ContainsKey(role))
            throw new System.NotImplementedException();
        List<Camera> cams = roleCameras[role];
        Camera newCam = cams[0];
        newCam.gameObject.SetActive(true);
        return newCam;

    }

    public void ChangeRoles(Role lastRole, Role newRole)
    {
        if (!roleCameras.ContainsKey(lastRole) || !roleCameras.ContainsKey(newRole))
            throw new System.NotImplementedException();
        foreach (Camera cam in roleCameras[lastRole])
        {
            cam.gameObject.SetActive(false);
        }
    }

}
