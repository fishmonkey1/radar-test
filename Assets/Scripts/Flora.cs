using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Flora : MonoBehaviour
{
    [SerializeField] List<GameObject> floraPrefabs = new List<GameObject>();

    [SerializeField] [Range(4f, 30f)] int minRadius = 6;
    [SerializeField] [Range(4f, 30f)] int maxRadius = 20;
    [SerializeField] Vector2 sampleRegionSize = Vector2.one;
    [SerializeField] int numSamplesBeforeRejection = 30;
    [SerializeField] float floraPrefabsScaleMin;
    [SerializeField] float floraPrefabsScaleMax;

    List<Vector2> points = new List<Vector2>();

    LayerTerrain lt;

    public bool EditorAutoUpdate = true;

    private void Awake()
    {
        DestroyExisting();
    }
    private void Start()
    {
        GenPSD();
    }

    List<Vector3> gizmo = new List<Vector3>();

    public void GenPSD()
    {
        points = PoissonDiscSampling.GeneratePoints(minRadius, maxRadius, sampleRegionSize, numSamplesBeforeRejection);
        foreach (Vector2 point in points)
        {
            int index = Random.Range(0, floraPrefabs.Count); // select random prefab

            RaycastHit hit;
            Ray ray = new Ray(new Vector3(point.x, 300, point.y), Vector3.down);
            if (Physics.Raycast(ray, out hit, 500))
            {
                Debug.Log("Hit point: " + hit.point);
                gizmo.Add(hit.point);
            }

            GameObject tree = Instantiate(floraPrefabs[index], this.transform.InverseTransformPoint(hit.point), Quaternion.identity, this.transform) as GameObject;
            tree.transform.Rotate(0, Random.Range(0, 360), 0);

            tree.transform.localScale = Vector3.one * Random.Range(tree.transform.localScale.x * floraPrefabsScaleMin, tree.transform.localScale.x * floraPrefabsScaleMax); // add in some scaling randomness for size diffs
            
        }
    }

    public void DestroyExisting()
    {
        // destroy existing
        GameObject[] clones = GameObject.FindGameObjectsWithTag("Flora");
        foreach (var clone in clones)
        {
            DestroyImmediate(clone);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(sampleRegionSize / 2, sampleRegionSize);
        if (gizmo != null)
        {
            foreach (Vector3 point in gizmo)
            {
                Gizmos.DrawSphere(point, 1);
            }
        }
    }





}
