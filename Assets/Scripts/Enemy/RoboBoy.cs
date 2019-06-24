//==================================
//Author: Alex Liu, Vin Tansiri
//Title: RoboBoy.cs
//Date: 15 June 2019
//==================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class RoboBoy : Enemy
{
    [Header("Robot Move")]
    public bool CanMove = true;

    [Header("Laser Variables")]
    public float laserSpeed;
    public GameObject laserPrefab;
    private Transform eyes;

    [Header("Death Explosion")]
    public int explosionDamage;
    public float explosionRadius;
    public float explosionForce;

    public override void Start()
    {
        base.Start();
        eyes = transform.Find("Eyes").transform;
    }

    public override void Move(Vector2 force)
    {
        if (!anim.GetBool("IsMoving"))
        {
            anim.SetBool("IsMoving", true);
        }

        if (CanMove)
        {
            rend.flipX = direction.x < 0.1f;

            rigid.MovePosition(rigid.position + force * Time.deltaTime);
        }
    }

    public void MoveBool()
    {
        if (!CanMove)
        {
            CanMove = true;
        }

        else
        {
            CanMove = false;
        }
    }

    public override void Attack()
    {
        base.Attack();

        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);

        if (attackCooldownTimer >= attackCooldownMax)
        {
            rigid.velocity = Vector2.zero;
            if (CanAttack)
            {
                CanAttack = false;
                GetTargetPos();
                anim.SetTrigger("EnemyAttack");
                StartCoroutine(ShootLaserWithDelay(0.6f));
                StartCoroutine(ShootLaserWithDelay(1.27f));
                StartCoroutine(MoveAfterAttack(1.5f));
            }
        }

        else if (distanceToTarget < attackAttemptRadius/2)
        {
            direction = (transform.position - target.position).normalized;
            force = direction * moveSpeed * Time.deltaTime;
            Move(force);
        }

        else if (distanceToTarget < (attackAttemptRadius/3)*2 + 0.25f && distanceToTarget > (attackAttemptRadius/3)*2 - 0.25f)
        {
            return;
        }

        else
        {
            AttackMode = false;
        }
    }

    IEnumerator ShootLaserWithDelay(float delay)
    {
        print("shooting laser");
        yield return new WaitForSeconds(delay);
        if (!IsDying)
        {
            GameObject laser = Instantiate(laserPrefab, eyes.position, Quaternion.identity);
            Projectile projectile = laser.GetComponent<Projectile>();
            projectile.transform.up = (Vector3)projectile.targetPos + projectile.transform.position;
            projectile.damage = damage;
            projectile.knockbackForce = knockbackForce;
            projectile.speed = laserSpeed;
            projectile.targetPos = attackTargetPos; 
        }
    }

    IEnumerator MoveAfterAttack(float delay)
    {
        yield return new WaitForSeconds(delay);
        attackCooldownTimer = 0f;
        CanAttack = true;
    }

    public void GetTargetPos()
    {
        print("Getting target");
        attackTargetPos = target.position;
        attackDirection = (attackTargetPos - rigid.position).normalized;
        rend.flipX = attackDirection.x < 0f;
    }

    public override void Death()
    {
        base.Death();
        StartCoroutine(ExplodeWithDelay(deathDelay));
    }

    IEnumerator ExplodeWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        anim.SetTrigger("IsDead");
    }

    public void DeathExplosion()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            Player player = hit.GetComponent<Player>();
            if (player)
            {
                Vector2 playerPos = player.transform.position;
                playerPos.y += 0.25f;
                Vector2 direction = ((Vector2)transform.position - playerPos).normalized * -1f;
                Vector2 force = direction * knockbackForce * Time.deltaTime;

                player.TakeDamageWithKnockback(explosionDamage, force);
            } 
        }

        GameObject shadow = transform.Find("SpriteManager").transform.Find("Shadow").gameObject;
        Destroy(shadow);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
