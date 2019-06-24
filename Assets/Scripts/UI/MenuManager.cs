//==================================
//Author: Vin Tansiri
//Title: MenuManager.cs
//Date: 19 June 2019
//==================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public LoadManager lm;
    public Image loadingScreen;
    public bool IsLoading;
    public int sceneIndex;

    public void Awake()
    {
        lm = GetComponent<LoadManager>();
        loadingScreen = GameObject.FindGameObjectWithTag("Loading Screen").GetComponent<Image>();
    }

    public void ChangeScene (int index)
    {
        IsLoading = true;
        sceneIndex = index;
    }

    public IEnumerator ChangeSceneWithFadeout (int index, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(index);
    }

    private void Update()
    {
        if(IsLoading)
        {
            IsLoading = false;
            lm.state = LoadState.LoadIn;
            StartCoroutine(ChangeSceneWithFadeout(sceneIndex, 1.5f));
        }
    }
}
