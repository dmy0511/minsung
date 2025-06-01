using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    [Header("���� ����")]
    public int damage = 1;

    [Header("�����")]
    public bool showDebugInfo = false;

    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: Spike�� Collider2D�� Trigger�� �������� �ʾҽ��ϴ�!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (showDebugInfo)
            {
                Debug.Log($"�÷��̾ ����({gameObject.name})�� ��ҽ��ϴ�!");
            }

            DamagePlayer(other);
        }
    }

    void DamagePlayer(Collider2D playerCollider)
    {
        PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            if (!playerHealth.IsInvincible())
            {
                playerHealth.TakeDamage(damage);

                if (showDebugInfo)
                {
                    Debug.Log($"���ð� �÷��̾�� {damage} �������� �������ϴ�!");
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.Log("�÷��̾ ���� ���¿��� �������� ���� �ʾҽ��ϴ�.");
                }
            }
        }
        else
        {
            Debug.LogWarning("�÷��̾�Լ� PlayerHealth ������Ʈ�� ã�� �� �����ϴ�!");
        }
    }

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    public int GetDamage()
    {
        return damage;
    }
}
