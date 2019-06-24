using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpriteManager : MonoBehaviour
{
    public SpriteRenderer rend;
    private SpriteRenderer shadow;
    public Collider2D enemyBox;
    public float yPos;

    void Start()
    {
        rend = GetComponentInParent<SpriteRenderer>();
        enemyBox = transform.Find("Movement Collider").GetComponent<Collider2D>();

        if (CompareTag("Enemy"))
        {
            shadow = transform.Find("Shadow").GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        yPos = enemyBox.bounds.center.y;
        if (shadow)
        {
            if (shadow.sortingOrder != rend.sortingOrder - 1)
            {
                shadow.sortingOrder = rend.sortingOrder - 1;
            } 
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerSpriteManager player = other.GetComponent<PlayerSpriteManager>();
        if (player)
        {
            if (yPos < player.yPos)
            {
                if (rend.sortingOrder <= player.rend.sortingOrder)
                {
                    rend.sortingOrder = player.rend.sortingOrder + 1;
                }
            }

            else if (yPos >= player.yPos)
            {
                if (rend.sortingOrder >= player.rend.sortingOrder)
                {
                    rend.sortingOrder = player.rend.sortingOrder - 1;
                }
            }
        }

        PropSpriteManager prop = other.GetComponent<PropSpriteManager>();
        if (prop)
        {
            if (yPos < prop.yPos)
            {
                if (rend.sortingOrder <= prop.rend.sortingOrder)
                {
                    rend.sortingOrder = prop.rend.sortingOrder + 1;
                }
            }

            else if (yPos >= prop.yPos)
            {
                if (rend.sortingOrder >= prop.rend.sortingOrder)
                {
                    rend.sortingOrder = prop.rend.sortingOrder - 1;
                }
            }
        }

        EnemySpriteManager enemy = other.GetComponent<EnemySpriteManager>();
        if (enemy)
        {
            if (yPos < enemy.yPos)
            {
                if (rend.sortingOrder <= enemy.rend.sortingOrder)
                {
                    rend.sortingOrder = enemy.rend.sortingOrder + 1;
                }
            }

            else if (yPos >= enemy.yPos)
            {
                if (rend.sortingOrder >= enemy.rend.sortingOrder)
                {
                    rend.sortingOrder = enemy.rend.sortingOrder - 1;
                }
            }
        }
    }
}
