using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public List<VehicleSpawnData> AllVehicles = new();
        /// <summary>
        /// The team that this picker is for, which is set by the Lobby script after the TeamPicker has been done.
        /// </summary>
        public TeamInfo LinkedTeam;
    }
}
