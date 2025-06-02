using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    // Text UI element displaying the countdown timer
    public TextMeshProUGUI timerText;

    // Current countdown time remaining (in seconds)
    public float remainingTime;

    // Singleton instance for easy access
    public static Timer instance;

    // Reference to the main SinglePlayerScript controlling the game state
    public SinglePlayerScript timer;

    // Flag to indicate if the game is in overtime mode
    private bool overtime = false;

    // Initialize singleton instance
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Update the countdown timer every frame
    void Update()
    {
        // Skip updating if the game is currently frozen
        if (timer.isFrozen)
            return;

        // Only update if the timer is active
        if (timer.pauseTimer)
        {
            // Decrease time
            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
            }
            else
            {
                // Handle time expiration
                if (!overtime)
                {
                    // Try to determine winner
                    if (timer.CheckWinner())
                    {
                        remainingTime = 0;
                    }
                    else
                    {
                        // Start overtime (disable timer display)
                        timerText.enabled = false;
                        overtime = true;
                    }
                }
            }

            // Display the remaining time as whole seconds
            int seconds = Mathf.RoundToInt(remainingTime);
            timerText.text = string.Format("{0}", seconds);
        }
    }

    // Set a new countdown duration
    public void SetTimer(float sec)
    {
        remainingTime = sec;
    }
}
