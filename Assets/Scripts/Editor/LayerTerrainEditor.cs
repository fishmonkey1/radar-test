using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LayerTerrain))]
public class LayerTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LayerTerrain script = (LayerTerrain)target;
        
        if (DrawDefaultInspector()) // runs on any inspector change
        {
            bool paramsLoad = false;

            for (int i = 0; i < script.elevationLayers.NoisePairs.Count; i++)
            {
                MapNoisePair pair = script.elevationLayers.NoisePairs[i];
                if (pair.LoadFromJSON)
                {
                    paramsLoad = true;
                    pair.LoadFromJSON = false;
                    Debug.Log("trying to load params from editor script");
                    script.LoadNoiseParamsFromJson(pair);
                    break;
                }
                if (pair.SaveToJSON)
                {
                    pair.SaveToJSON = false;
                    Debug.Log("trying to save params from editor script");
                    script.SerializeNoiseParamsToJson(pair);
                    break;
                }
            }

            if (paramsLoad && script.autoUpdate)
            {
                script.runMapGen();
            }
            else if (script.autoUpdate)
            {
                script.runMapGen();
            }
        }


        if (GUILayout.Button("Generate"))
        {
            script.runMapGen();
        }

        /*if (GUILayout.Button("Serialize Params to JSON"))
        {
            script.SerializeNoiseParamsToJson();
        }
        if (GUILayout.Button("Load Params From JSON"))
        {
            script.LoadNoiseParamsFromJson();
        }*/

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
