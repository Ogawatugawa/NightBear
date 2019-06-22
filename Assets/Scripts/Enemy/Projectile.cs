using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public int damage;
    public float speed;
    public float knockbackForce;
    public Vector2 targetPos;
    public Vector2 direction;
    private Vector2 moveForce;
    private Rigidbody2D rigid;

    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        direction = (targetPos - (Vector2)transform.position).normalized;
        transform.up = direction;
        moveForce = direction * speed;
    }

    void Update()
    {
        transform.up = direction;
        rigid.MovePosition(rigid.position + moveForce * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player)
        {
            Vector2 playerPos = player.transform.position;
            playerPos.y += 0.25f;
            Vector2 direction = ((Vector2)transform.position - playerPos).normalized * -1f;
            Vector2 force = direction * knockbackForce * Time.deltaTime;

            player.TakeDamageWithKnockback(damage, force);
            Destroy(gameObject);
        }

        else if (collision.CompareTag("Impassable"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Camera Bounds"))
        {
            Destroy(gameObject);
            print(collision);
        }
    }
}
