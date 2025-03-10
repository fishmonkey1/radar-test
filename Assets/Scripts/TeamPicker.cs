using System.Collections.Generic;
using UnityEngine;
using Mirror;
using HorniTank;

public class TeamPicker : NetworkBehaviour
{
    [SerializeField]
    GameObject buttonPrefab;
    [SerializeField]
    RectTransform ButtonPanel; //This is where we parent the buttons to. This panel is set from the inspector and has a vertical layout group on it

    List<GameObject> buttons = new();
    TeamInfo SelectedTeam = null;

    /// <summary>
    /// This is temporarily in place until I can have information about which teams are availible passed in from the Gamemode data. This is set by InitalizeTeams from the Lobby script.
    /// </summary>
    public List<TeamsScriptableObject> TeamScriptableObjects = new();

    public void InitializeTeams()
    {
        Debug.Log("Initializing Teams list with temporary teams info");
        if (TeamScriptableObjects.Count == 0)
        {
            Debug.Log("You forgot to assign teams to the TeamPicker component!"); //Bully the programmer
        }
        else
        {
            foreach (var team in TeamScriptableObjects)
            {
                Teams.AddTeam(team.Team);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Time to make a button for each of the teams. This needs to read which player teams are valid from a mixture of the map information and the gamemode. For now the game mode is locked to cooperative which only allows one team
        //Since the game mode is locked, the Lobby script will technically skip this script entirely.
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
