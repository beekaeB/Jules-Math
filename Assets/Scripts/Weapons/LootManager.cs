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

        // 1. Determine Rarity, Manufacturer, and Element. For now, we'll pick them randomly.
        // A more advanced system could use weighted probabilities.
        newWeapon.manufacturer = (Manufacturer)Random.Range(0, System.Enum.GetValues(typeof(Manufacturer)).Length);
        newWeapon.rarity = (Rarity)Random.Range(0, System.Enum.GetValues(typeof(Rarity)).Length);
        newWeapon.element = (Element)Random.Range(0, System.Enum.GetValues(typeof(Element)).Length);

        // 2. Select one part from each category and add it to the weapon.
        // We check if the pool is not empty before trying to access it.
        if (barrels.Any()) newWeapon.parts.Add(barrels[Random.Range(0, barrels.Count)]);
        if (grips.Any()) newWeapon.parts.Add(grips[Random.Range(0, grips.Count)]);
        if (magazines.Any()) newWeapon.parts.Add(magazines[Random.Range(0, magazines.Count)]);
        if (sights.Any()) newWeapon.parts.Add(sights[Random.Range(0, sights.Count)]);
        if (stocks.Any()) newWeapon.parts.Add(stocks[Random.Range(0, stocks.Count)]);

        // 3. Generate a procedural name for the weapon.
        newWeapon.weaponName = $"{newWeapon.manufacturer} Repeater"; // Example name

        // 4. Log the details of the generated weapon for debugging.
        Debug.Log("--- Weapon Generated ---" + System.Environment.NewLine + newWeapon.ToString());

        return newWeapon;
    }
}
