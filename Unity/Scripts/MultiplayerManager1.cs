using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
public class MultiplayerManager1 : MonoBehaviour
{
    public static MultiplayerManager1 instance;
    public GameObject goImage;
    public GameObject oneImage;
    public GameObject twoImage;
    public GameObject threeImage;
    public GameObject ball;
    public Image playerLeft;
    public Image playerRight;
    public Image leftShoeImage;
    public Image rightShoeImage;
    public Image winnerImage;
    public Image playerRightWinner;
    public Image playerLeftWinner;
    public Shoe leftShoe;
    public Shoe rightShoe;
    public Transform ballPos;
    private RectTransform rectTransformLeft;
    private RectTransform rectTransformRight;
    public float moveSpeed = 5f; // Movement speed
    private int leftGoalsCount = 0;
    private int rightGoalsCount = 0;
    public Rigidbody2D ballMove;
    public Rigidbody2D rbLeft;
    public Rigidbody2D rbRight;
    private Vector3 spawnBallPosition = new Vector3(0f, 3f, 0);
    private Vector2 spawnPosLeftPlayer;
    private Vector2 spawnPosRightPlayer;
    public bool isFrozen = false;
    public bool pauseTimer = false;
    private bool overtime = false;
    public bool gameIsgoing = false;
    private bool leftWin = false;
    private bool rightWin = false;
    private bool gameFinish;
    public TextMeshProUGUI rightPlayerScore;
    public TextMeshProUGUI leftPlayerScore;
    public PlayerControllerMultiplayer leftPlayerController;
    public SecondPlayerControllerMultiplayer rightPlayerController;
    public bool firstToConnect=false;
    public TimerMultiplayer timer;
    void Start()
    {
        Application.runInBackground = true;
        winnerImage.enabled = false;
        playerRightWinner.enabled = false;
        playerLeftWinner.enabled = false;
        pauseTimer = false;
        goImage.SetActive(false);
        oneImage.SetActive(false);
        twoImage.SetActive(false);
        threeImage.SetActive(false);
        if (instance == null)
        {
            instance = this;
        }
        rectTransformLeft = playerLeft.GetComponent<RectTransform>();
        rectTransformRight = playerRight.GetComponent<RectTransform>();
        spawnPosLeftPlayer = rectTransformLeft.position;
        spawnPosRightPlayer = rectTransformRight.position;
    }
    void Update()
    {
        if (gameFinish == true)
        {
            if (leftWin == true)
            {
                if (leftPlayerController.isGrounded == true)
                    rbLeft.linearVelocity = Vector2.up * 5;
            }
            if (rightWin == true)
            {
                if (rightPlayerController.isGrounded == true)
                    rbRight.linearVelocity = Vector2.up * 5;
            }
            return;
        }

        if(gameIsgoing == false)
        {
            ball.SetActive(false);
            playerRight.enabled = false;
            rightShoeImage.enabled = false;
            isFrozen = true;
            if (firstToConnect != true)
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
                {
                    BeginTheGame();
                }
            }
            
        }
        
        if (isFrozen) return;
        Vector2 targetPos = new Vector2(ballPos.position.x, ballPos.position.y);
        if (targetPos.x > -8.67f && targetPos.x < -8 && targetPos.y < -1 && targetPos.y > -3)
        {
            LeftGoalScorred();
        }
        if (targetPos.x > 8f && targetPos.x < 8.67 && targetPos.y < -1 && targetPos.y > -3)
        {
            RightGoalScorred();
        }
    }
    private void BeginTheGame()
    {
        CancelInvoke(nameof(BeginTheGame));
        Debug.Log("Opponent connected. Starting listener.");
        gameIsgoing = true;
        playerRight.enabled = true;
        rightShoeImage.enabled = true;
        ball.SetActive(true);
        isFrozen = false;
        ServerScript.instance.StartLisener(); // Now start your listening thread
        StartCoroutine(StartGame());
    }
    private void LeftGoalScorred()
    {

        leftGoalsCount++;
        leftPlayerScore.text = string.Format("{0}", leftGoalsCount);
        if (overtime)
        {
            RightPlayerWin();
        }
        else
        {
            StartCoroutine(StartGame());
        }
    }
    private void RightGoalScorred()
    {
        rightGoalsCount++;
        rightPlayerScore.text = string.Format("{0}", rightGoalsCount);
        if (overtime)
        {
            LeftPlayerWin();
        }
        else
        {
            StartCoroutine(StartGame());
        }
    }
    IEnumerator WaitForOpponent()
    {
        
        yield return new WaitForSecondsRealtime(3);
        
    }
    IEnumerator StartGame()
    {
        isFrozen = true;
        Debug.Log(overtime);
        Debug.Log(firstToConnect);
        Debug.Log(leftGoalsCount);
        Debug.Log(rightGoalsCount);
        if(!overtime && !firstToConnect && leftGoalsCount == 0 && rightGoalsCount == 0)
        {
            yield return new WaitForSecondsRealtime(2);
            timer.remainingTime = 60f;
        }
        yield return new WaitForSecondsRealtime(1);
        ballPos.position = spawnBallPosition;
        ballMove.linearVelocity = Vector3.zero;
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
    private void RightPlayerWin()
    {
        ball.SetActive(false);
        winnerImage.enabled = true;
        playerRightWinner.enabled = true;
        Debug.Log("right Player Win");
        isFrozen = true;
        rightWin = true;
        gameFinish = true;
        gameIsgoing = false;
    }
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
