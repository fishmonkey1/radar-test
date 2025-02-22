using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Flora))]
public class FloraEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Flora script = (Flora)target;

        float prevScale = script.scale;
        float prevStep = script.step;
        bool prevEditNoise = script.EditNoise;
        
        if (DrawDefaultInspector()) // runs on any inspector change
        {
            if (script.EditNoise == true)
            {
                //script.DestroyExisting();
                //script.GenPSD(script.sampleRegionSize/2);

                // if val change 
                // recalculate underlying noise and display change with color,
                // but don't run PDS and place grass until edit mode exited

                if (prevScale != script.scale)
                {
                    script.CalculateGrassNoise(true);
                    prevScale = script.scale;
                }
                if (prevStep != script.step)
                {
                    script.CalculateGrassNoise(true);
                    prevStep = script.step;
                }
                if (prevEditNoise == false) { script.CalculateGrassNoise(true); prevEditNoise = true; }
                
            } else
            {
                if (prevEditNoise == true )
                {
                    script.CalculateGrassNoise(false, true);
                    script.GenGrass();
                    prevEditNoise = false;
                }
            }
                       
        }


        if (GUILayout.Button("Spawn Flora"))
        {
            //script.DestroyExisting();
            script.GenPDS(script.sampleRegionSize / 2);
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
