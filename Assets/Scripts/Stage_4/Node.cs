using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("��ǥ ����")]
    public float fallSpeed = 5f;
    public int damage = 1;
    public string groundTag = "Ground";

    [Header("�����")]
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
            Debug.Log($"��ǥ ����: {transform.position}");
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
                    Debug.Log($"��ǥ�� �÷��̾�� {damage} �������� �������ϴ�!");
                }
            }

            DestroyNote();
        }
        else if (other.CompareTag(groundTag))
        {
            if (showDebugInfo)
            {
                Debug.Log("��ǥ�� ���� ��� ������ϴ�.");
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
                Debug.Log("��ǥ�� ���� ��� ������ϴ�. (Collision)");
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
                Debug.Log("��ǥ�� ȭ�� ������ ���� �ڵ� ���ŵ˴ϴ�.");
            }

            Destroy(gameObject);
        }
    }
}
