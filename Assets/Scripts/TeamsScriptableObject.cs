using HorniTank;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A scriptable object for creating teams as assets instead of using a JSON file for them.
/// </summary>
[CreateAssetMenu(fileName = "Team", menuName = "ScriptableObjects/Teams")]
public class TeamsScriptableObject : ScriptableObject
{
    public TeamInfo Team; //This is basically the entire class.
}
