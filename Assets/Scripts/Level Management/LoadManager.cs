using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class LoadManager : MonoBehaviour
{
    public CinemachineFramingTransposer cam;
    public Image loadingPanel;
    public Transform exitPoint;
    public LoadState state = LoadState.NotLoading;
    public bool HasDelayed = false;
    GameObject player;
    // Start is called before the first frame update
    void Start()
    {

        loadingPanel = GameObject.FindGameObjectWithTag("Loading Screen").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        Color color = loadingPanel.color;
        switch (state)
        {
            case LoadState.LoadIn:
                if (color.a < 1)
                {
                    color.a += 2 * Time.deltaTime;
                    loadingPanel.color = color;

                    player = GameObject.FindGameObjectWithTag("Player");
                    float moveSpeed = player.GetComponent<Player>().moveSpeed;
                    Player.CanMove = false;
                    Player.isDodging = false;
                    Player.motion = Player.direction * moveSpeed;
                }

                else if (!HasDelayed)
                {
                    StartCoroutine(LoadOutStartDelay(0.75f));
                }
                break;
            case LoadState.LoadOut:
                if (color.a > 0)
                {
                    Player.CanMove = true;
                    Player.motion = Vector2.zero;
                    color.a -= 2 * Time.deltaTime;
                    loadingPanel.color = color;
                }
                else
                {
                    state = LoadState.NotLoading;
                }
                break;
            case LoadState.NotLoading:
                if (HasDelayed)
                {
                    HasDelayed = false;
                }
                break;
            default:
                break;
        }
    }

    IEnumerator LoadOutStartDelay(float delay)
    {
        HasDelayed = true;
        yield return new WaitForSeconds(delay);
        player.transform.position = exitPoint.position;
        state = LoadState.LoadOut;
    }
}
public enum LoadState { LoadIn, LoadOut, NotLoading }