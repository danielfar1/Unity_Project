using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class SinglePlayerScript : MonoBehaviour
{
    public static SinglePlayerScript instance;
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
    public Image reload;
    public Image goBack;
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
    private bool leftWin = false;
    private bool rightWin = false;
    public TextMeshProUGUI rightPlayerScore;
    public TextMeshProUGUI leftPlayerScore;
    public PlayerController leftPlayerController;
    public SecondPlayerController rightPlayerController;
    public GameObject ballParticles;

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
        if (instance == null)
        {
            instance = this;
        }
        rectTransformLeft = playerLeft.GetComponent<RectTransform>();
        rectTransformRight = playerRight.GetComponent<RectTransform>();
        spawnPosLeftPlayer = rectTransformLeft.position;
        spawnPosRightPlayer = rectTransformRight.position;
        StartCoroutine(GameStart());
    }
    private void Update()
    {
        if (leftWin == true)
        {
            if (leftPlayerController.isGrounded == true)
                rbLeft.linearVelocity = Vector2.up * 5;
        }
        if(rightWin == true)
        {
            if (rightPlayerController.isGrounded == true)
                rbRight.linearVelocity = Vector2.up * 5;
        }
        rectTransformLeft.rotation = Quaternion.Euler(rectTransformLeft.rotation.eulerAngles.x, rectTransformLeft.rotation.eulerAngles.y, 0);
        rectTransformRight.rotation = Quaternion.Euler(rectTransformRight.rotation.eulerAngles.x, rectTransformRight.eulerAngles.y, 0);
        if (isFrozen) return;
        Vector2 targetPos = new Vector2(ballPos.position.x, ballPos.position.y);
        if (targetPos.x > -8.67f && targetPos.x < -8 && targetPos.y < -1 && targetPos.y > -3 )
        {
            LeftGoalScorred();
        }
        if (targetPos.x > 8f && targetPos.x < 8.67 && targetPos.y < -1 && targetPos.y > -3)
        {
            RightGoalScorred();
        }
        

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
            StartCoroutine(GameStart());
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
            StartCoroutine(GameStart());
        }

    }
    IEnumerator GameStart()
    {
        isFrozen = true;
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
        if(leftGoalsCount>rightGoalsCount)
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
    private void RightPlayerWin()
    {
        ball.SetActive(false);
        winnerImage.enabled = true;
        playerRightWinner.enabled = true;
        reload.enabled = true;
        goBack.enabled = true;
        Debug.Log("right Player Win");
        isFrozen = true;
        rightWin = true;
       
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void GoBack()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 3);
    }
    private void OnApplicationQuit()
    {
        if (ServerScript.instance.stream != null)
            ServerScript.instance.stream.Close();
        if (ServerScript.instance.client != null)
            ServerScript.instance.client.Close();
        Debug.Log("Disconnected from the server.");
    }
}
