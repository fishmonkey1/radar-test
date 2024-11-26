using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data class for defining a village/town on the map from its nodes. Contains a list of all nodes that belong to the village, and a single VillageCenter that is the target of navigate orders and so on. Note that there is no implemented way to remove villages
/// </summary>
[System.Serializable]
public class VillageInfo
{
    /// <summary>
    /// All of the nodes that belong to this village
    /// </summary>
    public List<Node> VillageNodes;
    /// <summary>
    /// One node picked to be the epicenter of the village, roughly
    /// </summary>
    public Node VillageCenter;
    /// <summary>
    /// Static cache where all VillageInfo instances live. Key: VillageCenter, Value: instance of VillageInfo
    /// </summary>
    public static Dictionary<Node, VillageInfo> Villages = new();

    /// <summary>
    /// Adds a village to <see cref="Villages"/> for access.
    /// </summary>
    /// <param name="village">The village to add to our static list.</param>
    public static void AddVillage (VillageInfo village)
    {
        if (!Villages.ContainsKey(village.VillageCenter))
        {
            Villages.Add(village.VillageCenter, village);
        }
        else
        {
            Debug.Log("Attempted to add a village that already has its center node registered");
        }
        if (!village.VillageNodes.Contains(village.VillageCenter))
        { //Somebody forgot to put the central node into the list of all nodes, so lets fix that
            village.VillageNodes.Add(village.VillageCenter);
        }
        //Finally make sure that all of the other nodes in the village have their references set to this villageinfo
        foreach (Node n in village.VillageNodes)
        {
            n.village = village; //Set the reference for them
        }
    }


}
