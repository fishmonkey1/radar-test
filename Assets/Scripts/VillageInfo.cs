using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VillageInfo
{
    public List<Node> VillageNodes; //All of the nodes that belong to this village
    public Node VillageCenter; //One node picked to be the epicenter of the village, roughly
    public static Dictionary<Node, VillageInfo> Villages = new();

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
