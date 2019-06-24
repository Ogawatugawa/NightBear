using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public Image deathScreen;
    public GameObject pauseScreen;
    public bool DeathScreenOn;
    // Start is called before the first frame update
    void Awake()
    {
        deathScreen = GameObject.FindGameObjectWithTag("Death Screen").GetComponent<Image>();
        pauseScreen.SetActive(false);
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

        if (Input.GetKeyDown(KeyCode.Escape) && !pauseScreen.activeInHierarchy)
        {
            PauseGame(true);
        }

        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame(false);
        }
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void PauseGame(bool IsPaused)
    {
        if (IsPaused)
        {
            Time.timeScale = 0;
            pauseScreen.SetActive(true);
        }

        else
        {
            Time.timeScale = 1;
            pauseScreen.SetActive(false);
        }
    }
}
