using System.Collections.Generic;

[System.Serializable]
public class VillageInfo
{
    public List<Node> VillageNodes; //All of the nodes that belong to this village
    public Node VillageCenter; //One node picked to be the epicenter of the village, roughly
    public static Dictionary<Node, VillageInfo> Villages = new();

    public VillageInfo()
    {
        Villages.Add(VillageCenter, this);
    }
}
