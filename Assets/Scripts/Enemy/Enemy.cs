﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Enemy : MonoBehaviour
{
    [Header("Enemy Health")]
    public int currentEnemyHealth;

    [Header("Enemy Movement")]
    public float idleSpeed;
    public float pursueSpeed;
    protected float moveSpeed;
    public Rigidbody2D rigid;
    protected SpriteRenderer rend;

    [Header("Enemy Attack")]
    public int damage;
    public float knockbackForce;
    public float attackStartDelay;
    public float attackEndDelay;
    public float attackCooldownMax = 2f;
    protected float attackCooldownTimer = 0f;
    protected bool AttackMode = false;
    protected bool CanAttack = true;
    protected Vector2 attackTargetPos;
    protected Vector2 attackDirection;

    [Header("Enemy AI")]
    public EnemyState state;
    public float detectionRadius;
    public float attackAttemptRadius;
    public float waypointDist;
    public GameObject waypointParentPrefab;
    public GameObject waypointPrefab;
    protected Transform target;
    private GameObject waypointParent;
    private Transform[] idleWaypoints;
    private Path path;
    private int waypointIndex = 0;
    private bool PathFinished = false;
    private Seeker seeker;
    private float updateTimer = 0;
    private float updateTimerMax = 0.5f;
    private CircleCollider2D detectionCollider;

    [Header("Movement Vectors")]
    protected Vector2 destination;
    protected Vector2 direction, force;

    [Header("Animation")]
    protected Animator anim;
    protected bool FlashUp;
    protected bool IsDying;
    private int flashCount;
    private int flashCountMax;

    #region Start and Update


    public virtual void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        CreateWaypoints();

        rend = GetComponent<SpriteRenderer>();
        seeker = GetComponent<Seeker>();
        rigid = GetComponent<Rigidbody2D>();

        attackCooldownTimer = attackCooldownMax;
        AttackMode = false;
        CanAttack = true;

        flashCount = flashCountMax;

        anim = GetComponent<Animator>();
    }

    public virtual void Update()
    {
        AttackChecks();

        DamageFlash();

        if (!IsDying)
        {
            if (AttackMode)
            {
                Attack();
            }

            else if (!AttackMode)
            {
                PathFind();
                Move(force);
            }
        }
    }
    #endregion

    #region Damage/Death Functions and Enumerators
    public virtual void TakeDamage(int damage)
    {
        // Take damage
        currentEnemyHealth -= damage;
        // Set flash count to 0 to begin damage flash function
        flashCount = 0;
        flashCountMax = 2;

        if (currentEnemyHealth <= 0 && !IsDying)
        {
            Death();
        }
    }

    public virtual void Death()
    {
        flashCountMax = 4;
        // Stop moving
        rigid.velocity = Vector2.zero;
        // Play death animation
        anim.SetTrigger("IsDead");
        // Turn movement collider into trigger
        Collider2D col = transform.Find("SpriteManager").transform.Find("Movement Collider").GetComponent<Collider2D>();
        col.isTrigger = true;
        IsDying = true;
        // Destroy this script
        StartCoroutine(DestroyDelay(1f));
    }

    public virtual void DamageFlash()
    {
        Color flashColor = rend.color;
        if (flashCount < flashCountMax)
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
                    flashCount++;
                }
            }
        }
        rend.color = flashColor;
    }

    IEnumerator DestroyDelay(float delay)
    {
        Color flashColor = rend.color;
        flashColor.b = 1;
        flashColor.g = 1;
        rend.color = flashColor;
        yield return new WaitForSeconds(delay);
        rigid.bodyType = RigidbodyType2D.Static;
        Destroy(this);
    }
    #endregion

    #region Attack Functions and Enumerators
    public virtual void Attack()
    {
        anim.SetBool("IsMoving", false);
    }

    public virtual void AttackChecks()
    {
        if (attackCooldownTimer < attackCooldownMax)
        {
            attackCooldownTimer += Time.deltaTime;
        }

        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
        if (distanceToTarget < detectionRadius)
        {
            state = EnemyState.Pursue;
        }
        else if (distanceToTarget > 10)
        {
            state = EnemyState.Idle;
        }

        if (distanceToTarget < attackAttemptRadius)
        {
            AttackMode = true;
        }
        else if (attackTargetPos == Vector2.zero)
        {
            AttackMode = false;
        }
    }

    public virtual IEnumerator AttackDelays(float startDelay, float endDelay)
    {
        attackCooldownTimer = 0f;
        if (attackTargetPos == Vector2.zero)
        {
            attackTargetPos = target.position;
            attackDirection = (attackTargetPos - rigid.position).normalized;
        }
        rigid.velocity = Vector2.zero;
        yield return new WaitForSeconds(startDelay);
        anim.SetTrigger("EnemyAttack");
        yield return new WaitForSeconds(endDelay);
        attackTargetPos = Vector2.zero;
        UpdatePath();
        AttackMode = false;
        print("Not attacking");
    }
    #endregion

    #region Movement and PathFinding
    public virtual void Move(Vector2 force)
    {
        anim.SetBool("IsMoving", true);

        rend.flipX = direction.x > 0.1f;

        rigid.MovePosition(rigid.position + force * Time.deltaTime);
    }

    // THIS FUNCTION DETERMINES WHETHER THE ENEMY IS IDLE OR PURSUING THE PLAYER, SETS OUR MOVEMENT VECTOR2s AND CHECKS IF THE ENEMY IS AT THE NEXT WAYPOINT
    void PathFind()
    {
        switch (state)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Pursue:
                Pursue();
                break;
            default:
                break;
        }

        direction = (destination - rigid.position).normalized;
        force = direction * moveSpeed * Time.deltaTime;

        float distance = Vector2.Distance(rigid.position, destination);

        if (distance < waypointDist)
        {
            waypointIndex++;
            if (waypointIndex > idleWaypoints.Length - 1)
            {
                waypointIndex = 0;
            }
        }
    }

    //THIS FUNCTION CREATES THE IDLE WAYPOINTS FOR Idle()
    void CreateWaypoints()
    {

        GameObject clone = Instantiate(waypointParentPrefab, transform.position, Quaternion.identity);
        waypointParent = clone;

        Vector3 parentPos = waypointParent.transform.position;
        Vector3 vector1 = new Vector3(parentPos.x - Random.Range(0.75f, 1.25f), parentPos.y + Random.Range(0.75f, 1.25f), 0f);
        Vector3 vector2 = new Vector3(parentPos.x + Random.Range(0.75f, 1.25f), parentPos.y + Random.Range(0.75f, 1.25f), 0f);
        Vector3 vector3 = new Vector3(parentPos.x - Random.Range(0.75f, 1.25f), parentPos.y - Random.Range(0.75f, 1.25f), 0f);
        Vector3 vector4 = new Vector3(parentPos.x + Random.Range(0.75f, 1.25f), parentPos.y - Random.Range(0.75f, 1.25f), 0f);

        Instantiate(waypointPrefab, vector1, Quaternion.identity, waypointParent.transform);
        Instantiate(waypointPrefab, vector2, Quaternion.identity, waypointParent.transform);
        Instantiate(waypointPrefab, vector3, Quaternion.identity, waypointParent.transform);
        Instantiate(waypointPrefab, vector4, Quaternion.identity, waypointParent.transform);

        waypointIndex = Random.Range(0, 3);

        idleWaypoints = waypointParent.GetComponentsInChildren<Transform>();
    }

    // THIS FUNCTION SETS THE ENEMY'S SPEED AND SETS ITS DESTINATION AS THE NEXT WAYPOINT IN THE IDLE WAYPOINT GROUP
    void Idle()
    {
        moveSpeed = idleSpeed;
        destination = idleWaypoints[waypointIndex].position;
    }

    // THIS FUNCTION SETS THE ENEMY'S SPEED AND SETS ITS DESTINATION AS THE NEXT WAYPOINT IN THE AI PATH
    void Pursue()
    {
        UpdatePath();

        if (path == null)
        {
            return;
        }

        if (waypointIndex >= path.vectorPath.Count)
        {
            PathFinished = true;
            return;
        }

        else
        {
            PathFinished = true;
        }

        moveSpeed = pursueSpeed;

        destination = path.vectorPath[waypointIndex];
    }

    // THIS FUNCTION THAT CREATES THE PATH THE AI WILL FOLLOW
    void UpdatePath()
    {
        if (updateTimer < updateTimerMax)
        {
            updateTimer += Time.deltaTime;
        }

        else
        {
            seeker.StartPath(rigid.position, target.position, OnPathComplete);
            updateTimer = 0;
        }
    }

    // THIS FUNCTION IS CALLED WHEN THE AI AGENT REACHES THE END OF A PATH
    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
        }
    }
    #endregion
}
public enum EnemyState { Idle, Pursue }