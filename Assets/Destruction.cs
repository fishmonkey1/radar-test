using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destruction : MonoBehaviour
{

    //MeshCollider meshCollider;
    
    //GameObject building_obj;
    Animator animator;
    [SerializeField] ParticleSystem explosionParticles;

    Health health;

    // Start is called before the first frame update
    void Start()
    {
    animator = GetComponent<Animator>();
    health = GetComponent<Health>();    
    }

    // Update is called once per frame
    void Update()
    {
        if (health.CurrentHealth <= 0)
        {
            animator.SetTrigger("collision");
        }
    }

    public void ExplosionParticles()
    {
        Instantiate(explosionParticles, transform.position, transform.rotation);
    }


    // not doing this, going to use health system
    /*private void OnCollisionEnter(Collision collision)
    {
        // trigger smoke
        

        Debug.Log("BUILDING COLLISIONNNNNNNN");
        // drop into ground
        
        Debug.Log("SHOULD HAVE DROPPED");


        Destroy(this, 10f);
        //Destroy(explosionParticles,10);
    }*/

}
