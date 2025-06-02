using UnityEngine;
using System.Collections;

/// Manages the kicking motion of the player's shoe,
/// including animation curves, follow behavior, and interaction with user input.
public class Shoe : MonoBehaviour
{
    public Transform player; // Reference to the player object
    public Transform playerTransform; // Duplicate for precision in calculations

    // Vertical offsets (how high the shoe moves in each phase)
    public float moveDistance1 = 0.1f;
    public float moveDistance2 = 0.2f;
    public float moveDistance3 = 0.2f;
    public float moveDistance4 = 0.4f;

    // Horizontal offsets (how far the shoe moves in each phase)
    public float horizontalDistance1 = -0.8f;
    public float horizontalDistance2 = -0.2f;
    public float horizontalDistance3 = -0.1f;
    public float horizontalDistance4 = -0.1f;

    public float moveSpeed = 2f; // Speed of the kick movement
    public float rotationY = 0;  // Flip shoe for left/right orientation
    public string key;           // Key used to hold the kick pose ("Space" or "Enter")

    private bool isKicking = false;  // Whether a kick is currently in progress
    private Vector3 startPosition;
    private Vector3 peakPosition1, peakPosition2, peakPosition3, peakPosition4;

    /// Initial rotation setup.
    private void Start()
    {
        transform.rotation = Quaternion.Euler(0, rotationY, 40);
    }

    /// Keeps the shoe attached to the player's feet when idle.
    void Update()
    {
        if (!isKicking)
        {
            FollowPlayer();
        }
    }

    /// Called externally to initiate a kick animation.
    public void StartKick()
    {
        if (!isKicking)
        {
            isKicking = true;
            startPosition = new Vector3(playerTransform.position.x, playerTransform.position.y - 0.67f, 0);
            StartCoroutine(MoveInCurve());
        }
    }

    /// Animates the shoe in an upward and forward motion,
    /// simulating a curved kick path using multiple lerp steps and rotation changes.
    IEnumerator MoveInCurve()
    {
        transform.rotation = Quaternion.Euler(0, rotationY, 40);
        peakPosition1 = playerTransform.position + new Vector3(horizontalDistance1 / 2, -0.67f + moveDistance1, 0);
        yield return StartCoroutine(MoveToPosition(startPosition, peakPosition1));

        transform.rotation = Quaternion.Euler(0, rotationY, 50);
        peakPosition2 = peakPosition1 + new Vector3(horizontalDistance2 / 2, moveDistance2, 0);
        yield return StartCoroutine(MoveToPosition(peakPosition1, peakPosition2));

        transform.rotation = Quaternion.Euler(0, rotationY, 70);
        peakPosition3 = peakPosition2 + new Vector3(horizontalDistance3 / 2, moveDistance3, 0);
        yield return StartCoroutine(MoveToPosition(peakPosition2, peakPosition3));

        transform.rotation = Quaternion.Euler(0, rotationY, 100);
        peakPosition4 = peakPosition3 + new Vector3(horizontalDistance4 / 2, moveDistance4, 0);
        yield return StartCoroutine(MoveToPosition(peakPosition3, peakPosition4));

        // Hold final position while the key is held down
        transform.rotation = Quaternion.Euler(0, rotationY, 120);
        while ((key == "Space" && Input.GetKey(KeyCode.Space)) ||
               (key == "Enter" && Input.GetKey(KeyCode.Return)))
        {
            yield return StartCoroutine(MoveToPosition(peakPosition4, peakPosition4));
        }

        // Reverse motion back to idle
        transform.rotation = Quaternion.Euler(0, rotationY, 120);
        yield return StartCoroutine(MoveToPosition(peakPosition4, peakPosition3));

        transform.rotation = Quaternion.Euler(0, rotationY, 100);
        yield return StartCoroutine(MoveToPosition(peakPosition3, peakPosition2));

        transform.rotation = Quaternion.Euler(0, rotationY, 70);
        yield return StartCoroutine(MoveToPosition(peakPosition2, peakPosition1));

        transform.rotation = Quaternion.Euler(0, rotationY, 50);
        yield return StartCoroutine(MoveToPosition(peakPosition1, startPosition));

        transform.rotation = Quaternion.Euler(0, rotationY, 40);
        isKicking = false;
    }

    /// Smoothly moves the shoe from one position to another over time using linear interpolation.
    IEnumerator MoveToPosition(Vector3 start, Vector3 end)
    {
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(start, end, elapsedTime);
            elapsedTime += Time.deltaTime * moveSpeed;
            yield return null;
        }

        transform.position = end;
    }

    /// Makes the shoe follow the player's position, offset downward slightly to simulate attachment to the foot.
    void FollowPlayer()
    {
        transform.position = player.position - new Vector3(0, 0.67f, 0);
    }

    /// Handles enabling collision with ground (if needed).
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Ground detected under shoe.");
            collision.isTrigger = !enabled; // Could be part of a ground interaction mechanism
        }
    }
}
