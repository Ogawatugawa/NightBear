//==================================
//Author: Vin Tansiri
//Title: PropSpriteManager.cs
//Date: 25 May 2019
//==================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropSpriteManager : MonoBehaviour
{
    public AlphaState state;
    public Color color;
    public SpriteRenderer rend;
    public Collider2D propCollider;
    public float yPos;

    public bool TransparencyOn;

    void Start()
    {
        color = GetComponent<SpriteRenderer>().color;
        rend = GetComponent<SpriteRenderer>();
        propCollider = transform.Find("Collider").GetComponent<Collider2D>();
        yPos = propCollider.bounds.center.y;
    }

    void Update()
    {
        if (TransparencyOn)
        {
            switch (state)
            {
                case AlphaState.AlphaUp:
                    if (color.a < 1f)
                    {
                        color.a += 2f * Time.deltaTime;
                        GetComponent<SpriteRenderer>().color = color;
                    }
                    break;
                case AlphaState.AlphaDown:
                    if (color.a > 0.5f)
                    {
                        color.a -= 2f * Time.deltaTime;
                        GetComponent<SpriteRenderer>().color = color;
                    }
                    break;
                default:
                    if (color.a > 0.5f)
                    {
                        color.a -= 2f * Time.deltaTime;
                        GetComponent<SpriteRenderer>().color = color;
                    }
                    break;
            }
        }
    }
}
public enum AlphaState { AlphaUp, AlphaDown }
