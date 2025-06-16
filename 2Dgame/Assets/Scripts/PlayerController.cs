using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private enum State { idle, run, jump, falling };
    private State state = State.idle;
    private Collider2D coll;
    [SerializeField] private LayerMask Ground;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private int apples = 0;
    [SerializeField] private Text applesText;




    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
    }

    void Update()
    {
        float hDirection = Input.GetAxis("Horizontal");

        if (hDirection > 0)
        {
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
            transform.localScale = new Vector2(1, 1);
        }
        else if (hDirection < 0)
        {
            rb.linearVelocity = new Vector2(-speed, rb.linearVelocity.y);
            transform.localScale = new Vector2(-1, 1);
        }

        if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(Ground))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            state = State.jump;
        }

        VelocityState();
        anim.SetInteger("State", (int)state); // Ensure Animator has an int parameter named "state"
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Collectable")
        {
            Destroy(collision.gameObject);
            apples += 1;

            applesText.text = apples.ToString();
        }
    }
    private void VelocityState()
    {
        if (state == State.jump)
        {
            if (rb.linearVelocity.y < .1f)
            {
                state = State.falling;
            }
        }
        else if (state == State.falling)
        {
            if (coll.IsTouchingLayers(Ground))
            {
                state = State.idle;
            }
        }

        else if (Mathf.Abs(rb.linearVelocity.x) > 2f)
        {
            state = State.run;
        }
        else
        {
            state = State.idle;
        }
    }
}
