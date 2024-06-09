using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] float MaxHealth;
    public float CurrentHealth;
    public delegate void OnTargetDestroyed(GameObject target, GameObject damager);
    public OnTargetDestroyed OnDestroyed;

    // Start is called before the first frame update
    void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public void AddHealth(float health)
    { //Just leaving this here even though I doubt we'll be using it anytime soon
        CurrentHealth += health;
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
    }

    public void RemoveHealth(float health, GameObject damager)
    {
        CurrentHealth -= health;
        if (CurrentHealth <= 0)
        { //This object has been destroyed
            if (OnDestroyed != null) //Call any functions that were listening for this to be boomed
                OnDestroyed(gameObject, damager);
            GameObject.Destroy(gameObject); //Remove the destroyed thingy
        }
    }
    
}
