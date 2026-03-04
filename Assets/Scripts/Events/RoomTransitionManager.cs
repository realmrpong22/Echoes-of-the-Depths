using Cinemachine;
using Game.Player;
using System.Collections;
using UnityEngine;
using static RoomTransitionTrigger;

public class RoomTransitionManager : MonoBehaviour
{
    public static RoomTransitionManager Instance;

    public CinemachineVirtualCamera vcam;
    public CinemachineConfiner2D confiner;
    public Transform player;

    public float transitionTime = 0.7f;

    float screenWidth;
    float screenHeight;
    bool transitioning;

    void Awake()
    {
        Instance = this;

        Camera cam = Camera.main;
        screenHeight = cam.orthographicSize * 2;
        screenWidth = screenHeight * cam.aspect;
    }

    public void StartTransition(TransitionDirection dir, Collider2D newBounds)
    {
        if (transitioning) return;

        StartCoroutine(Transition(dir, newBounds));
    }

    IEnumerator Transition(TransitionDirection dir, Collider2D newBounds)
    {
        transitioning = true;

        player.GetComponent<PlayerController>().enabled = false;

        vcam.Follow = null;

        Vector3 start = vcam.transform.position;
        Vector3 end = start;

        switch (dir)
        {
            case TransitionDirection.Right:
                end += Vector3.right * screenWidth;
                break;

            case TransitionDirection.Left:
                end += Vector3.left * screenWidth;
                break;

            case TransitionDirection.Up:
                end += Vector3.up * screenHeight;
                break;

            case TransitionDirection.Down:
                end += Vector3.down * screenHeight;
                break;
        }

        float t = 0;

        while (t < transitionTime)
        {
            t += Time.deltaTime;
            vcam.transform.position = Vector3.Lerp(start, end, t / transitionTime);
            yield return null;
        }

        confiner.m_BoundingShape2D = newBounds;
        confiner.InvalidateCache();

        vcam.Follow = player;

        player.GetComponent<PlayerController>().enabled = true;

        transitioning = false;
    }
}