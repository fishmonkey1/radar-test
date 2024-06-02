using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadGen))]
public class RoadGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        if (DrawDefaultInspector())
        {
            RoadGen script = (RoadGen)target;
            if (script != null) //the script is null until GenTerrain has called it
            {
                if (script.autoUpdate)
                {
                    script.runMapGen();
                }
            }
            
        }
    }
}
