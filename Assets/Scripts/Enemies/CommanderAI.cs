using UnityEngine;
using System.Collections;

/// <summary>
/// A special enemy type that can capture the player with a tractor beam.
/// Inherits from the base EnemyAI.
/// </summary>
public class CommanderAI : EnemyAI
{
    [Header("Commander Settings")]
    public float beamRange = 3f;
    public float beamDuration = 4f;

    private bool isBeaming = false;
    private PlayerController player;
    private PlayerController capturedPlayer = null;

    protected override void Start()
    {
        base.Start();
        player = FindObjectOfType<PlayerController>();
    }

    protected override IEnumerator WaitInFormationCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(timeInFormation * 0.8f, timeInFormation * 1.2f));

        isBeaming = true;
        Debug.Log("Commander is activating its tractor beam!");

        yield return new WaitForSeconds(beamDuration);

        isBeaming = false;

        // If we didn't capture a player, just dive.
        if (capturedPlayer == null)
        {
            Debug.Log("Commander tractor beam failed. Diving.");
            currentState = EnemyState.DIVING;
        }
    }

    protected override void Update()
    {
        base.Update(); // Handles regular diving movement

        if (isBeaming && player != null && capturedPlayer == null)
        {
            if (Vector2.Distance(transform.position, player.transform.position) < beamRange)
            {
                Debug.Log("Player captured by Commander!");
                player.OnCapture(this.transform);
                capturedPlayer = player;
                isBeaming = false;
            }
        }
    }

    protected override void Die()
    {
        // If this commander had captured a player, rescue them!
        if (capturedPlayer != null)
        {
            capturedPlayer.OnRescue();
        }
        base.Die(); // Call the original Die logic (give points, drop loot, etc.)
    }
}
