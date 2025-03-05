// VehicleSpawnPosition.cs
using UnityEngine;

[SelectionBase]
[AddComponentMenu("SpawnSystem/Vehicle Spawn Position")]
public class VehicleSpawnPosition : MonoBehaviour
{
    [Header("Gizmo Settings")]
    [SerializeField] private float _gizmoSize = 1f;
    [SerializeField] private Color _gizmoColor = new Color(0, 1, 1, 0.5f);
    [SerializeField] private bool _showDirectionArrow = true;

    [Header("Spawn Settings")]
    [Tooltip("If true, this spawn point will be ignored by the spawner system")]
    [SerializeField] private bool _disabled = false;

    [Tooltip("Optional label for organization and debugging")]
    [SerializeField] private string _spawnPointLabel = "Spawn Point";

    public bool IsDisabled => _disabled;
    public string SpawnPointLabel => string.IsNullOrEmpty(_spawnPointLabel) ? name : _spawnPointLabel;

    private void OnDrawGizmos()
    {
        if (_disabled) return;

        DrawSpawnPointGizmo();
        if (_showDirectionArrow) DrawDirectionArrow();
    }

    private void DrawSpawnPointGizmo()
    {
        Gizmos.color = _gizmoColor;

        // Draw a semi-transparent cube
        Gizmos.DrawCube(transform.position, Vector3.one * _gizmoSize);

        // Draw wireframe outline
        Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 1f);
        Gizmos.DrawWireCube(transform.position, Vector3.one * _gizmoSize);
    }

    private void DrawDirectionArrow()
    {
        Gizmos.color = Color.yellow;
        Vector3 forward = transform.forward * _gizmoSize * 1.5f;

        // Main arrow line
        Gizmos.DrawLine(transform.position, transform.position + forward);

        // Arrow head
        Vector3 right = transform.right * _gizmoSize * 0.25f;
        Vector3 up = transform.up * _gizmoSize * 0.25f;
        Gizmos.DrawLine(transform.position + forward, transform.position + forward * 0.75f - right);
        Gizmos.DrawLine(transform.position + forward, transform.position + forward * 0.75f + right);
        Gizmos.DrawLine(transform.position + forward, transform.position + forward * 0.75f - up);
        Gizmos.DrawLine(transform.position + forward, transform.position + forward * 0.75f + up);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure gizmo size is always positive
        _gizmoSize = Mathf.Max(0.1f, _gizmoSize);

        // Update name in hierarchy if label is set
        if (!string.IsNullOrEmpty(_spawnPointLabel))
        {
            gameObject.name = $"SpawnPoint [{_spawnPointLabel}]";
        }
    }
#endif
}