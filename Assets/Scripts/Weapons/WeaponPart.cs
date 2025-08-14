using UnityEngine;

/// <summary>
/// Enum defining the different types of weapon parts.
/// This helps ensure a weapon is generated with one of each essential part.
/// </summary>
public enum PartType
{
    Barrel,
    Grip,
    Magazine,
    Sights,
    Stock
}

/// <summary>
/// A ScriptableObject representing a single component of a weapon (e.g., a barrel, a grip).
/// These are created as assets in the Unity Editor and contain the stat modifiers for that part.
/// </summary>
[CreateAssetMenu(fileName = "New Weapon Part", menuName = "Arithmetica/Create Weapon Part")]
public class WeaponPart : ScriptableObject
{
    [Header("Part Metadata")]
    public string partName = "Default Part";
    public PartType partType;

    [Header("Stat Modifiers")]
    public float damageModifier = 0f;
    public float fireRateModifier = 0f;
    public int magazineSizeModifier = 0;
    // More modifiers like accuracy, reload speed, etc., can be added here.
}
