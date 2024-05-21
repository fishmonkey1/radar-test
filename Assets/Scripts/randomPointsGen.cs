using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class randomPointsGen : MonoBehaviour
{
   
   public static Vector3 randomPoint(Vector3 Start_Point, float Radius)
    {
        Vector3 Dir = Random.insideUnitSphere * Radius;
        Dir += Start_Point;
        NavMeshHit Hit_;
        Vector3 Final_Pos = Vector3.zero;

        int areaMask = 1; //what mesh we're on? i forgor

        if (NavMesh.SamplePosition(Dir, out Hit_, Radius, areaMask))
        {
            Final_Pos = Hit_.position;
        }
        return Final_Pos;
    }
}
