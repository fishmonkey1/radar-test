using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    [SerializeField] DamageInfo damage; //THATS A LOTTA DAMAGE
    [SerializeField] float speed; //How quick it moves towards you
    [SerializeField] GameObject explosionPrefab; //Use this for making the boom after this goes bye bye
    public GameObject shooter; //Who sent this projectile
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

        if (damage.AreaDamageType == DamageInfo.AreaDamage.SPHERE)
        {
            Collider[] hitTargets = Physics.OverlapSphere(transform.position, damage.SphereRadius);
            //Now do damage stuff to everybody if they can be hurt
            //Use their distance from the projectile with the curve to determine amount of damage done
            foreach (Collider collider in hitTargets)
            {
                Health health = collider.GetComponent<Health>();
                if (health != null)
                {
                    float targetDamage = CalculateDamage(collider.gameObject);
                    health.RemoveHealth(targetDamage, shooter);
                }
            }
        }

        if (damage.AreaDamageType == DamageInfo.AreaDamage.POINT)
        {
            //Otherwise the damage is only done to whatever the projectile hit, provided it can be hurt
            Health health = collision.gameObject.GetComponent<Health>();
            if (health != null)
                health.RemoveHealth(damage.Damage, shooter);
        }

        //Then create the explosion and destroy this projectile
        GameObject.Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        GameObject.Destroy(gameObject); //Destroy our projectile

    }

    private float CalculateDamage(GameObject target)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        float falloffCurve = Mathf.InverseLerp(0, damage.SphereRadius, distance);
        float damageAmount = damage.DamageFalloffCurve.Evaluate(falloffCurve);
        float totalDamage = damageAmount * damage.Damage;
        return totalDamage;
    }

}
