using UnityEngine;
using TMPro;
public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float remainingTime;
    public static Timer instance;
    public SinglePlayerScript timer;
    private bool overtime = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Update()
    {
        if (timer.isFrozen)
            return;
        if(timer.pauseTimer)
        {
            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
            }
            else
            {
                if(overtime == false)
                {
                    if (timer.CheckWinner())
                    {
                        remainingTime = 0;
                    }
                    else
                    {
                        timerText.enabled = false;
                        overtime = true;
                    }
                }
            }
            int seconds = Mathf.RoundToInt(remainingTime);
            timerText.text = string.Format("{00}", seconds);
        }
    }
    public void SetTimer(float sec)
    {
        remainingTime = sec;
    }
}
