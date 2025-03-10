using Mirror;
using UnityEngine;

namespace HorniTank
{
    /// <summary>
    /// This script handles the flow of the UI elements in the Lobby prefab. It allows for team selection, then on to picking or creating a vehicle to crew, then into the rolepicker to choose which role they want to take in that vehicle.
    /// </summary>
    public class Lobby : MonoBehaviour
    {

        [SerializeField]
        RolePicker rolePicker; //Reference to the RolePicker script for piping VehicleSpawnData objects into
        [SerializeField]
        TeamPicker teamPicker; //Reference to the TeamPicker script for determining the scene. TODO: Read team information from the map information
        [SerializeField]
        VehiclePicker vehiclePicker; //Reference for picking or making a new vehicle to use for your selected team

        [SerializeField]
        RectTransform rolePickerWindow; //The UI transform of the window for picking roles.
        [SerializeField]
        RectTransform teamPickerWindow; //The UI transform of the window for picking your team.
        [SerializeField]
        RectTransform vehiclePickerWindow; //The UI transform of the window for picking which vehicle to crew.

        RectTransform currentScreen = null;

        //HACK: This is just some hax0r singleton shit for now so the other UI elements can call ShowMenu functions on the Lobby
        public static Lobby Instance;

        public TeamInfo localPickedTeam { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.LogWarning("You have more than one Lobby instance in your scene, dummy!"); //Bully the programmer for their stupidity
                enabled = false; //Turn the script off so nothing bad happens
            }
        }

        private void Start()
        {
            //If we're in cooperative mode, then there is only one team to join. We'll assign the friendly team to ourselves and then move on to the vehicle picker.
            if (GameModes.GetCurrentGameMode() == GameModes.Cooperative)
            {
                //We need to make sure the Teams section has been populated, which means we need to call the TeamPicker
                teamPicker.InitializeTeams();
                //TODO: Get rid of this hardcoding Vicky, its horrible! You need to put information in the Gamemode to list which teams are options, then move all of this stuff over there
                localPickedTeam = Teams.GetTeamByName("Friendly"); //In cooperative mode all players are on the Friendly team. As long as the Teams.Instance.AllTeams list is populated, this will work. Possible that the teamPicker may not work.
                ShowVehiclePicker();
            }
            //Otherwise we need to have the player select their team first
            else
            {
                //TODO: Finished Team Picker when other gamemodes are implemented
                ShowTeamPicker();
            }
        }

        public void ShowVehiclePicker()
        {
            if (currentScreen != null)
            {
                currentScreen.gameObject.SetActive(false); //Turn that screen off
            }
            vehiclePickerWindow.gameObject.SetActive(true); //Make the vehicle picker active
            currentScreen = vehiclePickerWindow;
        }

        public void ShowTeamPicker()
        {
            //TODO: Add in the team picker stuff once a game mode selection method is made.
        }

        public void ShowRolePicker()
        {
            if (currentScreen != null)
            {
                currentScreen.gameObject.SetActive(false);
            }
            rolePickerWindow.gameObject.SetActive(true);
            currentScreen = rolePickerWindow;
        }

        public void OnGameReady()
        {
            //We fire this after clicking the ready button. We pass the VehiclePicker's data into the TankRoomManager
            TankRoomManager tankRoom = TankRoomManager.singleton;
            tankRoom.SetVehicleSpawnData(vehiclePicker.VehicleToProfiles); //Hand over the vehicle spawn data
        }

    }
}