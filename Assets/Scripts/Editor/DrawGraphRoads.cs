using System.Collections;
using System.Collections.Generic;
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
    }

}
