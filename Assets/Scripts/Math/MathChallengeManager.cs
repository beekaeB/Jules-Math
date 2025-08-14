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
        int a, b, c, ans;
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
            case 4: // Complex Multiplication
                a = Random.Range(11, 20);
                b = Random.Range(11, 20);
                return new MathProblem($"{a} * {b} = ?", a * b);
            case 5: // Simple Division (no remainder)
                b = Random.Range(2, 10);
                ans = Random.Range(2, 10);
                a = b * ans;
                return new MathProblem($"{a} / {b} = ?", ans);
            case 6: // Two-step problem (mult then add)
                a = Random.Range(2, 8);
                b = Random.Range(2, 8);
                c = Random.Range(2, 8);
                return new MathProblem($"({a} * {b}) + {c} = ?", (a * b) + c);
            case 7: // Simple Algebra (solve for x)
                a = Random.Range(2, 5);
                b = Random.Range(1, 5);
                c = Random.Range(1, 5);
                ans = a * b + c;
                return new MathProblem($"{a}x + {c} = {ans}", b);
            case 8: // More complex two-step problem
                a = Random.Range(5, 12);
                b = Random.Range(2, 6);
                c = Random.Range(10, 20);
                return new MathProblem($"({a} * {b}) - {c} = ?", (a * b) - c);
            default:
                // If tier is > 8, default to the hardest problem type.
                if (tier > 8) return GenerateMathProblem(8);
                // Otherwise default to the easiest.
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
