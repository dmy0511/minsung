using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("음표 설정")]
    public float fallSpeed = 5f;
    public int damage = 1;
    public string groundTag = "Ground";

    [Header("디버그")]
    public bool showDebugInfo = false;

    private Rigidbody2D rb;
    private bool hasHitPlayer = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0;
        rb.velocity = Vector2.down * fallSpeed;

        if (showDebugInfo)
        {
            Debug.Log($"음표 생성: {transform.position}");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasHitPlayer)
        {
            hasHitPlayer = true;

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsInvincible())
            {
                playerHealth.TakeDamage(damage);

                if (showDebugInfo)
                {
                    Debug.Log($"음표가 플레이어에게 {damage} 데미지를 입혔습니다!");
                }
            }

            DestroyNote();
        }
        else if (other.CompareTag(groundTag))
        {
            if (showDebugInfo)
            {
                Debug.Log("음표가 땅에 닿아 사라집니다.");
            }

            DestroyNote();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            if (showDebugInfo)
            {
                Debug.Log("음표가 땅에 닿아 사라집니다. (Collision)");
            }

            DestroyNote();
        }
    }

    void DestroyNote()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        if (transform.position.y < -10f)
        {
            if (showDebugInfo)
            {
                Debug.Log("음표가 화면 밖으로 나가 자동 제거됩니다.");
            }

            Destroy(gameObject);
        }
    }
}
