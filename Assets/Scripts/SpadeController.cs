using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpadeController : MonoBehaviour
{
    [Header("플레이어 감지 설정")]
    public float detectionRange = 5f;
    public float detectionHeightOffset = 1f;
    public LayerMask playerLayer = 1;

    [Header("머리 밟기 감지")]
    public Transform headCheck;
    public float headCheckRadius = 0.3f;

    [Header("눌림 효과 설정")]
    public float crushedScale = 0.5f;
    public float crushAnimationTime = 0.2f;

    [Header("돌진 설정")]
    public float chargeSpeed = 8f;
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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<BoxCollider2D>();

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

    void HandleIdleState()
    {
        if (player != null && IsPlayerInRange())
        {
            StartPreparing();
        }

        rb.velocity = Vector2.zero;
    }

    void HandlePreparingState()
    {
        rb.velocity = Vector2.zero;

        if (player != null)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            chargeDirection = directionToPlayer;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = chargeDirection.x > 0;
            }
        }
    }

    void HandleChargingState()
    {
        rb.velocity = chargeDirection * chargeSpeed;

        if (stateTimer >= chargeDuration)
        {
            StartCooldown();
        }
    }

    void HandleCooldownState()
    {
        rb.velocity = Vector2.zero;

        if (stateTimer >= cooldownTime)
        {
            SetState(SpadeState.Idle);
            PlayAnimation(idleAnimName);
        }
    }

    bool IsPlayerInRange()
    {
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        return distance <= detectionRange;
    }

    void StartPreparing()
    {
        if (player != null)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            chargeDirection = directionToPlayer;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = chargeDirection.x > 0;
            }
        }

        SetState(SpadeState.Preparing);
        PlayAnimation(attackAnimName);
    }

    public void StartChargeFromAnimation()
    {
        SetState(SpadeState.Charging);
    }

    void StartCooldown()
    {
        SetState(SpadeState.Cooldown);
        
        PlayAnimation(idleAnimName);
    }

    void SetState(SpadeState newState)
    {
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
    }

    void OnDrawGizmosSelected()
    {
        if (showDetectionRange)
        {
            Vector2 detectionCenter = (Vector2)transform.position + Vector2.up * detectionHeightOffset;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectionCenter, detectionRange);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(detectionCenter, 0.1f);

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
        }
    }
}
