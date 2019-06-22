using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpriteManager : MonoBehaviour
{
    public SpriteRenderer rend;
    private SpriteRenderer shadow;
    public BoxCollider2D playerBox;
    public float yPos;

    void Start()
    {
        playerBox = GameObject.FindGameObjectWithTag("Feet").GetComponent<BoxCollider2D>();
        rend = GetComponentInParent<SpriteRenderer>();
        shadow = GameObject.Find("Bear Shadow").GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        print(yPos);
        yPos = playerBox.bounds.center.y;
        if (shadow.sortingOrder != rend.sortingOrder)
        {
            shadow.sortingOrder = rend.sortingOrder;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PropSpriteManager prop = other.GetComponent<PropSpriteManager>();
        if (prop)
        {
            if (yPos < prop.yPos)
            {
                if (rend.sortingOrder <= prop.rend.sortingOrder)
                {
                    rend.sortingOrder = prop.rend.sortingOrder + 1; 
                }
                prop.state = AlphaState.AlphaUp;
            }

            else if (yPos >= prop.yPos)
            {
                if (rend.sortingOrder >= prop.rend.sortingOrder)
                {
                    rend.sortingOrder = prop.rend.sortingOrder - 1; 
                }
                prop.state = AlphaState.AlphaDown;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PropSpriteManager prop = other.GetComponent<PropSpriteManager>();
        if (prop)
        {
            if (prop.state != AlphaState.AlphaUp)
            {
                prop.state = AlphaState.AlphaUp;
            }
        }
    }
}
