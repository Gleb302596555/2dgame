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
    private AudioSource footstep;
    private AudioSource audioSource;

    private bool hasSpawned = false;
    private bool isDead = false;
    private bool facingRight = true;

    private int score = 0;

    [SerializeField] private LayerMask Ground;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private int apples = 0;
    [SerializeField] private Text applesText;
    [SerializeField] private float hurtForce = 10f;
    [SerializeField] private int health = 3;
    [SerializeField] private Text healthAmount;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private Text scoreText;
    [SerializeField] private AudioClip steroidEnterSound;
    [SerializeField] private AudioClip steroidExitSound;
    [SerializeField] private AudioClip CollectableSound;
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float airControlMultiplier = 0.5f;
    [SerializeField] private float maxSpeed = 10f;

    private bool isControlLocked = false;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        footstep = GetComponent<AudioSource>();
        audioSource = GetComponent<AudioSource>();

        healthAmount.text = health.ToString();
        StartCoroutine(SpawnAnimation());

        PlayerPrefs.SetString("LastLevel", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }

    void Update()
    {
        if (!hasSpawned || isDead) return;

        if (state != State.hurt)
        {
            Movement();
        }

        VelocityState();
        anim.SetInteger("State", (int)state);
    }



    private void Movement()
    {
        float hDirection = Input.GetAxisRaw("Horizontal");
        bool isGrounded = coll.IsTouchingLayers(Ground);

        float targetSpeed = hDirection * speed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        if (!isGrounded)
            accelRate *= airControlMultiplier;

        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, 0.9f) * Mathf.Sign(speedDiff);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement * Time.fixedDeltaTime, rb.linearVelocity.y);

        if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * maxSpeed, rb.linearVelocity.y);

        if (hDirection > 0 && !facingRight)
            Flip();
        else if (hDirection < 0 && facingRight)
            Flip();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            state = State.jump;
        }
    }


    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void AddScore(int amount)
    {
        score += amount;
        scoreText.text = score.ToString();

        int currentGlobal = PlayerPrefs.GetInt("GlobalScore", 0);
        PlayerPrefs.SetInt("GlobalScore", currentGlobal + amount);
    }

    private IEnumerator ResetPower()
    {
        yield return new WaitForSeconds(5);
        jumpForce = 10f;
        GetComponent<SpriteRenderer>().color = Color.white;
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
            health += 1;
            applesText.text = apples.ToString();
            healthAmount.text = health.ToString();
            AddScore(1);
            if (CollectableSound != null)
            {
                audioSource.PlayOneShot(CollectableSound);
            }

        }

        if (collision.tag == "PowerUp")
        {
            Destroy(collision.gameObject);
            StartCoroutine(ActivateSteroidMode());
        }

        if (collision.tag == "CheckPoint")
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            if (state == State.falling)
            {
                Destroy(other.gameObject);
                AddScore(3);
            }
            else
            {
                if (isSteroidActive) return;

                state = State.hurt;
                isControlLocked = true;
                anim.SetTrigger("Hurt");
                StartCoroutine(UnlockControlAfterHurt());
                HandleHealth();
                StartCoroutine(FlashRed());

                if (hurtSound != null)
                    audioSource.PlayOneShot(hurtSound);

                Vector2 knockback = (transform.position.x < other.transform.position.x)
                    ? new Vector2(-hurtForce, rb.linearVelocity.y)
                    : new Vector2(hurtForce, rb.linearVelocity.y);

                rb.linearVelocity = knockback;
            }
        }

        if (other.gameObject.tag == "Spikes")
        {
            if (isSteroidActive) return;

            state = State.hurt;
            isControlLocked = true;
            anim.SetTrigger("Hurt");
            StartCoroutine(UnlockControlAfterHurt());
            HandleHealth();

            Vector2 pushDirection = (transform.position - other.transform.position).normalized;
            rb.linearVelocity = new Vector2(pushDirection.x * hurtForce, jumpForce / 2f);

            StartCoroutine(FlashRed());

            if (hurtSound != null)
                audioSource.PlayOneShot(hurtSound);
        }
    }

    private void HandleHealth()
    {
        if (isSteroidActive) return;

        health -= 1;
        healthAmount.text = health.ToString();

        if (health <= 0 && !isDead)
        {
            isDead = true;
            anim.SetTrigger("Disappear");
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            coll.enabled = false;
            PlayDeathSound();
            StartCoroutine(RestartAfterDisappear());
        }
    }

    private IEnumerator UnlockControlAfterHurt()
    {
        yield return new WaitForSeconds(0.5f); // або тривалість анімації
        isControlLocked = false;
    }




    private IEnumerator RestartAfterDisappear()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void FootStep()
    {
        footstep.Play();
    }

    private IEnumerator SpawnAnimation()
    {
        anim.SetTrigger("Spawn");
        yield return new WaitForSeconds(2f);
        hasSpawned = true;
        if (spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }

    public void PlayDeathSound()
    {
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }

    private IEnumerator FlashRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;
    }

    private bool isSteroidActive = false;

    private IEnumerator ActivateSteroidMode()
    {
        if (isSteroidActive) yield break; 
        isSteroidActive = true;

        float growDuration = 0.5f;
        float shrinkDuration = 0.5f;
        float effectDuration = 5f;

        float elapsed = 0f;

        float originalSpeed = speed;
        float originalJump = jumpForce;
        Vector3 originalScale = transform.localScale;

        float targetSpeed = speed * 1.5f;
        float targetJump = jumpForce * 1.5f;
        Vector3 targetScale = new Vector3(Mathf.Abs(originalScale.x) * 1.5f, originalScale.y * 1.5f, originalScale.z);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = Color.white;
        Color steroidColor = Color.yellow;

        if (steroidEnterSound != null)
        {
            audioSource.PlayOneShot(steroidEnterSound);
        }

 
        while (elapsed < growDuration)
        {
            float t = elapsed / growDuration;
            speed = Mathf.Lerp(originalSpeed, targetSpeed, t);
            jumpForce = Mathf.Lerp(originalJump, targetJump, t);

            Vector3 scaled = Vector3.Lerp(originalScale, targetScale, t);
            float currentDirection = Mathf.Sign(transform.localScale.x);
            scaled.x *= currentDirection;
            transform.localScale = scaled;

            sr.color = Color.Lerp(originalColor, steroidColor, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        speed = targetSpeed;
        jumpForce = targetJump;
        float finalDirection = Mathf.Sign(transform.localScale.x);
        targetScale.x *= finalDirection;
        transform.localScale = targetScale;
        sr.color = steroidColor;

        yield return new WaitForSeconds(effectDuration);

        if (steroidExitSound != null)
        {
            audioSource.PlayOneShot(steroidExitSound);
        }

       
        elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            float t = elapsed / shrinkDuration;
            speed = Mathf.Lerp(targetSpeed, originalSpeed, t);
            jumpForce = Mathf.Lerp(targetJump, originalJump, t);

            Vector3 scaled = Vector3.Lerp(targetScale, originalScale, t);
            float currentDirection = Mathf.Sign(transform.localScale.x);
            scaled.x *= currentDirection;
            transform.localScale = scaled;

            sr.color = Color.Lerp(steroidColor, originalColor, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        speed = originalSpeed;
        jumpForce = originalJump;

        Vector3 finalScale = originalScale;
        finalScale.x = Mathf.Abs(originalScale.x) * Mathf.Sign(transform.localScale.x);
        transform.localScale = finalScale;

        sr.color = originalColor;

        isSteroidActive = false;
    }



}
