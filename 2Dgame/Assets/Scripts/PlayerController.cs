using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator anim;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            rb.linearVelocity = new Vector2(5, 0);
            transform.localScale = new Vector2(1, 1);
            anim.SetBool("running", true);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            rb.linearVelocity = new Vector2(-5, 0);
            transform.localScale = new Vector2(-1, 1);
            anim.SetBool("running", true);
        }
        else {
            anim.SetBool("running", false);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 10f);
        }

    }
}