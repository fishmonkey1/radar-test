using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tankSteer : MonoBehaviour
{
    [SerializeField] float maxSpeed = 50.0f;
    [SerializeField] float maxReverseSpeed = -2.0f;
    [SerializeField] float turnAngleMin = 20f;
    [SerializeField] float turnAngleMax = 50f;
    [SerializeField] float acceleration = 2.0f;
    [SerializeField] float decceleration = 5f;
    [SerializeField] float currSpeed;
    [SerializeField] float turnAngle;


    void Update()
    {   
        if (Input.GetAxis("Vertical") != 0f)
        {
            currSpeed = Mathf.MoveTowards(currSpeed, maxSpeed * Input.GetAxis("Vertical"), acceleration * Time.deltaTime);   
        } else 
        {    //if not accel/decel, move speed towards 0
            currSpeed = Mathf.MoveTowards(currSpeed, 0, decceleration * Time.deltaTime);
        }
        Mathf.Clamp(currSpeed, maxReverseSpeed, maxSpeed);

        // The faster we are, the less we can turn
        float turnAngleMaffs = turnAngleMax * (1f - (Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(currSpeed))));
        turnAngle = Mathf.Clamp(turnAngleMaffs, turnAngleMin, turnAngleMax);

        transform.Rotate(Vector3.up * Input.GetAxis("Horizontal") * turnAngle * Time.deltaTime);
        transform.Translate(Vector3.forward * currSpeed * Time.deltaTime);


    }

}
