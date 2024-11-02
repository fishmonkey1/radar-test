using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LayerTerrain))]
public class LayerTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        
        LayerTerrain script = (LayerTerrain)target;
        
        if (DrawDefaultInspector())
        {
            if (script.autoUpdate)
            {
                script.runMapGen();
            }
        }


        if (GUILayout.Button("Generate"))
        {
            script.runMapGen();
        }

        if (GUILayout.Button("Serialize Params to JSON"))
        {
            script.SerializeNoiseParamsToJson();
        }
        if (GUILayout.Button("Load Params From JSON"))
        {
            script.LoadNoiseParamsFromJson();
        }

        if (GUILayout.Button("Serialize Map to JSON"))
        {
            script.SerializeMapToJson();
        }

        if (GUILayout.Button("Load Map From JSON"))
        {
            script.LoadMapFromJson();
        }

    }
}
