using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Flora))]
public class FloraEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Flora script = (Flora)target;
        
        if (DrawDefaultInspector()) // runs on any inspector change
        {
            if (script.EditorAutoUpdate)
            {
                script.DestroyExisting();
                script.GenPSD(script.sampleRegionSize/2);
            }
        }

        if (GUILayout.Button("Spawn Flora"))
        {
            //script.DestroyExisting();
            script.GenPSD(script.sampleRegionSize / 2);
        }
        if (GUILayout.Button("Spawn Grass"))
        {
            //script.DestroyExisting();
            script.GenGrass();
        }
        if (GUILayout.Button("Destroy All"))
        {
            script.DestroyExisting();
        }

    }
}
