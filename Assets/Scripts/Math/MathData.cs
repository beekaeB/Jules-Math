/// <summary>
/// Defines the types of upgrades a player can apply to a weapon in the Cognitive Forge.
/// </summary>
public enum UpgradeType
{
    RarityUp,    // Increases the weapon's rarity tier.
    RerollPart,  // Replaces one random part with another of the same type.
    InfuseElement// Adds or changes the weapon's elemental property.
}

/// <summary>
/// A simple class to hold a math problem and its corresponding answer.
/// </summary>
[System.Serializable]
public class MathProblem
{
    public string question;
    public float answer; // Using float to support division problems later.

    public MathProblem(string question, float answer)
    {
        this.question = question;
        this.answer = answer;
    }
}
