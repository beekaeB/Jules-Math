using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the procedural generation of weapons.
/// This class holds pools of weapon parts and assembles them into complete weapons.
/// </summary>
public class LootManager : MonoBehaviour
{
    // Singleton instance for easy access
    public static LootManager Instance { get; private set; }

    [Header("Weapon Part Pools")]
    [Tooltip("All possible barrel parts that can be used for generation.")]
    public List<WeaponPart> barrels;
    [Tooltip("All possible grip parts that can be used for generation.")]
    public List<WeaponPart> grips;
    [Tooltip("All possible magazine parts that can be used for generation.")]
    public List<WeaponPart> magazines;
    [Tooltip("All possible sight parts that can be used for generation.")]
    public List<WeaponPart> sights;
    [Tooltip("All possible stock parts that can be used for generation.")]
    public List<WeaponPart> stocks;

    void Awake()
    {
        // Standard singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Generates a new weapon with randomized characteristics and parts.
    /// </summary>
    /// <returns>A newly created Weapon object.</returns>
    public Weapon GenerateWeapon()
    {
        Weapon newWeapon = new Weapon();

        // 1. Determine Rarity, Manufacturer, and Element.
        newWeapon.manufacturer = (Manufacturer)Random.Range(0, System.Enum.GetValues(typeof(Manufacturer)).Length);
        newWeapon.rarity = (Rarity)Random.Range(0, System.Enum.GetValues(typeof(Rarity)).Length);
        newWeapon.element = (Element)Random.Range(0, System.Enum.GetValues(typeof(Element)).Length);

        // 2. Determine the number of parts based on rarity.
        int numParts = 2; // Common
        switch (newWeapon.rarity)
        {
            case Rarity.Uncommon: numParts = 3; break;
            case Rarity.Rare: numParts = 4; break;
            case Rarity.Epic: numParts = 5; break;
            case Rarity.Cipher: numParts = 5; break; // Cipher gets 5 parts and maybe other bonuses later
        }

        // 3. Create a list of all possible part types and shuffle it.
        List<PartType> availablePartTypes = System.Enum.GetValues(typeof(PartType)).Cast<PartType>().ToList();
        availablePartTypes = availablePartTypes.OrderBy(x => Random.value).ToList(); // Simple shuffle

        // 4. Add the determined number of random parts to the weapon.
        for (int i = 0; i < numParts && i < availablePartTypes.Count; i++)
        {
            PartType partType = availablePartTypes[i];
            var pool = GetPartPool(partType);
            if (pool != null && pool.Any())
            {
                newWeapon.parts.Add(pool[Random.Range(0, pool.Count)]);
            }
        }

        // 5. Generate a procedural name for the weapon.
        newWeapon.weaponName = $"{newWeapon.rarity} {newWeapon.manufacturer} Repeater";

        // 6. Log the details of the generated weapon for debugging.
        Debug.Log("--- Weapon Generated ---" + System.Environment.NewLine + newWeapon.ToString());

        return newWeapon;
    }

    /// <summary>
    /// Helper method to get the correct part pool from a given PartType.
    /// </summary>
    private System.Collections.Generic.List<WeaponPart> GetPartPool(PartType partType)
    {
        switch (partType)
        {
            case PartType.Barrel: return barrels;
            case PartType.Grip: return grips;
            case PartType.Magazine: return magazines;
            case PartType.Sights: return sights;
            case PartType.Stock: return stocks;
            default: return null;
        }
    }
}
