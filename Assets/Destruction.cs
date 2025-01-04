using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destruction : MonoBehaviour
{
    Animator animator;

    [SerializeField] ParticleSystem DefaultExplosionParticles;
    Health health;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        
        //SetDestructionType();
    }

    public void SetDestructionType()
    {   if (health != null)
        {
            if (gameObject.tag == "Building") health.OnDestroyed = DestroyBuilding;
            else if (gameObject.tag == "Enemy") health.OnDestroyed = DestroyEnemy;
            else health.OnDestroyed = DestroyDefault;
        }
    }

 
    public void DestroyDefault(GameObject target, GameObject damager)
    {
        Debug.Log("DestroyDefault");
        animator.SetTrigger("Default Destruction");

        void Particles()
        {
            Instantiate(DefaultExplosionParticles, transform.position, transform.rotation);
        }
    }

    public void DestroyBuilding(GameObject target, GameObject damager)
    {
        Debug.Log("DestroyBuilding");
        animator.SetTrigger("Building Destruction");

        void Particles()
        {
            Instantiate(DefaultExplosionParticles, transform.position, transform.rotation);
        }
    }

    public void DestroyEnemy(GameObject target, GameObject damager)
    {
        Debug.Log("DestroyEnemy");
        animator.SetTrigger("Enemy Destruction");

        void Particles()
        {
            Instantiate(DefaultExplosionParticles, transform.position, transform.rotation);
        }
    }

    //runs from keyevent on animation
    public void Cleanup() { health.Cleanup(); }
}
