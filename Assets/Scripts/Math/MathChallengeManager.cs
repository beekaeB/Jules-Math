using UnityEngine;
using System.Linq;

/// <summary>
/// Manages the generation of math problems and the application of weapon upgrades.
/// </summary>
public class MathChallengeManager : MonoBehaviour
{
    public static MathChallengeManager Instance { get; private set; }

    void Awake()
    {
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
    /// Generates a math problem based on a given difficulty tier.
    /// </summary>
    /// <param name="tier">The difficulty tier (1-8). Higher is harder.</param>
    public MathProblem GenerateMathProblem(int tier)
    {
        int a, b;
        switch (tier)
        {
            case 1: // Simple Addition
                a = Random.Range(1, 10);
                b = Random.Range(1, 10);
                return new MathProblem($"{a} + {b} = ?", a + b);
            case 2: // Simple Subtraction
                a = Random.Range(5, 15);
                b = Random.Range(1, a);
                return new MathProblem($"{a} - {b} = ?", a - b);
            case 3: // Simple Multiplication
                a = Random.Range(2, 10);
                b = Random.Range(2, 10);
                return new MathProblem($"{a} * {b} = ?", a * b);
            // Higher tiers can be added here, e.g., division, multiple operations, etc.
            default:
                // Default to tier 1 for any unspecified tiers
                return GenerateMathProblem(1);
        }
    }

    /// <summary>
    /// Checks if the submitted answer is correct.
    /// </summary>
    public bool CheckAnswer(float submittedAnswer, float correctAnswer)
    {
        // Use a small tolerance for floating point comparisons
        return Mathf.Abs(submittedAnswer - correctAnswer) < 0.01f;
    }

    /// <summary>
    /// Applies a specified upgrade to a given weapon.
    /// </summary>
    public void ApplyUpgrade(Weapon weapon, UpgradeType upgradeType)
    {
        if (weapon == null) return;

        switch (upgradeType)
        {
            case UpgradeType.RarityUp:
                // Increase rarity by one tier, if not already at max
                if (weapon.rarity < Rarity.Cipher)
                {
                    weapon.rarity++;
                    Debug.Log($"Weapon rarity increased to {weapon.rarity}");
                }
                break;

            case UpgradeType.RerollPart:
                if (weapon.parts.Any() && LootManager.Instance != null)
                {
                    // Pick a random part to replace
                    int partIndex = Random.Range(0, weapon.parts.Count);
                    WeaponPart oldPart = weapon.parts[partIndex];

                    // Find a new part of the same type
                    WeaponPart newPart = null;
                    var pool = GetPartPool(oldPart.partType);
                    if (pool != null && pool.Any())
                    {
                        newPart = pool[Random.Range(0, pool.Count)];
                    }

                    // Replace the part
                    if (newPart != null)
                    {
                        weapon.parts[partIndex] = newPart;
                        Debug.Log($"Reroll: Replaced '{oldPart.partName}' with '{newPart.partName}'");
                    }
                }
                break;

            case UpgradeType.InfuseElement:
                // Get a list of all possible elements except the current one and Kinetic
                var possibleElements = System.Enum.GetValues(typeof(Element)).Cast<Element>()
                                           .Where(e => e != weapon.element && e != Element.Kinetic).ToList();
                if (possibleElements.Any())
                {
                    weapon.element = possibleElements[Random.Range(0, possibleElements.Count)];
                    Debug.Log($"Weapon element infused to {weapon.element}");
                }
                break;
        }
    }

    /// <summary>
    /// Helper method to get the correct part pool from the LootManager.
    /// </summary>
    private System.Collections.Generic.List<WeaponPart> GetPartPool(PartType partType)
    {
        switch (partType)
        {
            case PartType.Barrel: return LootManager.Instance.barrels;
            case PartType.Grip: return LootManager.Instance.grips;
            case PartType.Magazine: return LootManager.Instance.magazines;
            case PartType.Sights: return LootManager.Instance.sights;
            case PartType.Stock: return LootManager.Instance.stocks;
            default: return null;
        }
    }
}
