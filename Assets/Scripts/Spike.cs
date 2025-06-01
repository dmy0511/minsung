using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : MonoBehaviour
{
    [Header("가시 설정")]
    public int damage = 1;

    [Header("디버그")]
    public bool showDebugInfo = false;

    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: Spike의 Collider2D가 Trigger로 설정되지 않았습니다!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (showDebugInfo)
            {
                Debug.Log($"플레이어가 가시({gameObject.name})에 닿았습니다!");
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
                    Debug.Log($"가시가 플레이어에게 {damage} 데미지를 입혔습니다!");
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.Log("플레이어가 무적 상태여서 데미지를 받지 않았습니다.");
                }
            }
        }
        else
        {
            Debug.LogWarning("플레이어에게서 PlayerHealth 컴포넌트를 찾을 수 없습니다!");
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
