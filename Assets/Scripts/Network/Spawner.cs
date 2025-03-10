using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Returned when a system tries to spawn something with VehicleData.
/// TODO: Update this class to include data relevant to spawning enemies.
/// TODO: Migrate some of the enemy spawning logic to this class or a related one
/// </summary>
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

/// <summary>
/// Monobehaviour script that is saved into a Map scene. The Spawner handles local positioning and spawn point selection with a Round Robin spawn approach for now. Points must be flagged as useable again by the programmer, as nothing resets them yet.
/// TODO: Create a system the handles freeing up the spawn points. This will need certain rules like checking when the spawn point is not blocked, or activating after a certain amount of time. Reference the game doc and Trello cards for more thoughts.
/// TODO: Update the Spawner to consider which Team the VehicleData has before selecting a spawn point for them.
/// TODO: Consider creating a method to check all the spawn points for which ones the player could select, allowing for a more Vattlebit Remastered style of spawning the crew.
/// TODO: Another big one to think about if spawn spot selection will be a thing would be limiting those commands to clients with certain Roles selected.
/// </summary>
public class Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private bool _autoFindSpawnPoints = true;
    [SerializeField] private List<VehicleSpawnPosition> _manualSpawnPoints = new List<VehicleSpawnPosition>();
    [SerializeField] private bool _showDebugGizmos = true;

    static Spawner instance;
    /// <summary>
    /// There should only be one graph per gameplay scene.
    /// </summary>
    public static Spawner Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Spawner>();
            }
            return instance;
        }
    }

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

    /// <summary>
    /// Set up the singleton for the Spawner so that scripts within the map can find Spawn Points for whatever they need.
    /// Awake also populates the SpawnPoints list depending on the editor settings.
    /// </summary>
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

    /// <summary>
    /// Populate the SpawnPoints lists based on whether or not the Editor has manual spawn points listed.
    /// </summary>
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

    /// <summary>
    /// Check that there was actually a spawn point placed on the map, otherwise alert the programmer, halt, and catch fire.
    /// </summary>
    private void ValidateSpawnPoints()
    {
        if (_spawnPoints.Count == 0)
        {
            //TODO: Add more programmer bullying to the Spawner
            Debug.LogError("No spawn points found in scene! Silly girl, forgetting to add a SpawnPoint."); //Bad programmer, forgetting to add spawn points.
            enabled = false;
        }
    }

    /// <summary>
    /// Attempt to locate a spawn point, create the local vehicle prefab there, and then return a <see cref="SpawnResult"/> with what the spawner was able to do to create the vehicle.
    /// TODO: Write rules for what criteria the spawn point and vehicle must meet to be valid.
    /// TODO: Decide whether this function should handle enemies as well, or if that should be a separate system.
    /// </summary>
    /// <param name="vehiclePrefab"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Internal function for <see cref="TrySpawnVehicle(GameObject)"/>
    /// TODO: This might be my spot to handle how the spawn is selected according to rules/criteria that its data must match.
    /// </summary>
    /// <param name="foundPoint">The SpawnPoint to be used in TrySpawnVehicle</param>
    /// <returns></returns>
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

    /// <summary>
    /// Mark a SpawnPoint as useable again. Unused for now, as of 3/8/2025
    /// TODO: Write a script that goes on SpawnPoints which handles releasing it on certain triggers. See Trello cards and Dev Doc for more details.
    /// </summary>
    /// <param name="spawnPoint"></param>
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