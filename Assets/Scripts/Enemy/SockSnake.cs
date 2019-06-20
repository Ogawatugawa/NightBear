using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class SockSnake : Enemy
{
    [Header("Hitbox")]
    private float hitboxRadius;
    private Vector2 globalHitboxPos;
    private Vector2 localHitboxPos;
    private bool IsLunging = false;
    private bool IsAttacking = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(localHitboxPos, hitboxRadius);
        Gizmos.DrawSphere(attackTargetPos, 0.1f);
    }

    public void MouthHitbox()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(localHitboxPos, hitboxRadius);
        foreach (var hit in hits)
        {
            Player player = hit.GetComponent<Player>();
            if (player)
            {
                Vector2 playerPos = player.transform.position;
                playerPos.y += 0.25f;
                Vector2 direction = (localHitboxPos - playerPos).normalized * -1f;
                Vector2 force = direction * knockbackForce * Time.deltaTime;

                Debug.DrawRay(transform.position, direction);

                player.TakeDamageWithKnockback(damage, force);
            }
        }
    }

    public void AttackMove()
    {
        Vector2 moveForce = attackDirection * 300 * Time.deltaTime;
        rigid.velocity = moveForce;
    }


    public override void Attack()
    {
        base.Attack();

        if (attackCooldownTimer >= attackCooldownMax)
        {
            StartCoroutine(AttackDelays(attackStartDelay, attackEndDelay));
        }

        if (attackCooldownTimer < attackCooldownMax)
        {
            direction = ((Vector2)target.transform.position - rigid.position).normalized;
            rend.flipX = attackDirection.x > 0.1f;
            rigid.velocity = Vector2.zero;
        }

        if (IsLunging)
        {
            AttackMove();

            if (IsAttacking)
            {
                MouthHitbox();
            }
        }
    }

    public override void Update()
    {
        localHitboxPos = (Vector2)transform.position + globalHitboxPos;
        if (direction.x > 0)
        {
            localHitboxPos.x += 0.3f;
        }

        base.Update();
    }

    public void MoveBool()
    {
        if (!IsLunging)
        {
            IsLunging = true;
        }

        else
        {
            IsLunging = false;
        }
    }

    public void AttackBool()
    {
        if (!IsAttacking)
        {
            IsAttacking = true;
        }

        else
        {
            IsAttacking = false;
        }
    }
}
