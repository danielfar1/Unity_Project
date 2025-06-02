using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Handles logic for the second player's movement and kicking in multiplayer mode.
/// Includes ground checking and interaction with the Shoe script.
public class SecondPlayerControllerMultiplayer : MonoBehaviour
{
    /// Singleton instance for accessing the second player globally.
    public static SecondPlayerControllerMultiplayer instance;

    [Header("Movement Settings")]
    public float speed;
    public float jumpForce;

    private Rigidbody2D rb;

    [Header("Ground Detection")]
    public bool isGrounded;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask whatIsGround;

    [Header("References")]
    public Shoe Shoe;
    public MultiplayerManager1 freeze;

    /// Initializes the Rigidbody and sets the singleton instance.
    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        rb = GetComponent<Rigidbody2D>();
    }

    /// Checks if the second player is currently grounded.
    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
    }

    /// Initiates a kick using the associated Shoe script, if not frozen.
    public void Kick()
    {
        if (freeze.isFrozen)
            return;

        Shoe.StartKick();
    }
}
