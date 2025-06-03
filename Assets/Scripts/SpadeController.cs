using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpadeController : MonoBehaviour
{
    [Header("플레이어 감지 설정")]
    public Vector2 detectionBoxSize = new Vector2(10f, 8f);
    public Vector2 detectionBoxOffset = Vector2.zero;
    public LayerMask playerLayer = 1;

    [Header("머리 밟기 감지")]
    public Transform headCheck;
    public float headCheckRadius = 0.3f;

    [Header("눌림 효과 설정")]
    public float crushedScale = 0.5f;
    public float crushAnimationTime = 0.2f;

    [Header("돌진 설정")]
    public float chargeSpeed = 15f;
    public float chargeDuration = 1f;
    public float cooldownTime = 2f;

    [Header("애니메이션 설정")]
    public string idleAnimName = "Idle";
    public string attackAnimName = "Attack";

    [Header("디버그")]
    public bool showDetectionRange = true;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private BoxCollider2D enemyCollider;
    private BoxCollider2D triggerCollider;

    private enum SpadeState
    {
        Idle,
        Preparing,
        Charging,
        Cooldown,
        Crushed,
        Dead
    }

    private SpadeState currentState = SpadeState.Idle;
    private Vector2 chargeDirection;
    private float stateTimer;

    private bool isCrushed = false;
    private bool isBeingCrushed = false;
    private Vector3 originalScale;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private Vector3 originalHeadCheckPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.mass = 100f;
            rb.drag = 5f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();

        foreach (BoxCollider2D col in colliders)
        {
            if (col.isTrigger)
            {
                triggerCollider = col;
            }
            else
            {
                enemyCollider = col;
            }
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
            Debug.Log("트리거 콜라이더 초기 비활성화");
        }
        else
        {
            Debug.LogWarning("트리거 콜라이더를 찾을 수 없습니다. isTrigger가 true로 설정된 BoxCollider2D가 있는지 확인하세요.");
        }

        originalScale = transform.localScale;
        if (enemyCollider != null)
        {
            originalColliderSize = enemyCollider.size;
            originalColliderOffset = enemyCollider.offset;
        }

        if (headCheck == null)
        {
            GameObject checkObj = new GameObject("HeadCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, 0.5f, 0);
            headCheck = checkObj.transform;
        }

        originalHeadCheckPosition = headCheck.localPosition;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }

        if (animator == null)
        {
            Debug.LogWarning("Animator 컴포넌트가 없습니다. 애니메이션을 재생하려면 Animator를 추가해주세요.");
        }

        currentState = SpadeState.Idle;
        PlayAnimation(idleAnimName);
    }

    void FixedUpdate()
    {
        PreventPlayerPush();
    }

    void Update()
    {
        if (currentState != SpadeState.Dead)
        {
            CheckIfPlayerOnHead();
        }

        if (currentState == SpadeState.Crushed || currentState == SpadeState.Dead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        switch (currentState)
        {
            case SpadeState.Idle:
                HandleIdleState();
                break;

            case SpadeState.Preparing:
                HandlePreparingState();
                break;

            case SpadeState.Charging:
                HandleChargingState();
                break;

            case SpadeState.Cooldown:
                HandleCooldownState();
                break;
        }

        UpdateStateTimer();
    }

    void PreventPlayerPush()
    {
        if (currentState == SpadeState.Crushed || currentState == SpadeState.Dead)
            return;

        if (rb != null)
        {
            if (currentState != SpadeState.Charging)
            {
                Vector2 velocity = rb.velocity;
                velocity.x = 0f;

                if (velocity.y > 2f)
                {
                    velocity.y = Mathf.Min(velocity.y, 0f);
                }

                rb.velocity = velocity;
            }
        }
    }

    void HandleIdleState()
    {
        if (player != null && IsPlayerInRange())
        {
            StartPreparing();
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    void HandlePreparingState()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (player != null)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            chargeDirection = directionToPlayer;

            if (spriteRenderer != null)
            {
                bool facingRight = chargeDirection.x > 0;
                spriteRenderer.flipX = facingRight;
                UpdateHeadCheckPosition(facingRight);
            }
        }
    }

    void HandleChargingState()
    {
        Vector2 targetVelocity = chargeDirection * chargeSpeed;
        
        targetVelocity.y = rb.velocity.y;
        rb.velocity = targetVelocity;

        Debug.Log($"Charging! Velocity set to: {targetVelocity}, Actual RB velocity: {rb.velocity}");

        if (stateTimer >= chargeDuration)
        {
            Debug.Log("Charge duration ended");
            StartCooldown();
        }
    }

    void HandleCooldownState()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (stateTimer >= cooldownTime)
        {
            SetState(SpadeState.Idle);
            PlayAnimation(idleAnimName);
        }
    }

    void UpdateHeadCheckPosition(bool facingRight)
    {
        if (headCheck != null)
        {
            float xPos = facingRight ? originalHeadCheckPosition.x : -originalHeadCheckPosition.x;
            headCheck.localPosition = new Vector3(xPos, originalHeadCheckPosition.y, originalHeadCheckPosition.z);
        }
    }

    bool IsPlayerInRange()
    {
        if (player == null) return false;

        Vector2 boxCenter = (Vector2)transform.position + detectionBoxOffset;
        Vector2 playerPos = player.position;

        float halfWidth = detectionBoxSize.x * 0.5f;
        float halfHeight = detectionBoxSize.y * 0.5f;

        bool insideX = playerPos.x >= boxCenter.x - halfWidth && playerPos.x <= boxCenter.x + halfWidth;
        bool insideY = playerPos.y >= boxCenter.y - halfHeight && playerPos.y <= boxCenter.y + halfHeight;

        return insideX && insideY;
    }

    void StartPreparing()
    {
        if (player != null)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            chargeDirection = directionToPlayer;

            if (spriteRenderer != null)
            {
                bool facingRight = chargeDirection.x > 0;
                spriteRenderer.flipX = facingRight;
                UpdateHeadCheckPosition(facingRight);
            }
        }

        SetState(SpadeState.Preparing);
        PlayAnimation(attackAnimName);
    }

    public void StartChargeFromAnimation()
    {
        Debug.Log("=== Animation Event Called! ===");
        Debug.Log($"Current State before change: {currentState}");
        Debug.Log($"Charge Direction: {chargeDirection}");
        Debug.Log($"Charge Speed: {chargeSpeed}");

        SetState(SpadeState.Charging);

        EnableTriggerCollider();

        Debug.Log($"State changed to: {currentState}");
    }

    void StartCooldown()
    {
        SetState(SpadeState.Cooldown);

        DisableTriggerCollider();

        PlayAnimation(idleAnimName);
    }

    void EnableTriggerCollider()
    {
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
            Debug.Log("트리거 콜라이더 활성화 - 돌진 상태에서 플레이어 감지 가능");
        }
    }

    void DisableTriggerCollider()
    {
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
            Debug.Log("트리거 콜라이더 비활성화 - 돌진 상태가 아님");
        }
    }

    void SetState(SpadeState newState)
    {
        if (currentState == SpadeState.Charging && newState != SpadeState.Charging)
        {
            DisableTriggerCollider();
        }

        currentState = newState;
        stateTimer = 0f;
    }

    void UpdateStateTimer()
    {
        stateTimer += Time.deltaTime;
    }

    void PlayAnimation(string animationName)
    {
        if (animator == null) return;

        if (currentState == SpadeState.Crushed || currentState == SpadeState.Dead) return;

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            animator.Play(animationName);
        }
    }

    void CheckIfPlayerOnHead()
    {
        if (isBeingCrushed) return;

        Collider2D playerCollider = Physics2D.OverlapCircle(headCheck.position, headCheckRadius, playerLayer);

        if (playerCollider != null)
        {
            Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();
            if (playerRb != null && playerRb.velocity.y < 0)
            {
                playerRb.velocity = new Vector2(playerRb.velocity.x, 10f);

                PlayerController playerController = playerCollider.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.ForcePlayJumpAnimation();
                }

                if (!isCrushed)
                {
                    StartCoroutine(CrushAnimation());
                }
                else
                {
                    StartCoroutine(DestroyEnemy());
                }
            }
        }
    }

    IEnumerator CrushAnimation()
    {
        isBeingCrushed = true;

        SetState(SpadeState.Crushed);

        if (animator != null)
        {
            animator.enabled = false;
        }

        float elapsed = 0;
        while (elapsed < crushAnimationTime)
        {
            float t = elapsed / crushAnimationTime;
            float scaleY = Mathf.Lerp(originalScale.y, originalScale.y * crushedScale, t);

            transform.localScale = new Vector3(originalScale.x, scaleY, originalScale.z);

            if (enemyCollider != null)
            {
                enemyCollider.size = new Vector2(
                    originalColliderSize.x,
                    originalColliderSize.y * (scaleY / originalScale.y)
                );

                enemyCollider.offset = new Vector2(
                    originalColliderOffset.x,
                    originalColliderOffset.y * (scaleY / originalScale.y)
                );
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = new Vector3(originalScale.x, originalScale.y * crushedScale, originalScale.z);

        isCrushed = true;
        isBeingCrushed = false;
    }

    IEnumerator DestroyEnemy()
    {
        isBeingCrushed = true;
        SetState(SpadeState.Dead);

        if (spriteRenderer != null)
        {
            float fadeTime = 0.5f;
            float elapsed = 0;
            Color originalColor = spriteRenderer.color;

            while (elapsed < fadeTime)
            {
                float alpha = Mathf.Lerp(1, 0, elapsed / fadeTime);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == SpadeState.Charging)
        {
            if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Ground"))
            {
                StartCooldown();
            }
            else if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null && !playerHealth.IsInvincible())
                {
                    playerHealth.TakeDamage(1);
                    Debug.Log("Spade가 플레이어에게 돌진 공격!");
                }
                StartCooldown();
            }
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 pushDirection = (collision.transform.position - transform.position).normalized;
                pushDirection.y = Mathf.Max(pushDirection.y, 0);
                playerRb.AddForce(pushDirection * 3f, ForceMode2D.Impulse);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == SpadeState.Charging && other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsInvincible())
            {
                playerHealth.TakeDamage(1);
                Debug.Log("Spade가 플레이어에게 돌진 공격! (Trigger)");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (showDetectionRange)
        {
            Vector2 boxCenter = (Vector2)transform.position + detectionBoxOffset;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(boxCenter, detectionBoxSize);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(boxCenter, 0.1f);

            if (currentState == SpadeState.Preparing || currentState == SpadeState.Charging)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, chargeDirection * 2f);
            }
        }

        if (headCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
        }
    }

    void OnGUI()
    {
        if (Application.isPlaying && showDetectionRange)
        {
            GUI.Label(new Rect(10, 10, 200, 20), $"Spade State: {currentState}");
            GUI.Label(new Rect(10, 30, 200, 20), $"Timer: {stateTimer:F1}");

            string triggerStatus = triggerCollider != null ? (triggerCollider.enabled ? "활성" : "비활성") : "없음";
            GUI.Label(new Rect(10, 50, 200, 20), $"Trigger: {triggerStatus}");
        }
    }
}
