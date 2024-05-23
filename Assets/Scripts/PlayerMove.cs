using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{

	[SerializeField] private float moveSpeed = 10.0f;

    void Update()
	{
		if (Input.GetKey(KeyCode.W)) transform.position += Vector3.forward * moveSpeed * Time.deltaTime;
		if (Input.GetKey(KeyCode.A)) transform.position += Vector3.left    * moveSpeed * Time.deltaTime;
		if (Input.GetKey(KeyCode.S)) transform.position += Vector3.back    * moveSpeed * Time.deltaTime;
		if (Input.GetKey(KeyCode.D)) transform.position += Vector3.right   * moveSpeed * Time.deltaTime;	
    }
}
