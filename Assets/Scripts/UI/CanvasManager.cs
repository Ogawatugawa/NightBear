using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public Image deathScreen;
    public bool DeathScreenOn;
    // Start is called before the first frame update
    void Start()
    {
        deathScreen = GameObject.FindGameObjectWithTag("Death Screen").GetComponent<Image>();
        deathScreen.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (DeathScreenOn)
        {
            if (!deathScreen.isActiveAndEnabled)
            {
                deathScreen.gameObject.SetActive(true);
            }

            Color color = deathScreen.color;

            if (deathScreen.color.a < 1)
            {
                color.a += 0.5f * Time.deltaTime;
            }

            deathScreen.color = color;
        }
    }
}
