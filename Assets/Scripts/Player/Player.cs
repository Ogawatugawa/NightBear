using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Health")]
    public int curHealth = 6;
    public int maxHealth = 6;
    public float knockbackTime =  0.25f;
    public float postHitInvincibility = 0.75f;
    public Image[] healthPips;
    public bool CanBeDamaged = true;
    private bool IsFlashing = false;
    private bool FlashUp;

    [Header("Movement Variables")]
    public float baseSpeed = 5f;
    public float moveSpeed;
    public static Vector2 motion;
    public static Vector2 direction = new Vector2(0, -1);
    public static bool CanMove = true;

    [Header("Dodge Variables")]
    public float dodgeSpeed;
    public float maxDodgeTime, preDodgeDelay, afterDodgeDelay;
    private float dodgeTimer;
    public static bool isDodging;
    private Vector2 dodgeDirection;

    private Animator anim;
    private SpriteRenderer rend;
    public static Rigidbody2D rb;

    void Start()
    {
        anim = GetComponent<Animator>();
        rend = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        dodgeTimer = maxDodgeTime;
    }

    void Update()
    {
        CheckHealthPips();
        DamageFlash();

        if (CanMove)
        {
            float inputH = Input.GetAxisRaw("Horizontal");
            float inputV = Input.GetAxisRaw("Vertical");

            Move(inputH, inputV);

            FaceDirection(inputH, inputV);

            Dodge();
        }

        //Move using Rigidbody 2D
        rb.MovePosition(rb.position + motion * Time.deltaTime);
    }

    #region Health/Damage Functions and Enumerators
    void CheckHealthPips()
    {
        int i = 0;
        foreach (var pip in healthPips)
        {
            Animator anim = pip.GetComponent<Animator>();

            if (curHealth < i + 1 && !anim.GetBool("HeartDepleted"))
            {
                anim.SetBool("HeartDepleted", true);
            }

            else if (curHealth >= i + 1 && anim.GetBool("HeartDepleted"))
            {
                anim.SetBool("HeartDepleted", false);
            }

            i++;
        }
    }

    public void TakeDamage(int damage)
    {
        if (CanBeDamaged)
        {
            curHealth -= damage;
            if (curHealth <= 0)
            {
                Death();
            }
        }
    }

    public void TakeDamageWithKnockback(int damage, Vector2 force)
    {
        if (CanBeDamaged)
        {
            curHealth -= damage;
            StartCoroutine(Knockback(knockbackTime, force));
            if (curHealth <= 0)
            {
                Death();
            }
            else
            {
                StartCoroutine(HitInvincibility(postHitInvincibility));
            }
        }
    }

    public void Death()
    {
        rb.bodyType = RigidbodyType2D.Static;
        anim.SetTrigger("IsDying");
        StartCoroutine(EndDeath(0.5f));
    }

    public void DodgeBool()
    {
        if (CanBeDamaged)
        {
            CanBeDamaged = false;
        }

        else
        {
            CanBeDamaged = true;
        }
    }

    public virtual void DamageFlash()
    {
        Color flashColor = rend.color;
        if (IsFlashing)
        {
            if (!FlashUp)
            {
                if (flashColor.b > 0 || flashColor.g > 0)
                {
                    flashColor.b -= 15 * Time.deltaTime;
                    flashColor.g -= 15 * Time.deltaTime;
                }

                if (flashColor.b <= 0 && flashColor.g <= 0)
                {
                    FlashUp = true;
                }
            }

            else
            {
                if (flashColor.b < 1 || flashColor.g < 1)
                {
                    flashColor.b += 15 * Time.deltaTime;
                    flashColor.g += 15 * Time.deltaTime;
                }

                if (flashColor.b >= 1 && flashColor.g >= 1)
                {
                    FlashUp = false;
                }
            }
        }

        else
        {
            if (flashColor.b < 1 || flashColor.g < 1)
            {
                flashColor.b += 15 * Time.deltaTime;
                flashColor.g += 15 * Time.deltaTime;
            }
        }
        rend.color = flashColor;
    }

    #region Health/Damage Enumerators
    IEnumerator Knockback(float time, Vector2 force)
    {
        CanMove = false;
        motion = force;
        yield return new WaitForSeconds(time);
        CanMove = true;
    }

    IEnumerator HitInvincibility(float time)
    {
        CanBeDamaged = false;
        IsFlashing = true;
        yield return new WaitForSeconds(time);
        IsFlashing = false;
        if (!isDodging)
        {
            CanBeDamaged = true; 
        }
    }

    IEnumerator EndDeath (float time)
    {
        IsFlashing = true;
        yield return new WaitForSeconds(time);
        IsFlashing = false;
        Color flashColor = rend.color;
        if (flashColor.b != 1)
        {
            flashColor.b = 1;
        }
        if (flashColor.g != 1)
        {
            flashColor.g = 1; 
        }
        rend.color = flashColor;

        CanvasManager cm = GameObject.Find("Managers").GetComponent<CanvasManager>();
        cm.DeathScreenOn = true;

        Destroy(this);
    }
    #endregion

    #endregion

    #region Movement Functions and Enumerators
    void Move(float inputH, float inputV)
    {
        // Multiply Motion by Move Speed
        motion.x = inputH * moveSpeed;
        motion.y = inputV * moveSpeed;
        // Set our animator float Motion
        anim.SetFloat("Motion", motion.magnitude);
    }

    void Dodge()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDodging)
        {
            // Store the direction we were facing when the dodge started
            dodgeDirection = direction;
            // Delay movement by the pre-dodge penalty
            StartCoroutine(PreDodgeDelay(preDodgeDelay));
            // Set our dash timer to 0
            dodgeTimer = 0;
            // Set  IsDodging to true;
            isDodging = true;
        }

        // If Dodge is active, i.e. our dash timer is on and counting up
        if (dodgeTimer < maxDodgeTime && isDodging)
        {

            Physics2D.IgnoreLayerCollision(10, 11, true);
            direction = dodgeDirection;
            // Motion becomes our last faced direction multiplied by our dash speed
            motion = direction * dodgeSpeed;
            // Count up the dash timer by Time.deltaTime
            dodgeTimer += Time.deltaTime;
        }

        // Else if our dodge time is up and we're still flagged as dodging
        else if (dodgeTimer > maxDodgeTime && isDodging)
        {
            Physics2D.IgnoreLayerCollision(10, 11, false);
            // Stop moving
            motion = Vector2.zero;
            // Delay movement by post-dodge penalty
            StartCoroutine(AfterDodgeDelay(afterDodgeDelay));
            // Set isDodging to false
            isDodging = false;
        }
    }

    void FaceDirection(float inputH, float inputV)
    {
        // If we are currently sensing any input
        if ((inputH != 0 || inputV != 0) && !isDodging)
        {
            // Set our Direction variable as Motion (i.e. the last direction we travelled in based on inputs) normalised into -1, 0 or 1
            direction = motion.normalized;
        }

        // As long as our x and y direction aren't both 0
        if (!(direction.x == 0 && direction.y == 0))
        {
            // Set our animator floats for x and y
            anim.SetFloat("Horizontal", direction.x);
            anim.SetFloat("Vertical", direction.y);

            // Face left or right depending on which way we x direction we last input
            rend.flipX = direction.x < 0;
        }
    }

    #region Dodge Enumerators
    IEnumerator PreDodgeDelay(float delay)
    {
        CanMove = false;
        motion = Vector2.zero;
        anim.SetTrigger("DodgeRoll");
        yield return new WaitForSeconds(delay);
        CanMove = true;
    }

    IEnumerator AfterDodgeDelay(float delay)
    {
        CanMove = false;
        yield return new WaitForSeconds(delay);
        CanMove = true;
    }
    #endregion

    #endregion
}