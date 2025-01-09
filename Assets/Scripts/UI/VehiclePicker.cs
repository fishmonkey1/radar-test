using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
namespace HorniTank
{
    /// <summary>
    /// Handles picking currently existing vehicles over the network, or requesting a new vehicle to crew.
    /// </summary>
    public class VehiclePicker : NetworkBehaviour
    {
        /// <summary>
        /// Synchronizes all of the vehicles that have been requested by players on this team, up to the vehicle limit defined by TeamInfo.
        /// </summary>
        public VehicleSpawnData AllVehicles = new();
        /// <summary>
        /// The team that this picker is for, which is set by the Lobby script after the TeamPicker has been done.
        /// </summary>
        public TeamInfo LinkedTeam;

        /// <summary>
        /// When a player asks for a new vehicle or joins a crew, this dictionary gets updated over the network. When all players have readied up, this dictionary is passed to <see cref="TankRoomManager"/> so each vehicle can be spawned
        /// </summary>
        public Dictionary<VehicleData, ProfileGroup> VehicleToProfiles = new();

        /// <summary>
        /// Assign the panel from the UI prefab here so buttons can be assigned for selecting which vehicle crew to join. This is networked and works with the <see cref="TankRoomManager"/> to coordinate which vehicles to spawn when the game begins.
        /// </summary>
        [SerializeField]
        RectTransform JoinCrewButtonPanel;

        /// <summary>
        /// Assign the panel from the UI prefab here so buttons can be made for each type of Vehicle prefab that has been made.
        /// </summary>
        [SerializeField]
        RectTransform VehiclePrefabPanel;

        /// <summary>
        /// A plain button prefab for linking up to this script
        /// </summary>
        [SerializeField]
        GameObject ButtonPrefab;

        /// <summary>
        /// For hiding or displaying the window that the "Create New Vehicle" button toggles. This should be the root of the window instead of the content panel like <see cref="VehiclePrefabPanel"/>
        /// </summary>
        [SerializeField]
        GameObject VehiclePrefabPicker;

        List<GameObject> CrewedVehiclesButtons = new(); //I'm gonna use this to update buttons when things change. Use it kinda like object pooling. Shhh, Mommy's cooking

        private void Start()
        {
            CreatePrefabPickerButtons();
        }

        public void CreatePrefabPickerButtons()
        {
            PlayerProfile localProfile = NetworkClient.localPlayer.GetComponent<ProfileHolder>().Profile;
            foreach (VehicleData vehicle in AllVehicles.VehicleData)
            {
                //Make button
                GameObject ButtonInstance = GameObject.Instantiate(ButtonPrefab, VehiclePrefabPanel);
                Button ButtonScript = ButtonInstance.GetComponent<Button>();
                ButtonScript.onClick.AddListener(() => RequestNewVehicle(vehicle, localProfile));
            }
        }

        public void DrawCrewedVehiclesButtons()
        {
            //This makes a button for each vehicle a player has requested to use on this team
            //This function also updates all of the buttons when the crew number changes

            //Make a button that requests a new vehicle to use on this team. It opens the VehiclePrefabPicker window
            GameObject CreateVehicleButton = GameObject.Instantiate(ButtonPrefab, JoinCrewButtonPanel);
            Button CreateVehicle = CreateVehicleButton.GetComponent<Button>();
            CreateVehicle.onClick.AddListener(() => ShowPrefabPicker()); //Open the prefab picker when you click on the add new vehicle button

            PlayerProfile localProfile = NetworkClient.localPlayer.GetComponent<ProfileHolder>().Profile;

            if (VehicleToProfiles.Keys.Count > CrewedVehiclesButtons.Count)
            { //We've added more elements than we have buttons to render, let's fix that
                for (int i = (VehicleToProfiles.Keys.Count - 1) - (CrewedVehiclesButtons.Count - 1); i < VehicleToProfiles.Keys.Count; i++)
                { //Go through and add buttons for all of the missing ones
                    GameObject JoinButton = GameObject.Instantiate(ButtonPrefab, JoinCrewButtonPanel);
                    CrewedVehiclesButtons.Add(JoinButton);
                }
            }

            int index = 0;
            foreach(VehicleData vehicle in VehicleToProfiles.Keys)
            { //Go through each vehicle that has been requested and draw a button for them
                GameObject JoinVehicleButton = CrewedVehiclesButtons[index]; //Nab the button out of our list
                Button JoinVehicle = JoinVehicleButton.GetComponent<Button>();
                JoinVehicle.onClick.AddListener(() => RequestCrewVehicle(vehicle, localProfile)); //Request crewing a vehicle from the server
                TextMeshProUGUI buttonText = JoinVehicleButton.GetComponentInChildren<TextMeshProUGUI>();
                //Format the button text to read "Join Tank (1/4)" where Tank is vehicle's name, and the () contains current players out of max players
                buttonText.text = $"Join {vehicle.VehicleName} (1/{vehicle.MaxPlayers})";
                index++;
            }

            if (VehicleToProfiles.Keys.Count < CrewedVehiclesButtons.Count)
            { //We have more buttons than we need, let's disable the extras
                for (int i = (CrewedVehiclesButtons.Count - 1) - (VehicleToProfiles.Keys.Count - 1); i < CrewedVehiclesButtons.Count; i++)
                {
                    GameObject button = CrewedVehiclesButtons[i];
                    button.SetActive(false); //Turn the button off
                }
            }

        }

        void ShowPrefabPicker()
        {
            VehiclePrefabPicker.SetActive(true);
        }

        [Command(requiresAuthority = false)]
        public void RequestCrewVehicle(VehicleData vehicle, PlayerProfile requestingPlayer)
        {
            //A player asks to join the crew on a vehicle
            //If they were in a crew before, we need to remove them
            //Add them to the crew, tell the PlayerProfile about it as well
            //Then let the lobby know we have a vehicle picked and move on to the RolePicker like we were doing before
        }

        [Command(requiresAuthority = false)]
        public void RequestNewVehicle(VehicleData vehicle, PlayerProfile requestingPlayer)
        {
            //Ask the server for a new vehicle to crew
            //Server checks if making a new vehicle would put the team over the vehicle budget for the map
            //If all checks pass, server confirms the new vehicle and the player requests crewing it

            //Vicky note: For now, there are no server checks until the map info class is written and wired up
            //Assume that any requests for new vehicles are valid
            ProfileGroup newGroup = new ProfileGroup();
            newGroup.Group.Add(requestingPlayer);
            VehicleToProfiles.Add(vehicle, newGroup); //Stick the info into the dictionary. Now we need to update all the players
        }

        [ClientRpc]
        public void VehicleRequested(VehicleData vehicle, PlayerProfile requestingPlayer)
        {
            if (isServer)
                return; //Prevent the host from duplicating efforts
            //And we just duplicate the server stuff here for now
            ProfileGroup newGroup = new ProfileGroup();
            newGroup.Group.Add(requestingPlayer);
            VehicleToProfiles.Add(vehicle, newGroup);
        }

    }
}
