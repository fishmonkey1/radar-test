using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    [SerializeField] DamageInfo damage; //THATS A LOTTA DAMAGE
    [SerializeField] float speed; //How quick it moves towards you
    [SerializeField] GameObject explosionPrefab; //Use this for making the boom after this goes bye bye
    private Vector3 velocity; //Stores the movement of the projectile

    private void Start()
    {
        velocity = transform.forward * speed;
    }

    private void Update()
    {
        // Apply gravity to the velocity
        velocity.y += -9.81f * Time.deltaTime;

        // Move the projectile based on the velocity
        transform.position += velocity * Time.deltaTime;

        // Rotate the projectile to match the velocity direction
        if (velocity != Vector3.zero) // Check to avoid zero division
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //TODO: damage stuff here

        if (damage.AreaDamageType == DamageInfo.AreaDamage.SPHERE)
        {
            Collider[] hitTargets = Physics.OverlapSphere(transform.position, damage.SphereRadius);
            //Now do damage stuff to everybody if they can be hurt
            //Use their distance from the projectile with the curve to determine amount of damage done
        }

        if (damage.AreaDamageType == DamageInfo.AreaDamage.POINT)
        {
            //Otherwise the damage is only done to whatever the projectile hit, provided it can be hurt
        }

    }
}
