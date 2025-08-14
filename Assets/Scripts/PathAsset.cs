using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A ScriptableObject that defines a path as a series of waypoints.
/// This can be created in the Unity Editor to design enemy entrance paths.
/// </summary>
[CreateAssetMenu(fileName = "New Path Asset", menuName = "Arithmetica/Path Asset")]
public class PathAsset : ScriptableObject
{
    // The list of waypoints that make up the path.
    // These are in local space relative to the path's origin or spawn point.
    public List<Vector3> waypoints = new List<Vector3>();
}
