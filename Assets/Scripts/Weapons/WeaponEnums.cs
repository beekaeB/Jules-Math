/// <summary>
/// Defines the manufacturer of a weapon, which influences its core gimmick.
/// </summary>
public enum Manufacturer
{
    Jakobs,
    Tediore,
    Hyperion,
    Vladof,
    Torgue,
    Maliwan
}

/// <summary>
/// Defines the rarity tier of a weapon, influencing its power and number of parts.
/// </summary>
public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Cipher
}

/// <summary>
/// Defines the elemental damage type of a weapon.
/// </summary>
public enum Element
{
    Kinetic,    // Effective against unshielded, unarmored targets.
    Incendiary, // Effective against Flesh (red health bars).
    Shock,      // Effective against Shields (blue health bars).
    Corrosive   // Effective against Armor (yellow health bars).
}
