//==================================
//Author: Vin Tansiri
//Title: LoadingZone.cs
//Date: 11 June 2019
//==================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingZone : MonoBehaviour
{
    public Transform exitPoint;
    public LoadManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("Managers").GetComponent<LoadManager>();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Feet"))
        {
            manager.HasDelayed = false;
            manager.exitPoint = exitPoint;
            manager.state = LoadState.LoadIn;
        }
    }
}