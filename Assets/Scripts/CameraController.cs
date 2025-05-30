using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("카메라 설정")]
    public Camera cam;
    public Transform player;

    [Header("줌 설정")]
    public float minZoom = 1f;
    public float maxZoom = 5f;
    public float zoomSpeed = 3f;
    public float playerFollowZoom = 2.5f;

    [Header("드래그 설정")]
    public float dragSpeed = 3f;

    [Header("카메라 경계 설정")]
    public float leftBound = -10f;
    public float rightBound = 10f;
    public float topBound = 5f;
    public float bottomBound = -5f;

    [Header("플레이어 추적 설정")]
    public float followSpeed = 5f;
    public float playerCheckInterval = 0.1f;

    private Vector3 lastMousePosition;
    private bool isDragging = false;
    private bool isFollowingPlayer = false;
    private Vector3 lastPlayerPosition;
    private float playerIdleTime = 0f;
    private float maxIdleTime = 3f;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        if (player != null)
        {
            lastPlayerPosition = player.position;
            InvokeRepeating("CheckPlayerMovement", 0f, playerCheckInterval);
        }
    }

    void Update()
    {
        HandleZoom();
        HandleDrag();

        if (isFollowingPlayer && player != null)
        {
            FollowPlayer();
        }

        ClampCameraPosition();
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            if (!isFollowingPlayer)
            {
                float newSize = cam.orthographicSize - scroll * zoomSpeed;
                cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            }
        }
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;

            StopFollowingPlayer();
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 currentMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = lastMousePosition - currentMousePosition;

            transform.position += difference * dragSpeed;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void CheckPlayerMovement()
    {
        if (player == null) return;

        Vector3 currentPlayerPosition = player.position;
        float movementDistance = Vector3.Distance(currentPlayerPosition, lastPlayerPosition);

        if (movementDistance > 0.01f)
        {
            playerIdleTime = 0f;

            if (!isFollowingPlayer)
            {
                StartFollowingPlayer();
            }
        }
        else
        {
            playerIdleTime += playerCheckInterval;

            if (playerIdleTime >= maxIdleTime && isFollowingPlayer)
            {
                StopFollowingPlayer();
            }
        }

        lastPlayerPosition = currentPlayerPosition;
    }

    void StartFollowingPlayer()
    {
        isFollowingPlayer = true;

        StartCoroutine(SmoothZoom(playerFollowZoom));
    }

    void StopFollowingPlayer()
    {
        isFollowingPlayer = false;
    }

    void FollowPlayer()
    {
        if (player == null) return;

        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    System.Collections.IEnumerator SmoothZoom(float targetSize)
    {
        float currentSize = cam.orthographicSize;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            cam.orthographicSize = Mathf.Lerp(currentSize, targetSize, t);
            yield return null;
        }

        cam.orthographicSize = targetSize;
    }

    void ClampCameraPosition()
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float clampedX = Mathf.Clamp(transform.position.x, leftBound + camWidth, rightBound - camWidth);
        float clampedY = Mathf.Clamp(transform.position.y, bottomBound + camHeight, topBound - camHeight);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            new Vector3((leftBound + rightBound) / 2f, (bottomBound + topBound) / 2f, 0f),
            new Vector3(rightBound - leftBound, topBound - bottomBound, 0f)
        );
    }
}
