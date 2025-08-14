using UnityEngine;
using UnityEngine.SceneManagement; // Added for potential future use (e.g., restarting the game)

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Public property to access the score, but can only be set privately within this class
    public int Score { get; private set; } = 0;

    void Awake()
    {
        // Implement the singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // Persist the GameManager across scene loads
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Adds the given amount to the score.
    /// </summary>
    /// <param name="amount">The number of points to add.</param>
    public void AddScore(int amount)
    {
        if (amount > 0)
        {
            Score += amount;
            // Log the score for debugging purposes
            Debug.Log($"Score: {Score}");
        }
    }

    /// <summary>
    /// Handles the game over sequence.
    /// </summary>
    public void GameOver()
    {
        Debug.Log("Game Over! Final Score: " + Score);
        // In the future, this method would be responsible for showing a game over screen
        // and providing options to restart or return to the main menu.
        // Example: SceneManager.LoadScene("GameOverScreen");
    }
}
