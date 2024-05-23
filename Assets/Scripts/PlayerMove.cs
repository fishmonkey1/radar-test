using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
	[SerializeField] GameObject playerCamera;
	[SerializeField] GameObject radarCamera;
	[SerializeField] GameObject radar;
	[SerializeField] private float speed = 10.0f;

	// Y levels for the radar camera
	private float zoom1_y;
	private float zoom2_y;
	private float zoom3_y;

	private float zoom1_scale;
	private float zoom2_scale;
	private float zoom3_scale;

	private int radarZoomLevel; // 1-3, 3 most zoomed out

    private void Start()
    {
		//set radar to most zoomed in level
		radarCamera.transform.position = new Vector3(0, 40, 0);
		radarZoomLevel = 1;

		// get zoom_Y levels from Radar script and calc scaling
		zoom1_y = radar.GetComponent<Radar>().zoom1_y;
		zoom2_y = radar.GetComponent<Radar>().zoom2_y;
		zoom3_y = radar.GetComponent<Radar>().zoom3_y;
		zoom1_scale = radar.GetComponent<Radar>().blipScale;
        zoom2_scale = zoom1_scale + (zoom1_scale * (((zoom2_y - zoom1_y) / zoom1_y)));
		zoom3_scale = zoom1_scale + (zoom1_scale * (((zoom3_y - zoom1_y) / zoom1_y)));
	}

    void Update()
	{

		if (Input.GetKey(KeyCode.D))
		{
			transform.position += Vector3.right * speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.A))
		{
			transform.position += Vector3.left * speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.W))
		{
			transform.position += Vector3.forward * speed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.S))
		{
			transform.position += Vector3.back * speed * Time.deltaTime;
		}

		/*
		main camera at Y50 rel to player

		Radar zoom starts zoomed-in at Y40.
		then Y50
		then Y60

		Get % increase of start zoom (Y40) to new zoom.
		So Y50 would be a 25% increase from Y40, 
		so we would scale up icons by that much.

		radar icons start scaled at 10 for Y40.
		Then need to be scaled at 12.5 for Y50.
		Then need to be scaled at 15 for Y60. */


		if (Input.GetKey(KeyCode.Alpha1))
		{
			if (radarZoomLevel != 1)
			{
				radarCamera.transform.position = new Vector3(0, zoom1_y, 0);
				radarZoomLevel = 1;
				changeRadarIconScale(zoom1_scale);
			}
		}

		if (Input.GetKey(KeyCode.Alpha2))
		{
			if (radarZoomLevel != 2)
            {
                radarCamera.transform.position = new Vector3(0, zoom2_y, 0);
				radarZoomLevel = 2;
				changeRadarIconScale(zoom2_scale);
			}
        }

		if (Input.GetKey(KeyCode.Alpha3))
		{
			if (radarZoomLevel != 3)
            {
                radarCamera.transform.position = new Vector3(0, zoom3_y, 0);
				radarZoomLevel = 3;
				changeRadarIconScale(zoom3_scale);
			}
        }

    }

	private void calulateScales() // just using this for testing
    {
		float perc_2 = ( ( (zoom2_y - zoom1_y) / zoom1_y));
		float perc_3 = ( ( (zoom3_y - zoom1_y) / zoom1_y));

		zoom2_scale = zoom1_scale + (zoom1_scale * perc_2);
		zoom3_scale = zoom1_scale + (zoom1_scale * perc_3);
	}

	private void changeRadarIconScale(float newScale)
    {
		// Tell radar to start placing w/ new scale
		radar.GetComponent<Radar>().blipScale = newScale;

		// Change player scale
		radar.GetComponent<Radar>().Player_MinimapIcon.transform.localScale = new Vector3(newScale, newScale, newScale);

		// Get currentBlipsDict from Radar, and change all to new scale
		var currentBlipsDict = radar.GetComponent<Radar>().currentBlipsDict;

		foreach (KeyValuePair<GameObject, float> blip in currentBlipsDict)
		{
			blip.Key.transform.localScale = new Vector3(newScale, newScale, newScale);
		}
	
	
	}

}
