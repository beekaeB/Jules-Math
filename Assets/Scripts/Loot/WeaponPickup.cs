using UnityEngine;

/// <summary>
/// Represents a weapon that has been dropped in the game world.
/// It holds the weapon's data and handles being picked up by the player.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WeaponPickup : MonoBehaviour
{
    // The data for the weapon contained in this pickup.
    public Weapon weaponData;

    // A reference to a sprite renderer to change color based on rarity.
    public SpriteRenderer spriteRenderer;

    /// <summary>
    /// Initializes the pickup with the data of the weapon it represents.
    /// </summary>
    public void Initialize(Weapon weapon)
    {
        this.weaponData = weapon;
        // Optional: Change the pickup's appearance based on the weapon's rarity.
        if (spriteRenderer != null)
        {
            spriteRenderer.color = GetRarityColor(weapon.rarity);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object that entered the trigger is the player.
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log("Player collided with weapon pickup: " + weaponData.weaponName);

            // The PlayerController will handle the logic for equipping the weapon.
            // This method will be implemented in the next step.
            player.EquipWeapon(weaponData);

            // Destroy the pickup object after it has been collected.
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Returns a color associated with a given weapon rarity.
    /// </summary>
    private Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return Color.white;
            case Rarity.Uncommon: return Color.green;
            case Rarity.Rare: return Color.blue;
            case Rarity.Epic: return Color.magenta;
            case Rarity.Cipher: return Color.yellow;
            default: return Color.grey;
        }
    }
}
