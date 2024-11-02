using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GraphToRoads))]
public class DrawGraphRoads : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Draw Roads"))
        {
            GraphToRoads graphToRoads = (GraphToRoads)target;
            graphToRoads.DrawRoads();
        }
        if (GUILayout.Button("Set Nodes To Terrain Height"))
        {
            GraphToRoads graphToRoads = (GraphToRoads)target;
            graphToRoads.NodesToTerrainHeight();
        }
        if (GUILayout.Button("Add All Nodes To Graph"))
        {
            Graph.Instance.AddNodesToGraph();
        }
    }

}
