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
                script.GenPSD();
            }
        }

        if (GUILayout.Button("Spawn Flora"))
        {
            script.DestroyExisting();
            script.GenPSD();
        }

    }
}
