using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;


/// Manages multiplayer online game logic.
/// Handles state transitions such as countdown, goal scoring, game start, and overtime.
public class MultiplayerManager1 : MonoBehaviour
{
    public static MultiplayerManager1 instance;

    // UI elements
    public GameObject goImage, oneImage, twoImage, threeImage;
    public GameObject ball;
    public Image playerLeft, playerRight, leftShoeImage, rightShoeImage;
    public Image winnerImage, playerRightWinner, playerLeftWinner;

    // Player and ball references
    public Shoe leftShoe, rightShoe;
    public Transform ballPos;
    public Rigidbody2D ballMove, rbLeft, rbRight;

    // Player starting positions
    private RectTransform rectTransformLeft, rectTransformRight;
    private Vector3 spawnBallPosition = new Vector3(0f, 3f, 0);
    private Vector2 spawnPosLeftPlayer, spawnPosRightPlayer;

    // Game state variables
    public float moveSpeed = 5f;
    private int leftGoalsCount = 0, rightGoalsCount = 0;
    public bool isFrozen = false;
    public bool pauseTimer = false;
    private bool overtime = false;
    public bool gameIsgoing = false;
    private bool leftWin = false, rightWin = false, gameFinish = false;
    public bool firstToConnect = false;

    // UI score
    public TextMeshProUGUI rightPlayerScore, leftPlayerScore;

    // Player control references
    public PlayerControllerMultiplayer leftPlayerController;
    public SecondPlayerControllerMultiplayer rightPlayerController;

    public TimerMultiplayer timer;

    // Initialization
    void Start()
    {
        Application.runInBackground = true;

        // Hide winner UI
        winnerImage.enabled = false;
        playerRightWinner.enabled = false;
        playerLeftWinner.enabled = false;

        // Hide countdown UI
        goImage.SetActive(false);
        oneImage.SetActive(false);
        twoImage.SetActive(false);
        threeImage.SetActive(false);

        // Set singleton instance
        if (instance == null)
        {
            instance = this;
        }

        // Store spawn positions for players
        rectTransformLeft = playerLeft.GetComponent<RectTransform>();
        rectTransformRight = playerRight.GetComponent<RectTransform>();
        spawnPosLeftPlayer = rectTransformLeft.position;
        spawnPosRightPlayer = rectTransformRight.position;
    }

    void Update()
    {
        // Skip update if game has ended
        if (gameFinish)
        {
            if (leftWin && leftPlayerController.isGrounded)
                rbLeft.linearVelocity = Vector2.up * 5;

            if (rightWin && rightPlayerController.isGrounded)
                rbRight.linearVelocity = Vector2.up * 5;

            return;
        }

        // Wait until both players are connected
        if (!gameIsgoing)
        {
            FreezeGameUI();

            if (!firstToConnect)
            {
                if (ServerScript.instance.SendRequest("check_player_one", "") == true)
                {
                    firstToConnect = true;
                }
            }

            if (ServerScript.instance.SendRequest("check_opponent_connected", "") == true)
            {
                if (firstToConnect)
                    Invoke(nameof(BeginTheGame), 2f);
                else
                    BeginTheGame();
            }
        }

        if (isFrozen) return;

        // Check for goals
        Vector2 targetPos = new Vector2(ballPos.position.x, ballPos.position.y);
        if (IsInLeftGoal(targetPos)) LeftGoalScorred();
        if (IsInRightGoal(targetPos)) RightGoalScorred();
    }

    // Freeze game UI before game start
    private void FreezeGameUI()
    {
        ball.SetActive(false);
        playerRight.enabled = false;
        rightShoeImage.enabled = false;
        isFrozen = true;
    }

    private bool IsInLeftGoal(Vector2 pos) =>
        pos.x > -8.67f && pos.x < -8 && pos.y < -1 && pos.y > -3;

    private bool IsInRightGoal(Vector2 pos) =>
        pos.x > 8f && pos.x < 8.67 && pos.y < -1 && pos.y > -3;

    
    /// Called when both players are connected. Begins countdown and game.

    private void BeginTheGame()
    {
        CancelInvoke(nameof(BeginTheGame));
        Debug.Log("Opponent connected. Starting listener.");

        gameIsgoing = true;
        playerRight.enabled = true;
        rightShoeImage.enabled = true;
        ball.SetActive(true);
        isFrozen = false;

        ServerScript.instance.StartLisener(); // Start receiving messages
        StartCoroutine(StartGame());         // Begin countdown
    }

    
    /// Handles left goal scoring.

    private void LeftGoalScorred()
    {
        leftGoalsCount++;
        leftPlayerScore.text = leftGoalsCount.ToString();
        if (overtime) RightPlayerWin();
        else StartCoroutine(StartGame());
    }

    
    /// Handles right goal scoring.

    private void RightGoalScorred()
    {
        rightGoalsCount++;
        rightPlayerScore.text = rightGoalsCount.ToString();
        if (overtime) LeftPlayerWin();
        else StartCoroutine(StartGame());
    }

    
    /// Restarts round with countdown and resets game objects.

    IEnumerator StartGame()
    {
        isFrozen = true;

        // Start timer if it's the first round
        if (!overtime && !firstToConnect && leftGoalsCount == 0 && rightGoalsCount == 0)
        {
            yield return new WaitForSecondsRealtime(2);
            timer.remainingTime = 60f;
        }

        yield return new WaitForSecondsRealtime(1);

        // Reset positions
        ballPos.position = spawnBallPosition;
        ballMove.linearVelocity = Vector3.zero;
        ballMove.angularVelocity = 0;
        rectTransformLeft.position = spawnPosLeftPlayer;
        rectTransformRight.position = spawnPosRightPlayer;

        // Countdown sequence
        Time.timeScale = 0;
        threeImage.SetActive(true); yield return new WaitForSecondsRealtime(1);
        threeImage.SetActive(false); twoImage.SetActive(true); yield return new WaitForSecondsRealtime(1);
        twoImage.SetActive(false); oneImage.SetActive(true); yield return new WaitForSecondsRealtime(1);
        oneImage.SetActive(false); goImage.SetActive(true);

        // Start game
        Time.timeScale = 1;
        isFrozen = false;
        pauseTimer = true;

        yield return new WaitForSecondsRealtime(1);
        goImage.SetActive(false);
    }

    
    /// Checks for a winner. If tied, activates overtime.

    public bool CheckWinner()
    {
        isFrozen = true;

        if (leftGoalsCount > rightGoalsCount)
        {
            RightPlayerWin();
            return true;
        }
        else if (leftGoalsCount < rightGoalsCount)
        {
            LeftPlayerWin();
            return true;
        }
        else
        {
            Debug.Log("Overtime");
            overtime = true;
            StartCoroutine(StartGame());
            return false;
        }
    }

    
    /// Triggers win state for left player.

    private void LeftPlayerWin()
    {
        ball.SetActive(false);
        winnerImage.enabled = true;
        playerLeftWinner.enabled = true;

        Debug.Log("Left Player Win");
        isFrozen = true;
        leftWin = true;
        gameFinish = true;
        gameIsgoing = false;
    }

    
    /// Triggers win state for right player.

    private void RightPlayerWin()
    {
        ball.SetActive(false);
        winnerImage.enabled = true;
        playerRightWinner.enabled = true;

        Debug.Log("Right Player Win");
        isFrozen = true;
        rightWin = true;
        gameFinish = true;
        gameIsgoing = false;
    }

    
    /// Handles proper disconnection and cleanup when app quits.

    private void OnApplicationQuit()
    {
        gameIsgoing = false;
        ServerScript.instance.SendRequest("check_multiplayer_users", "disconnected");

        if (ServerScript.instance.stream != null)
            ServerScript.instance.stream.Close();
        if (ServerScript.instance.client != null)
            ServerScript.instance.client.Close();
    }
}
