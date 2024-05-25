using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tankSteer : MonoBehaviour
{
    [SerializeField] float speed = 5.0f;

    void Update()
    {
        float turnAngle = Mathf.Clamp(.75f / speed, -.5f, .5f); //this needs to be edited to be better but it works
        transform.Rotate(Vector3.up * Input.GetAxis("Horizontal") * (speed*10f) * Time.deltaTime);
        transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * speed * Time.deltaTime);  
    }

}
