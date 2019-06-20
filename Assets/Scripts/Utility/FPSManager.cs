using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSManager : MonoBehaviour
{
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        //// Make the game run as fast as possible
        //Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (Application.targetFrameRate != 60)
        {
            Application.targetFrameRate = 60;
        }
    }
}

