using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float CurrentSpeed, MaxSpeed;
    public Vector3 target; //Probably changing this later, so just stubbed for now
    public Color Color; //idk maybe even a material or some shit
    public EnemyType type; //Quickly determine what type of unit this is
    public RadarTarget radarInfo; //This holds the minimap icon and color info
}
