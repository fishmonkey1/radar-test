using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
	[SerializeField] GameObject playerCamera;
	[SerializeField] GameObject radarCamera;
	[SerializeField] GameObject radar;
	[SerializeField] private float speed = 10.0f;

	private int radarZoomLevel; // 1-3, 3 most zoomed out

    private void Start()
    {	
		//set radar to most zoomed in level
		radarCamera.transform.position = new Vector3(0, 40, 0);
		radarZoomLevel = 1;
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
		Then need to be scaled at 15 for Y60.

		 */


		if (Input.GetKey(KeyCode.Alpha1))
		{
			if (radarZoomLevel != 1)
			{
				radarCamera.transform.position = new Vector3(0, 40, 0);
				radarZoomLevel = 1;
				changeRadarIconScale(10f);
			}
		}

		if (Input.GetKey(KeyCode.Alpha2))
		{
			if (radarZoomLevel != 2)
            {
                radarCamera.transform.position = new Vector3(0, 50, 0);
				radarZoomLevel = 2;
				changeRadarIconScale(12.5f);
			}
        }

		if (Input.GetKey(KeyCode.Alpha3))
		{
			if (radarZoomLevel != 3)
            {
                radarCamera.transform.position = new Vector3(0, 60, 0);
				radarZoomLevel = 3;
				changeRadarIconScale(15f);
			}
        }

    }

	private void changeRadarIconScale(float newScale)
    {	
		// Tell radar to start placing w/ new scale

		// Get currentBlipsDict from Radar
		var currentBlipsDict = radar.GetComponent<Radar>().currentBlipsDict;

		foreach (KeyValuePair<GameObject, float> blip in currentBlipsDict)
		{
			var obj = blip.Key;
			obj.transform.localScale = new Vector3(newScale, newScale, newScale);
		}
	
	
	}

}
