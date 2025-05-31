using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public float speed;
    public float jumpForce;
    private float moveInput;

    private Rigidbody2D rb;

    public bool isGrounded;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask whatIsGround;

    public Shoe leftShoe;
    public SinglePlayerScript freeze;

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
        if (freeze.isFrozen)
            return;
        moveInput = Input.GetAxis("Horizontal1");
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    }
    private void Update()
    {

        if (freeze.isFrozen)
            return;
         if (Input.GetKeyDown(KeyCode.Space))
        {
            leftShoe.StartKick();
        }
        else if(Input.GetKey(KeyCode.W) && isGrounded == true)
        {
            jump();
        }
        
    }
    public void jump()
    {
        rb.linearVelocity = Vector2.up * jumpForce;
    }

}
