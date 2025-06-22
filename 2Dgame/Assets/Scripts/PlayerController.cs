using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;

    private enum State { idle, run, jump, falling, hurt };
    private State state = State.idle;

    private Collider2D coll;

    [SerializeField] private LayerMask Ground;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private int apples = 0;
    [SerializeField] private Text applesText;
    [SerializeField] private float hurtForce = 10f;
    [SerializeField] private int health;
    [SerializeField] private Text healthAmount;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        healthAmount.text = health.ToString(); 
    }

    void Update()
    {
        if (state != State.hurt)
        {
            Movement();
        }

        VelocityState();
        anim.SetInteger("State", (int)state);
    }

    private void Movement()
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Collectable")
        {
            Destroy(collision.gameObject);
            apples += 1;
            applesText.text = apples.ToString();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            if (state == State.falling)
            {
                Destroy(other.gameObject);
            }
            else
            {
                state = State.hurt;

               HandleHealth();

                if (other.gameObject.transform.position.x > transform.position.x)
                {
                    rb.linearVelocity = new Vector2(-hurtForce, rb.linearVelocity.y);
                }
                else
                {
                    rb.linearVelocity = new Vector2(hurtForce, rb.linearVelocity.y);
                }
            }
        }
    }

    private void HandleHealth()
    {
        health -= 1;
        healthAmount.text = health.ToString();
        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
