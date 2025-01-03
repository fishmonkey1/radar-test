using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destruction : MonoBehaviour
{

    //MeshCollider meshCollider;
    
    //GameObject building_obj;
    Animator animator;
    [SerializeField] ParticleSystem explosionParticles;

    // Start is called before the first frame update
    void Start()
    {
    animator = GetComponent<Animator>();
    //animator.Play("Base Layer.DropBuilding");
    
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ExplosionParticles()
    {
        Instantiate(explosionParticles, transform.position, transform.rotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // trigger smoke

        Debug.Log("BUILDING COLLISIONNNNNNNN");
        // drop into ground
        animator.SetTrigger("collision");
        Debug.Log("SHOULD HAVE DROPPED");


        Destroy(this, 10f);
        //Destroy(explosionParticles,10);
    }

}