using UnityEngine;

/// <summary>
/// Marks a GameObject as being detectable by a tank's Radar. All fields are set from the inspector.
/// </summary>
public class RadarTarget : MonoBehaviour
{
    /// <summary>
    /// Handles whether the <see cref="MinimapIconPrefab"/> is displayed on the radar or not.
    /// </summary>
    public bool IsRadarVisible = true; //Set to false if it should not be shown on the radar for some reason
    /// <summary>
    /// Handles keeping the icon on the map permenantly.
    /// </summary>
    public bool IsBuilding = false; //Set to true if this icon should remain on the map at all times.
    /// <summary>
    /// The icon to display for this RadarTarget.
    /// </summary>
    public GameObject MinimapIconPrefab;
    /// <summary>
    /// The color of the <see cref="MinimapIconPrefab"/>
    /// </summary>
    public Color MinimapIconColor;
}
