// Spawner.cs
using UnityEngine;
using System.Collections.Generic;

public class SpawnResult
{
    public GameObject PrefabToSpawn { get; }
    public GameObject SpawnedVehicle { get; }
    public Transform SpawnPointUsed { get; }
    public bool WasSuccessful { get; }
    public string ErrorMessage { get; }

    public SpawnResult(GameObject prefab, GameObject spawnedObject, Transform spawnPoint, bool success, string error = "")
    {
        PrefabToSpawn = prefab;
        SpawnedVehicle = spawnedObject;
        SpawnPointUsed = spawnPoint;
        WasSuccessful = success;
        ErrorMessage = error;
    }
}

public class Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private bool _autoFindSpawnPoints = true;
    [SerializeField] private List<VehicleSpawnPosition> _manualSpawnPoints = new List<VehicleSpawnPosition>();
    [SerializeField] private bool _showDebugGizmos = true;

    public static Spawner instance;

    private readonly List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
    private int _currentRoundRobinIndex;

    private class SpawnPoint
    {
        public readonly Transform Transform;
        public bool IsAvailable;

        public SpawnPoint(Transform transform)
        {
            Transform = transform;
            IsAvailable = true;
        }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Debug.LogError("More than one Spawner exists on the map! Disabling a spawner...");
            enabled = false;
        }
        InitializeSpawnPoints();
        ValidateSpawnPoints();
    }

    private void InitializeSpawnPoints()
    {
        _spawnPoints.Clear();

        if (_manualSpawnPoints.Count > 0 && !_autoFindSpawnPoints)
        {
            foreach (var pos in _manualSpawnPoints)
            {
                if (pos != null)
                    _spawnPoints.Add(new SpawnPoint(pos.transform));
            }
        }
        else
        {
            var foundPoints = FindObjectsByType<VehicleSpawnPosition>(
                FindObjectsSortMode.None);

            foreach (var point in foundPoints)
            {
                _spawnPoints.Add(new SpawnPoint(point.transform));
            }
        }
    }

    private void ValidateSpawnPoints()
    {
        if (_spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points found in scene!");
            enabled = false;
        }
    }

    public SpawnResult TrySpawnVehicle(GameObject vehiclePrefab)
    {
        if (vehiclePrefab == null)
        {
            return new SpawnResult(
                null,
                null,
                null,
                false,
                "Cannot spawn vehicle - prefab is null!");
        }

        if (!TryGetNextAvailableSpawnPoint(out var spawnPoint))
        { //There are no other spawn points to use, so send back a message refusing to spawn the vehicle
            return new SpawnResult(
                vehiclePrefab,
                null,
                null,
                false,
                "No available spawn points!");
        }

        var spawnedVehicle = Instantiate(
            vehiclePrefab,
            spawnPoint.Transform.position,
            spawnPoint.Transform.rotation);

        spawnPoint.IsAvailable = false;

        return new SpawnResult(
            vehiclePrefab,
            spawnedVehicle,
            spawnPoint.Transform,
            true,
            $"Successfully spawned {vehiclePrefab.name} at {spawnPoint.Transform.name}");
    }

    private bool TryGetNextAvailableSpawnPoint(out SpawnPoint foundPoint)
    {
        int attempts = 0;
        int startIndex = _currentRoundRobinIndex;

        do
        {
            var currentPoint = _spawnPoints[_currentRoundRobinIndex];
            _currentRoundRobinIndex = (_currentRoundRobinIndex + 1) % _spawnPoints.Count;

            if (currentPoint.IsAvailable)
            {
                foundPoint = currentPoint;
                return true;
            }

            attempts++;
        }
        while (attempts < _spawnPoints.Count);

        foundPoint = null;
        return false;
    }

    public void ReleaseSpawnPoint(Transform spawnPoint)
    {
        foreach (var point in _spawnPoints)
        {
            if (point.Transform == spawnPoint)
            {
                point.IsAvailable = true;
                return;
            }
        }

        Debug.LogWarning($"Tried to release unknown spawn point: {spawnPoint.name}");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_showDebugGizmos) return;

        foreach (var point in _spawnPoints)
        {
            if (point.Transform == null) continue;

            Gizmos.color = point.IsAvailable ? Color.green : Color.red;
            Gizmos.DrawWireCube(point.Transform.position + Vector3.up, Vector3.one);
            UnityEditor.Handles.Label(
                point.Transform.position + Vector3.up * 2,
                point.IsAvailable ? "Available" : "Occupied");
        }
    }
#endif
}