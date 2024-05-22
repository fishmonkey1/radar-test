using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Enemy
{
    public GameObject Prefab;
    public string EnemyTag;
    public float Speed;
    public Color Color; //idk maybe even a material or some shit
    public string color; //I'll come back to this
    public Vector3 SpawnPos; //idk just thinking about what we wanna do


    public void Init()
    {

    }


    public void DoSomeShit(string YoMama)
    {
        Debug.Log(YoMama + " is gay");
    }
}
