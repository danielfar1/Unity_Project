using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SecondPlayerControllerMultiplayer : MonoBehaviour
{
    public static SecondPlayerControllerMultiplayer instance;
    public float speed;
    public float jumpForce;

    private Rigidbody2D rb;

    public bool isGrounded;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask whatIsGround;

    public Shoe Shoe;
    public MultiplayerManager1 freeze;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
    }
    public void Kick()
    {
        if (freeze.isFrozen)
            return;
        Shoe.StartKick();
    }
    
}