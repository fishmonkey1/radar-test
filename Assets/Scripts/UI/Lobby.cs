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
        RectTransform RolePickerWindow; //The UI transform of the window for picking roles.
        [SerializeField]
        RectTransform TeamPickerWindow; //The UI transform of the window for picking your team.
        [SerializeField]
        RectTransform VehiclePickerWindow; //The UI transform of the window for picking which vehicle to crew.

    }
}
