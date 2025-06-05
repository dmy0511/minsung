using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedEyeTracker : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;

    [Header("Eye Movement Settings")]
    public float eyeMovementSpeed = 3f;
    public float maxEyeDistance = 0.2f;

    [Header("Eye Constraint")]
    public Transform eyeBoundary;
    public float boundaryRadius = 0.5f;
    public bool visualizeBoundary = true;

    [Header("Eye Visual")]
    public Transform eyePupil;

    private Vector3 originalLocalPosition;

    void Start()
    {
        if (eyePupil == null)
        {
            eyePupil = transform;
        }

        if (eyeBoundary == null)
        {
            eyeBoundary = transform;
        }

        originalLocalPosition = eyePupil.localPosition;

        FindPlayer();
    }

    void Update()
    {
        if (player == null) return;

        TrackPlayer();
    }

    void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void TrackPlayer()
    {
        Vector2 eyeWorldPosition = eyeBoundary.position;
        Vector2 playerWorldPosition = player.position;
        Vector2 directionToPlayer = (playerWorldPosition - eyeWorldPosition).normalized;

        Vector2 localDirection = eyeBoundary.InverseTransformDirection(directionToPlayer);

        Vector2 eyeOffset = localDirection * maxEyeDistance;
        Vector3 targetLocalPosition = originalLocalPosition + new Vector3(eyeOffset.x, eyeOffset.y, 0);

        Vector3 constrainedPosition = ConstrainToBoundary(targetLocalPosition);

        eyePupil.localPosition = Vector3.Lerp(
            eyePupil.localPosition,
            constrainedPosition,
            eyeMovementSpeed * Time.deltaTime
        );
    }

    Vector3 ConstrainToBoundary(Vector3 targetPosition)
    {
        Vector3 offset = targetPosition - originalLocalPosition;

        if (offset.magnitude > boundaryRadius)
        {
            offset = offset.normalized * boundaryRadius;
        }

        return originalLocalPosition + offset;
    }

    void OnDrawGizmosSelected()
    {
        if (eyeBoundary != null && visualizeBoundary)
        {
            Gizmos.color = Color.cyan;
            DrawWireCircle(eyeBoundary.position, boundaryRadius * Mathf.Max(eyeBoundary.lossyScale.x, eyeBoundary.lossyScale.y));

            Gizmos.color = Color.red;
            Vector3 scaledOriginalPos = eyeBoundary.TransformPoint(originalLocalPosition);
            DrawWireCircle(scaledOriginalPos, maxEyeDistance * Mathf.Max(eyeBoundary.lossyScale.x, eyeBoundary.lossyScale.y));
        }

        if (player != null)
        {
            Gizmos.color = Color.green;
            Vector3 eyePos = eyeBoundary != null ? eyeBoundary.position : transform.position;
            Gizmos.DrawLine(eyePos, player.position);

            // 현재 눈동자 위치 표시
            if (eyePupil != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(eyePupil.position, 0.05f);
            }
        }
    }

    void DrawWireCircle(Vector3 center, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    [ContextMenu("Reset Pupil Position")]
    public void ResetPupilPosition()
    {
        if (eyePupil != null)
        {
            eyePupil.localPosition = originalLocalPosition;
        }
    }

    [ContextMenu("Find Player")]
    public void FindPlayerManually()
    {
        FindPlayer();
        if (player != null)
        {
            Debug.Log($"플레이어 찾음: {player.name}");
        }
        else
        {
            Debug.LogWarning("플레이어를 찾을 수 없습니다. 'Player' 태그가 있는지 확인해주세요.");
        }
    }
}
