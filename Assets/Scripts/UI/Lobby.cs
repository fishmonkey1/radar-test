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

        TeamInfo localPickedTeam;

        private void Start()
        {
            //If we're in cooperative mode, then there is only one team to join. We'll assign the friendly team to ourselves and then move on to the vehicle picker.
            if (GameModes.GetCurrentGameMode() == GameModes.Cooperative)
            {
                localPickedTeam = Teams.GetTeamByName("Friendly"); //In cooperative mode all players are on the Friendly team
                ShowVehiclePicker();
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

    }
}
