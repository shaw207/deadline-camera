using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour, IRelativeObjects
{
    public enum Mode { OneShot, PingPong, Loop }

    [Header("Target (the object that will move)")]
    [SerializeField] private Transform target;

    [Header("Path Settings")]
    [SerializeField] private Transform[] waypoints;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private Mode mode = Mode.OneShot;

    [Header("Auto start?")]
    [SerializeField] private bool autoStart = false;

    [Header("Snap target to first waypoint on Start")]
    [SerializeField] private bool snapToFirstWaypoint = true;

    [Header("Drawer SFX")]
    [SerializeField] private bool playDrawerSfx = false;
    [SerializeField] private string sfxOpen = "Drawer_Open";
    [SerializeField] private string sfxClose = "Drawer_Close";

    private int currentIndex = 0;
    private bool isMoving = false;
    private bool forward = true;
    private Coroutine moveRoutine;

    private void Awake()
    {
        if (target == null)
            target = transform;
    }

    private void Start()
    {
        if (snapToFirstWaypoint && waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
        {
            target.position = waypoints[0].position;
            currentIndex = 0;
            forward = true;
        }

        if (autoStart)
            StartMovement();
    }

    public void action()
    {
        StartMovement();
    }

    public void action(object obj)
    {
        action();
    }

    public void Noaction()
    {
        StopMovement();
    }

    public void Noaction(object obj)
    {
        Noaction();
    }

    public bool checkStatu()
    {
        return !isMoving;
    }

    public bool UseParam()
    {
        return false;
    }

    private void StartMovement()
    {
        if (isMoving) return;

        if (playDrawerSfx && !string.IsNullOrWhiteSpace(sfxOpen))
            DeadLine.SoundManager.Instance?.PlayOneShotSound(sfxOpen);

        moveRoutine = StartCoroutine(MoveRoutine());
    }

    private void StopMovement()
    {
        if (moveRoutine == null && !isMoving) return;

        if (playDrawerSfx && !string.IsNullOrWhiteSpace(sfxClose))
            DeadLine.SoundManager.Instance?.PlayOneShotSound(sfxClose);

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        isMoving = false;
    }

    private IEnumerator MoveRoutine()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning("MovingPlatform needs at least 2 waypoints!");
            yield break;
        }

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                Debug.LogWarning($"MovingPlatform waypoint at index {i} is null!");
                yield break;
            }
        }

        isMoving = true;

        while (true)
        {
            int nextIndex = GetNextIndex();
            Vector3 targetPos = waypoints[nextIndex].position;

            while ((target.position - targetPos).sqrMagnitude > 0.001f)
            {
                target.position = Vector3.MoveTowards(
                    target.position,
                    targetPos,
                    speed * Time.deltaTime
                );

                yield return null;
            }

            target.position = targetPos;
            currentIndex = nextIndex;

            if (mode == Mode.OneShot && currentIndex == waypoints.Length - 1)
                break;
        }

        isMoving = false;
        moveRoutine = null;
    }

    private int GetNextIndex()
    {
        if (mode == Mode.OneShot)
            return Mathf.Min(currentIndex + 1, waypoints.Length - 1);

        if (mode == Mode.Loop)
            return (currentIndex + 1) % waypoints.Length;

        if (forward)
        {
            if (currentIndex == waypoints.Length - 1)
            {
                forward = false;
                return currentIndex - 1;
            }
            return currentIndex + 1;
        }
        else
        {
            if (currentIndex == 0)
            {
                forward = true;
                return currentIndex + 1;
            }
            return currentIndex - 1;
        }
    }
}
