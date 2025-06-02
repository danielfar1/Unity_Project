using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Controls the left player's movement and kicking actions in single-player mode.
/// Handles horizontal movement, jumping, ground checking, and shoe kick triggers.
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

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
    public Shoe leftShoe;
    public SinglePlayerScript freeze;

    /// Initializes the Rigidbody and singleton instance.
    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        rb = GetComponent<Rigidbody2D>();
    }

    /// Handles ground check and horizontal movement logic.
    /// Skips movement if the game is currently frozen.
    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        if (freeze.isFrozen)
            return;

        moveInput = Input.GetAxis("Horizontal1");
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    }

    /// Checks for jump and kick input. Prevents action when game is frozen.
    private void Update()
    {
        if (freeze.isFrozen)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            leftShoe.StartKick();
        }
        else if (Input.GetKey(KeyCode.W) && isGrounded)
        {
            jump();
        }
    }

    /// Applies vertical velocity to perform a jump.
    public void jump()
    {
        rb.linearVelocity = Vector2.up * jumpForce;
    }
}
