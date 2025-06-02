using UnityEngine;
using TMPro;

/// <summary>
/// Handles the in-game countdown timer for the multiplayer match.
/// Works with MultiplayerManager1 to trigger overtime or end the game when time runs out.
/// </summary>
public class TimerMultiplayer : MonoBehaviour
{
    public TextMeshProUGUI timerText;           // UI text element displaying the countdown
    public float remainingTime;                 // Remaining time in seconds
    public static TimerMultiplayer instance;    // Singleton reference
    public MultiplayerManager1 timer;           // Reference to main game manager
    private bool overtime = false;              // Flag to prevent triggering overtime multiple times

    private void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
        }
    }

    void Update()
    {
        // Stop countdown if the game is frozen (e.g., goal scored, round reset)
        if (timer.isFrozen)
            return;

        // Only update timer if it's currently running
        if (timer.pauseTimer)
        {
            // Count down
            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
            }
            else
            {
                // Time has run out
                if (!overtime)
                {
                    // Determine winner or start overtime
                    if (timer.CheckWinner())
                    {
                        remainingTime = 0;
                    }
                    else
                    {
                        timerText.enabled = false; // Hide timer in overtime
                        overtime = true;
                    }
                }
            }

            // Update timer display (rounded to whole seconds)
            int seconds = Mathf.RoundToInt(remainingTime);
            timerText.text = string.Format("{0}", seconds);
        }
    }
}
