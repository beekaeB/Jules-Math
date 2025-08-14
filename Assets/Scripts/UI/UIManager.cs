using UnityEngine;
// We will need this for the UI elements like InputField, though we are only writing the backend logic.
// using UnityEngine.UI;

/// <summary>
/// Manages the game's UI, including the state of the Cognitive Forge.
/// This script would be attached to a Canvas in the scene.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // --- Private State ---
    private Weapon weaponInForge;
    private MathProblem currentProblem;
    private int playerTier = 1; // Player's current math difficulty tier
    private bool isForgeActive = false;

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

    #region Public UI Event Handlers

    /// <summary>
    /// Coroutine to handle the entire Cognitive Forge sequence.
    /// The StageManager will wait for this coroutine to finish.
    /// </summary>
    public System.Collections.IEnumerator CognitiveForgeSequence()
    {
        isForgeActive = true;
        Debug.Log("Player has entered the Cognitive Forge. Please select a weapon to upgrade.");

        // In a real game, the UIManager would listen for a button click to select a weapon.
        // For this simulation, we'll auto-select the player's currently equipped weapon.
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.equippedWeapon != null)
        {
            SelectWeaponForForge(player.equippedWeapon);
        }
        else
        {
            Debug.LogWarning("Could not find player or equipped weapon to upgrade.");
            isForgeActive = false; // Exit if no weapon
        }

        // Wait here until the player has completed the forge interaction
        yield return new WaitUntil(() => !isForgeActive);

        Debug.Log("Cognitive Forge sequence complete.");
    }

    /// <summary>
    /// Called when the player selects a weapon to place in the forge.
    /// </summary>
    public void SelectWeaponForForge(Weapon weapon)
    {
        weaponInForge = weapon;
        currentProblem = MathChallengeManager.Instance.GenerateMathProblem(playerTier);

        Debug.Log($"Weapon '{weapon.weaponName}' placed in forge.");
        Debug.Log($"Math Problem: {currentProblem.question}");

        // In a real UI, this would display the weapon and the math problem.
    }

    /// <summary>
    /// Called when the player submits their answer via an input field and button.
    /// </summary>
    public void OnSubmitAnswer(string submittedAnswerString)
    {
        if (weaponInForge == null || currentProblem == null) return;

        if (float.TryParse(submittedAnswerString, out float submittedAnswer))
        {
            bool isCorrect = MathChallengeManager.Instance.CheckAnswer(submittedAnswer, currentProblem.answer);
            if (isCorrect)
            {
                Debug.Log("Correct! Please choose your upgrade.");
                // In a real UI, this would show the upgrade option buttons.
                // For now, we can just automatically apply one for testing.
                OnSelectUpgrade(UpgradeType.RerollPart);
            }
            else
            {
                Debug.Log("Incorrect answer. The weapon was not upgraded.");
                ExitCognitiveForge();
            }
        }
        else
        {
            Debug.LogWarning("Invalid answer format. Please enter a number.");
        }
    }

    /// <summary>
    /// Called when the player clicks one of the upgrade choice buttons.
    /// </summary>
    public void OnSelectUpgrade(UpgradeType upgradeType)
    {
        Debug.Log($"Upgrade '{upgradeType}' selected.");
        MathChallengeManager.Instance.ApplyUpgrade(weaponInForge, upgradeType);

        // The player's math tier could increase after a successful upgrade.
        playerTier++;

        ExitCognitiveForge();
    }

    /// <summary>
    /// Called to exit the forge and return to gameplay.
    /// </summary>
    public void ExitCognitiveForge()
    {
        Debug.Log("Exiting Cognitive Forge.");
        weaponInForge = null;
        currentProblem = null;
        isForgeActive = false; // This signals the CognitiveForgeSequence coroutine to complete
    }

    #endregion
}
