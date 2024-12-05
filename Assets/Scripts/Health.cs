using UnityEngine;
using Mirror;

/// <summary>
/// Networked health tracker for any objects that can be destroyed.
/// </summary>
public class Health : NetworkBehaviour
{
    [SerializeField] float MaxHealth;
    [SyncVar]
    public float CurrentHealth;
    public delegate void OnTargetDestroyed(GameObject target, GameObject damager);
    public OnTargetDestroyed OnDestroyed;

    // Start is called before the first frame update
    void Start()
    {
        CurrentHealth = MaxHealth;
    }

    /// <summary>
    /// Currently unused, as we have nothing in the game that heals anything.
    /// </summary>
    /// <param name="health"></param>
    public void AddHealth(float health)
    { //Just leaving this here even though I doubt we'll be using it anytime soon
        CurrentHealth += health;
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
    }

    /// <summary>
    /// Used by projectiles to deal damage to things with health.
    /// </summary>
    /// <param name="health"></param>
    /// <param name="damager"></param>
    public void RemoveHealth(float health, GameObject damager)
    {
        CurrentHealth -= health;
        if (CurrentHealth <= 0)
        { //This object has been destroyed
            if (OnDestroyed != null) //Call any functions that were listening for this to be boomed
                OnDestroyed(gameObject, damager);
            NetworkServer.Destroy(gameObject);
            GameObject.Destroy(gameObject); //Remove the destroyed thingy
        }
    }
    
}
