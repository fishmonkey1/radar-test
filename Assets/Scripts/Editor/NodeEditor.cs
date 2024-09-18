using NUnit.Framework.Interfaces;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Node))]
public class NodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Node node = (Node)target;
        if (node.NodeBuilding == NodeBuilding.VILLAGE_CENTER)
        {
            if (GUILayout.Button("Create Village Info"))
            {
                VillageInfo.AddVillage(node.village);
            }
        }
    }
}
