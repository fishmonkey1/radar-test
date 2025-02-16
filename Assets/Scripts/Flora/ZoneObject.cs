using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class ZoneObject
{   
    public GameObject spawnObject;
    [Range(0f, 1f)] public float probability;

}
