using UnityEngine;
using System.Collections;

public class Shoe : MonoBehaviour
{
    public Transform player;
    //y
    public float moveDistance1 = 0.1f;
    public float moveDistance2 = 0.2f;
    public float moveDistance3 = 0.2f;
    public float moveDistance4 = 0.4f;
    //x
    public float horizontalDistance1 = -0.8f;
    public float horizontalDistance2 = -0.2f;
    public float horizontalDistance3 = -0.1f;
    public float horizontalDistance4 = -0.1f;
    public float moveSpeed = 2f;   // Speed of the movement

    public float rotationY = 0;
    private Vector3 dis;
    public Transform playerTransform;

    private Vector3 startPosition;
    private Vector3 peakPosition1;
    private Vector3 peakPosition2;
    private Vector3 peakPosition3;
    private Vector3 peakPosition4;
    public string key;

    private bool isKicking = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("fad");
            collision.isTrigger = !enabled;
        }
    }
    void Update()
    {
        if (!isKicking)
        {
            FollowPlayer(); // Keep shoe with player when not kicking
        }
    }
    private void Start()
    {
        transform.rotation = Quaternion.Euler(0, rotationY, 40);
    }
    public void StartKick()
    {
        if (!isKicking)
        {
            isKicking = true;

            startPosition = new Vector3(playerTransform.position.x, playerTransform.position.y - 0.67f, 0);
            
            StartCoroutine(MoveInCurve());
        }
    }
    

    IEnumerator MoveInCurve()
    {
        // Move up and right (start to peak)
        transform.rotation = Quaternion.Euler(0, rotationY, 40);
        peakPosition1 = playerTransform.position + new Vector3( horizontalDistance1 / 2,  - 0.67f+ moveDistance1, 0);
        yield return StartCoroutine(MoveToPosition(startPosition, peakPosition1));

        transform.rotation = Quaternion.Euler(0, rotationY, 50);
        peakPosition1 = playerTransform.position + new Vector3(horizontalDistance1 / 2, -0.67f + moveDistance1, 0);
        peakPosition2 = playerTransform.position + new Vector3( horizontalDistance1 / 2 + horizontalDistance2 / 2,  - 0.67f + moveDistance1 + moveDistance2, 0);
        yield return StartCoroutine(MoveToPosition(peakPosition1, peakPosition2));

        transform.rotation = Quaternion.Euler(0, rotationY, 70);
        peakPosition2 = playerTransform.position + new Vector3(horizontalDistance1 / 2 + horizontalDistance2 / 2, -0.67f + moveDistance1 + moveDistance2, 0);
        peakPosition3 = playerTransform.position + new Vector3( horizontalDistance1 / 2 + horizontalDistance2 / 2+ horizontalDistance3 / 2,  - 0.67f + moveDistance1 + moveDistance2+ moveDistance3, 0);
        yield return StartCoroutine(MoveToPosition(peakPosition2, peakPosition3));

        transform.rotation = Quaternion.Euler(0, rotationY, 100);
        peakPosition3 = playerTransform.position + new Vector3(horizontalDistance1 / 2 + horizontalDistance2 / 2 + horizontalDistance3 / 2, -0.67f + moveDistance1 + moveDistance2 + moveDistance3, 0);
        peakPosition4 = playerTransform.position + new Vector3( horizontalDistance1 / 2 + horizontalDistance2 / 2 + horizontalDistance3 / 2 + horizontalDistance4/2, - 0.67f + moveDistance1 + moveDistance2 + moveDistance3 + moveDistance4, 0);
        yield return StartCoroutine(MoveToPosition(peakPosition3, peakPosition4));

        if(key == "Space")
        {
            while (Input.GetKey(KeyCode.Space))
            {
                transform.rotation = Quaternion.Euler(0, rotationY, 120);
                peakPosition4 = new Vector3(playerTransform.position.x + horizontalDistance1 / 2 + horizontalDistance2 / 2 + horizontalDistance3 / 2 + horizontalDistance4 / 2, playerTransform.position.y - 0.67f + moveDistance1 + moveDistance2 + moveDistance3 + moveDistance4, 0);
                yield return StartCoroutine(MoveToPosition(peakPosition4, peakPosition4));
            }
        }
        if(key == "Enter")
        {
            while (Input.GetKey(KeyCode.Return))
            {
                transform.rotation = Quaternion.Euler(0, rotationY, 120);
                peakPosition4 = new Vector3(playerTransform.position.x + horizontalDistance1 / 2 + horizontalDistance2 / 2 + horizontalDistance3 / 2 + horizontalDistance4 / 2, playerTransform.position.y - 0.67f + moveDistance1 + moveDistance2 + moveDistance3 + moveDistance4, 0);
                yield return StartCoroutine(MoveToPosition(peakPosition4, peakPosition4));
            }
        }
        transform.rotation = Quaternion.Euler(0, rotationY, 120);
        peakPosition4 = playerTransform.position + new Vector3(horizontalDistance1 / 2 + horizontalDistance2 / 2 + horizontalDistance3 / 2 + horizontalDistance4 / 2, -0.67f + moveDistance1 + moveDistance2 + moveDistance3 + moveDistance4, 0);
        peakPosition3 = playerTransform.position + new Vector3(horizontalDistance1 / 2 + horizontalDistance2 / 2 + horizontalDistance3 / 2, -0.67f + moveDistance1 + moveDistance2 + moveDistance3, 0);
        yield return StartCoroutine(MoveToPosition(peakPosition4, peakPosition3));

        transform.rotation = Quaternion.Euler(0, rotationY, 100);
        peakPosition3 = playerTransform.position + new Vector3(horizontalDistance1 / 2 + horizontalDistance2 / 2 + horizontalDistance3 / 2, -0.67f + moveDistance1 + moveDistance2 + moveDistance3, 0);
        peakPosition2 = playerTransform.position + new Vector3(horizontalDistance1 / 2 + horizontalDistance2 / 2, -0.67f + moveDistance1 + moveDistance2, 0);
        yield return StartCoroutine(MoveToPosition(peakPosition3, peakPosition2));

        transform.rotation = Quaternion.Euler(0, rotationY, 70);
        peakPosition2 = playerTransform.position + new Vector3(horizontalDistance1 / 2 + horizontalDistance2 / 2, -0.67f + moveDistance1 + moveDistance2, 0);
        peakPosition1 = playerTransform.position + new Vector3(horizontalDistance1 / 2, -0.67f + moveDistance1, 0);
        yield return StartCoroutine(MoveToPosition(peakPosition2, peakPosition1));

        transform.rotation = Quaternion.Euler(0, rotationY, 50);
        peakPosition1 = playerTransform.position + new Vector3(horizontalDistance1 / 2, -0.67f + moveDistance1, 0);
        startPosition = new Vector3(playerTransform.position.x, playerTransform.position.y - 0.67f, 0);
        yield return StartCoroutine(MoveToPosition(peakPosition1, startPosition));
        transform.rotation = Quaternion.Euler(0, rotationY, 40);
        isKicking = false;
    }

    IEnumerator MoveToPosition(Vector3 start, Vector3 end)
    {
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            // Lerp between the start and end positions
            transform.position = Vector3.Lerp(start, end, elapsedTime);

            elapsedTime += Time.deltaTime * moveSpeed;
            yield return null; // Wait for the next frame
        }

        // Ensure the object reaches the exact end position
        transform.position = end;
    }
    void FollowPlayer()
    {
        // Make sure the shoe stays attached to the player
        transform.position = player.position;
        transform.position -= new Vector3(0, 0.67f, 0);
    }
}
