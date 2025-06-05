using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{
    [Header("충돌 설정")]
    public LayerMask playerLayer = 1;
    public bool destroyOnPlayerHit = true;
    public bool destroyOnWallHit = true;

    [Header("데미지 설정")]
    public int damage = 1;

    [Header("회전 설정")]
    public bool autoRotateToVelocity = true;
    public bool useStraightLine = true;

    [Header("디버그 설정")]
    public bool showHeadDirection = true;
    public float headGizmoSize = 0.2f;
    public Vector2 headOffset = new Vector2(0, 0.5f);

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isLaunched = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (useStraightLine && rb != null)
        {
            rb.gravityScale = 0f;
        }

        Destroy(gameObject, 10f);
    }

    private void Update()
    {
        if (autoRotateToVelocity && rb != null && isLaunched && rb.velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void LaunchStraight(Vector2 direction, float speed, bool throwingRight)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;

        rb.velocity = direction * speed;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !throwingRight;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        isLaunched = true;

        Debug.Log($"창 일직선 발사 - 방향: {direction}, 속도: {rb.velocity}, 각도: {angle}도, 오른쪽: {throwingRight}");

        Vector3 headWorldPosition = transform.TransformPoint(headOffset);
        Debug.Log($"창 머리 위치: {headWorldPosition}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsInLayerMask(other.gameObject, playerLayer))
        {
            OnPlayerHit(other);

            if (destroyOnPlayerHit)
            {
                DestroySpear();
            }
        }
        else if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            OnWallHit(other);

            if (destroyOnWallHit)
            {
                
            }
        }
    }

    private void OnPlayerHit(Collider2D playerCollider)
    {
        Debug.Log("창이 플레이어에게 명중!");

        PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
        if (playerHealth != null && !playerHealth.IsInvincible())
        {
            playerHealth.TakeDamage(damage);
        }
    }

    private void OnWallHit(Collider2D wallCollider)
    {
        Debug.Log("창이 벽에 충돌!");
    }

    private void DestroySpear()
    {
        Destroy(gameObject);
    }

    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return ((layerMask.value & (1 << obj.layer)) > 0);
    }

    void OnDrawGizmos()
    {
        if (!showHeadDirection) return;

        Vector3 headWorldPosition = transform.TransformPoint(headOffset);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(headWorldPosition, headGizmoSize);

        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.yellow;
            Vector3 velocityDirection = rb.velocity.normalized;
            Gizmos.DrawRay(headWorldPosition, velocityDirection * 1f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(headWorldPosition + Vector3.up * 0.5f, $"Velocity: {rb.velocity}\nAngle: {Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg:F1}°");
#endif
        }

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.right * 0.5f); // X축 (오른쪽)
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.up * 0.5f);    // Y축 (위쪽)
        }
    }
}
