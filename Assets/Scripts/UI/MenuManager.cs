using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public Image loadingScreen;

    public void Awake()
    {
        loadingScreen = GameObject.FindGameObjectWithTag("Loading Screen").GetComponent<Image>();
    }
    public void ChangeScene (int index)
    {
        SceneManager.LoadScene(index);
    }
}
