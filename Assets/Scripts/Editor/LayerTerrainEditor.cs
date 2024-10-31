using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LayerTerrain))]
public class LayerTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        if (DrawDefaultInspector())
        {
            LayerTerrain script = (LayerTerrain)target;
            if (script.autoUpdate)
            {
                Debug.Log("Editor script sees change in Layer Terrain");
                script.runResearchMapGen();
            }
        }


        

        if (GUILayout.Button("Serialize Params to JSON"))
        {
            LayerTerrain script = (LayerTerrain)target;
            script.SerializeNoiseParamsToJson();
        }
        if (GUILayout.Button("Load Params From JSON"))
        {
            LayerTerrain script = (LayerTerrain)target;
            script.LoadNoiseParamsFromJson();
        }

        if (GUILayout.Button("Generate"))
        {
            LayerTerrain script = (LayerTerrain)target;
            //ResearchMapGenerator script1 = (ResearchMapGenerator)target;
            script.runResearchMapGen();
        }
    }
}
