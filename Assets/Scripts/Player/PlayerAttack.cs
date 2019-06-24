//==================================
//Author: Vin Tansiri
//Title: PlayerAttack.cs
//Date: 18 May 2019
//==================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public int attackDamage = 3;
    public float attackRadius = 0.8f;
    public float attackRange = 1.2f;
    public float knockbackForce;

    [Header("Attack Movement")]
    public float attackMoveSpeed;
    public float attackMoveTime;
    private Vector2 attackMotion;

    [Header("Combo and Delay")]
    public float attackStartDelay = 0.6f;
    public float attackEndDelay = 0.3f;
    public float endComboDelay = 0.7f;
    private float comboTimer = 0f;
    public float maxComboTime = 0.6f;
    private int attackCounter;
    private int attackComboLimit = 2;

    [Header("Attack Bools")]
    public static bool IsAttacking = false;
    public static bool CanAttack = true;
    public static bool CanBeDamaged = true;

    private Animator anim;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + (Vector3)Player.direction * attackRange, attackRadius);
    }

    void Start()
    {
        anim = GetComponentInParent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }

        if (IsAttacking)
        {
            Player.rb.MovePosition(Player.rb.position + attackMotion * Time.deltaTime);
        }

        ComboTime();
    }

    void Attack()
    {
        if (!Player.isDodging && CanAttack)
        {
            Player.CanMove = false;
            CanAttack = false;
            anim.SetTrigger("Attack");
            StartCoroutine(AttackWithStartDelay(attackStartDelay));


            if (attackCounter < attackComboLimit)
            {
                StartCoroutine(AttackEndDelay(attackEndDelay));
                attackCounter++;
                comboTimer = 0;
            }

            else if (attackCounter >= attackComboLimit)
            {
                StartCoroutine(AttackEndDelay(endComboDelay));
                attackCounter = 0;
            }
        }
    }

    void AttackHitBox()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll((Vector2)transform.position + Player.direction * attackRange, attackRadius);

        foreach (var collider in hitColliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy)
            {
                enemy.TakeDamage(attackDamage);

                Vector2 direction = ((Vector2)transform.position - (Vector2)enemy.transform.position).normalized * -1;
                Vector2 force = direction * knockbackForce * Time.deltaTime;
                enemy.rigid.MovePosition(enemy.rigid.position + force);
            }
        }
    }

    void ComboTime()
    {
        if (comboTimer < maxComboTime)
        {
            comboTimer += Time.deltaTime;
        }

        else
        {
            attackCounter = 0;
            comboTimer = maxComboTime;
        }
    }

    #region Enumerators
    public IEnumerator AttackMove(float time)
    {
        Player.motion = Vector2.zero;
        IsAttacking = true;
        attackMotion = Player.direction * attackMoveSpeed;
        Player.motion = attackMotion;
        yield return new WaitForSeconds(time);
        Player.motion = Vector2.zero;
        IsAttacking = false;
    }

    IEnumerator AttackWithStartDelay (float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(AttackMove(attackMoveTime));
        AttackHitBox();
    }

    public IEnumerator AttackEndDelay(float attackDelay)
    {
        yield return new WaitForSeconds(attackDelay);
        CanAttack = true;
        Player.CanMove = true;
    }
    #endregion
}
