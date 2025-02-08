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
    Destruction destruction;


    // Start is called before the first frame update
    void Start()
    {
        CurrentHealth = MaxHealth;
        destruction = GetComponent<Destruction>();
        if (destruction != null) destruction.SetDestructionType();
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
        Debug.Log("current: " + CurrentHealth + "  Damage Taken: "+ health+ "  new: "+ (CurrentHealth -= health));
        CurrentHealth -= health;

        if (CurrentHealth <= 0)
        {
            if (OnDestroyed != null)
            {
                OnDestroyed(gameObject, damager); //Call any functions that were listening for this to be boomed
            }
        }
    }

    public void Cleanup() // This is called from Destruction
    {
        // This should probs be handled elsewhere...
        NetworkServer.Destroy(gameObject);
        GameObject.Destroy(gameObject); //Remove the destroyed thingy
    }
}
