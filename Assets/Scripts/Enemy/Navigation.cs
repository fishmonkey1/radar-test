using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class Navigation : MonoBehaviour
{
    protected EnemySquad squad; //Keep a reference to the squad for checking orders
    protected Enemy owner; //Keep a reference to the enemy for updating speeds and such
    [SerializeField]public bool active; //If the script is inactive, we ignore all our update stuff
}
