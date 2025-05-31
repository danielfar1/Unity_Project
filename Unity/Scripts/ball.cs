using UnityEngine;

public class ball : MonoBehaviour
{
    public float launchForce = 1000f; // The strength of the force applied
    private Rigidbody2D rb;


    void Start()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();

        // Ensure the Rigidbody2D is set to dynamic
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }
    private void Update()
    {
        if(gameObject.transform.position.y< -2.600206f)
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, -2.600206f, gameObject.transform.position.z);
        }
        if (gameObject.transform.position.x < -8.67f)
        {
            gameObject.transform.position = new Vector3(-8.67f, gameObject.transform.position.y, gameObject.transform.position.z);
        }
        if (gameObject.transform.position.x > 8.67f)
        {
            gameObject.transform.position = new Vector3(8.67f, gameObject.transform.position.y, gameObject.transform.position.z);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        // Get the point of contact
        ContactPoint2D contact = collision.GetContact(0);

        // Determine if the collision is from the left or right
        bool hitFromLeft = contact.point.x < transform.position.x;

        // Determine the launch direction based on the side of impact
        Vector2 launchDirection = hitFromLeft ? Vector2.right : Vector2.left;

        // Apply force to the Rigidbody2D
        if (!collision.gameObject.CompareTag("Ground"))
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                launchForce = 200 * 1.5f;
                Debug.Log(launchForce);
            }
            else
            {
                launchForce = 200 * 0.5f;
            }
            rb.AddForce(launchDirection * launchForce);
            rb.AddTorque(launchForce * 0.1f);
        }

        // Optionally add torque for a spinning effect


    }
}
