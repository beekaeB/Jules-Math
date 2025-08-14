using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents a complete weapon, assembled from various parts.
/// Its final stats are calculated from its base stats combined with modifiers from all its parts.
/// </summary>
[System.Serializable]
public class Weapon
{
    public string weaponName;
    public Manufacturer manufacturer;
    public Rarity rarity;
    public Element element;

    // The components that make up this weapon
    public List<WeaponPart> parts;

    // Base stats for this weapon type, before part modifiers
    [Header("Base Stats")]
    public float baseDamage = 10f;
    public float baseFireRate = 5f;
    public int baseMagazineSize = 20;
    public float baseReloadTime = 2f;

    [Header("Gimmick Base Stats")]
    [Tooltip("The starting spread angle of the weapon.")]
    public float baseSpread = 5f;
    [Tooltip("How much accuracy increases per second of firing (for Hyperion).")]
    public float baseAccuracyRamp = 1f;
    [Tooltip("How much the fire rate increases per second of firing (for Vladof).")]
    public float baseFireRateRamp = 0.5f;


    #region Calculated Stats
    // These properties calculate the final stats on the fly by summing the base stat
    // and all modifiers from the attached parts.
    public float Damage => baseDamage + parts.Sum(p => p.damageModifier);
    public float FireRate => baseFireRate + parts.Sum(p => p.fireRateModifier);
    public int MagazineSize => baseMagazineSize + parts.Sum(p => p.magazineSizeModifier);
    public float ReloadTime => Mathf.Max(0.1f, baseReloadTime - parts.Sum(p => p.reloadTimeModifier)); // Reload time gets shorter with modifiers

    // Gimmick stats
    public float AccuracyRamp => baseAccuracyRamp + parts.Sum(p => p.accuracyBonusModifier);
    public float FireRateRamp => baseFireRateRamp + parts.Sum(p => p.fireRateRampModifier);
    #endregion

    public Weapon()
    {
        parts = new List<WeaponPart>();
    }

    /// <summary>
    /// Generates a descriptive string for the weapon's stats.
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<b>{weaponName}</b> ({rarity} {manufacturer})");
        sb.AppendLine($"Element: {element}");
        sb.AppendLine($"Damage: {Damage}");
        sb.AppendLine($"Fire Rate: {FireRate}");
        sb.AppendLine($"Magazine Size: {MagazineSize}");
        return sb.ToString();
    }
}
