using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Controls player movement and kick actions in multiplayer mode.
/// Manages horizontal movement, jumping, shoe kicks, and ground detection.
public class PlayerControllerMultiplayer : MonoBehaviour
{
    /// Singleton instance for global access.
    public static PlayerControllerMultiplayer instance;

    [Header("Movement Settings")]
    public float speed;
    public float jumpForce;
    private float moveInput;

    private Rigidbody2D rb;

    [Header("Ground Detection")]
    public bool isGrounded;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask whatIsGround;

    [Header("References")]
    public Shoe Shoe;
    public MultiplayerManager1 freeze;

    /// Indicates whether the player has kicked in the current frame.
    public bool kicked;

    /// Initializes Rigidbody and assigns singleton instance.
    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        rb = GetComponent<Rigidbody2D>();
    }

    /// Checks for ground status and applies horizontal movement.
    /// Ignores input if the game is frozen.
    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        if (freeze.isFrozen)
            return;

        moveInput = Input.GetAxis("Horizontal1");
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    }

    /// Processes input for jumping and kicking.
    /// Prevents action if game is frozen.
    private void Update()
    {
        if (freeze.isFrozen)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoe.StartKick();
            kicked = true;
        }
        else if (Input.GetKey(KeyCode.W) && isGrounded)
        {
            jump();
        }
    }

    /// Applies vertical force to simulate a jump.
    public void jump()
    {
        rb.linearVelocity = Vector2.up * jumpForce;
    }
}
