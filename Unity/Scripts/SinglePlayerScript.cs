using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

/// Handles the entire single player match flow: score tracking, kickoff sequence, win conditions, and UI control.
public class SinglePlayerScript : MonoBehaviour
{
    public static SinglePlayerScript instance;

    [Header("Countdown UI")]
    public GameObject goImage;
    public GameObject oneImage;
    public GameObject twoImage;
    public GameObject threeImage;

    [Header("Game Entities")]
    public GameObject ball;
    public Image playerLeft;
    public Image playerRight;
    public Image leftShoeImage;
    public Image rightShoeImage;

    [Header("Win UI")]
    public Image winnerImage;
    public Image playerRightWinner;
    public Image playerLeftWinner;
    public Image reload;
    public Image goBack;

    [Header("Shoes")]
    public Shoe leftShoe;
    public Shoe rightShoe;

    [Header("Transforms")]
    public Transform ballPos;
    private RectTransform rectTransformLeft;
    private RectTransform rectTransformRight;

    [Header("Game Settings")]
    public float moveSpeed = 5f;
    public bool isFrozen = false;
    public bool pauseTimer = false;

    [Header("Score")]
    public TextMeshProUGUI rightPlayerScore;
    public TextMeshProUGUI leftPlayerScore;

    [Header("Players")]
    public Rigidbody2D ballMove;
    public Rigidbody2D rbLeft;
    public Rigidbody2D rbRight;
    public PlayerController leftPlayerController;
    public SecondPlayerController rightPlayerController;
    public GameObject ballParticles;

    private int leftGoalsCount = 0;
    private int rightGoalsCount = 0;
    private bool overtime = false;
    private bool leftWin = false;
    private bool rightWin = false;

    private Vector3 spawnBallPosition = new Vector3(0f, 3f, 0);
    private Vector2 spawnPosLeftPlayer;
    private Vector2 spawnPosRightPlayer;

    /// Initializes game state, sets UI defaults, caches references, and starts kickoff.
    private void Awake()
    {
        Application.runInBackground = true;

        reload.enabled = false;
        goBack.enabled = false;
        pauseTimer = false;

        winnerImage.enabled = false;
        playerRightWinner.enabled = false;
        playerLeftWinner.enabled = false;

        goImage.SetActive(false);
        oneImage.SetActive(false);
        twoImage.SetActive(false);
        threeImage.SetActive(false);

        if (instance == null) instance = this;

        rectTransformLeft = playerLeft.GetComponent<RectTransform>();
        rectTransformRight = playerRight.GetComponent<RectTransform>();

        spawnPosLeftPlayer = rectTransformLeft.position;
        spawnPosRightPlayer = rectTransformRight.position;

        StartCoroutine(GameStart());
    }

    /// Constantly checks for win celebration and goal conditions.
    private void Update()
    {
        if (leftWin && leftPlayerController.isGrounded)
            rbLeft.linearVelocity = Vector2.up * 5;

        if (rightWin && rightPlayerController.isGrounded)
            rbRight.linearVelocity = Vector2.up * 5;

        rectTransformLeft.rotation = Quaternion.Euler(0, 0, 0);
        rectTransformRight.rotation = Quaternion.Euler(0, 0, 0);

        if (isFrozen) return;

        Vector2 targetPos = ballPos.position;

        if (targetPos.x > -8.67f && targetPos.x < -8 && targetPos.y < -1 && targetPos.y > -3)
            LeftGoalScorred();

        if (targetPos.x > 8f && targetPos.x < 8.67f && targetPos.y < -1 && targetPos.y > -3)
            RightGoalScorred();
    }

    /// Called when left player scores a goal.
    private void LeftGoalScorred()
    {
        leftGoalsCount++;
        leftPlayerScore.text = leftGoalsCount.ToString();

        if (overtime) RightPlayerWin();
        else StartCoroutine(GameStart());
    }

    /// Called when right player scores a goal.
    private void RightGoalScorred()
    {
        rightGoalsCount++;
        rightPlayerScore.text = rightGoalsCount.ToString();

        if (overtime) LeftPlayerWin();
        else StartCoroutine(GameStart());
    }

    /// Starts a round with a 3-2-1 countdown and resets ball and player positions.
    IEnumerator GameStart()
    {
        isFrozen = true;
        ballPos.position = spawnBallPosition;
        ballMove.linearVelocity = Vector2.zero;
        ballMove.angularVelocity = 0;
        rectTransformLeft.position = spawnPosLeftPlayer;
        rectTransformRight.position = spawnPosRightPlayer;

        Time.timeScale = 0;

        threeImage.SetActive(true);
        yield return new WaitForSecondsRealtime(1);

        threeImage.SetActive(false);
        twoImage.SetActive(true);
        yield return new WaitForSecondsRealtime(1);

        oneImage.SetActive(true);
        twoImage.SetActive(false);
        yield return new WaitForSecondsRealtime(1);

        oneImage.SetActive(false);
        goImage.SetActive(true);
        Time.timeScale = 1;

        isFrozen = false;
        pauseTimer = true;

        yield return new WaitForSecondsRealtime(1);
        goImage.SetActive(false);
    }

    /// Evaluates match result. Returns true if a winner was found, otherwise starts overtime.
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
            StartCoroutine(GameStart());
            return false;
        }
    }

    /// Called when left player loses or right player wins.
    private void LeftPlayerWin()
    {
        ball.SetActive(false);
        winnerImage.enabled = true;
        playerLeftWinner.enabled = true;
        reload.enabled = true;
        goBack.enabled = true;
        Debug.Log("Left Player Win");

        isFrozen = true;
        leftWin = true;
    }

    /// Called when right player loses or left player wins.
    private void RightPlayerWin()
    {
        ball.SetActive(false);
        winnerImage.enabled = true;
        playerRightWinner.enabled = true;
        reload.enabled = true;
        goBack.enabled = true;
        Debug.Log("Right Player Win");

        isFrozen = true;
        rightWin = true;
    }

    /// Reloads the current scene to restart the match.
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// Returns to the game mode selection scene.
    public void GoBack()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 3);
    }

    /// Cleans up connection to server on app quit.
    private void OnApplicationQuit()
    {
        if (ServerScript.instance.stream != null)
            ServerScript.instance.stream.Close();

        if (ServerScript.instance.client != null)
            ServerScript.instance.client.Close();

        Debug.Log("Disconnected from the server.");
    }
}
