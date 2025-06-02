using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 2f;
    public float patrolDistance = 3f;

    [Header("플레이어 감지 설정")]
    public float detectionRange = 5f;
    public float detectionHeightOffset = 1f;
    public LayerMask playerLayer = 1;

    [Header("창 던지기 설정")]
    public GameObject spearPrefab;
    public Transform spearSpawnPoint;
    public float spearSpeed = 8f;
    public float spearLifetime = 3f;
    public float attackCooldown = 2f;

    private Vector2 targetPlayerPosition;

    [Header("머리 밟기 감지")]
    public Transform headCheck;
    public float headCheckRadius = 0.3f;

    [Header("눌림 효과 설정")]
    public float crushedScale = 0.5f;
    public float crushAnimationTime = 0.2f;

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

    private enum HeartState
    {
        Patrolling,
        Preparing,
        Attacking,
        Cooldown,
        Crushed,
        Dead
    }

    private HeartState currentState = HeartState.Patrolling;
    private float stateTimer;
    private Vector2 startPosition;
    private int moveDirection = 1;
    private bool canAttack = true;
    private GameObject currentSpear;

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
        enemyCollider = GetComponent<BoxCollider2D>();

        startPosition = transform.position;

        originalScale = transform.localScale;
        if (enemyCollider != null)
        {
            originalColliderSize = enemyCollider.size;
            originalColliderOffset = enemyCollider.offset;
        }

        if (spearSpawnPoint == null)
        {
            GameObject spawnObj = new GameObject("SpearSpawnPoint");
            spawnObj.transform.parent = transform;
            spawnObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0);
            spearSpawnPoint = spawnObj.transform;
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
            Debug.LogWarning("Animator 컴포넌트가 없습니다.");
        }

        currentState = HeartState.Patrolling;
        PlayAnimation(idleAnimName);
    }

    void Update()
    {
        if (currentState != HeartState.Dead)
        {
            CheckIfPlayerOnHead();
        }

        if (currentState == HeartState.Crushed || currentState == HeartState.Dead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        switch (currentState)
        {
            case HeartState.Patrolling:
                HandlePatrollingState();
                break;

            case HeartState.Preparing:
                HandlePreparingState();
                break;

            case HeartState.Attacking:
                HandleAttackingState();
                break;

            case HeartState.Cooldown:
                HandleCooldownState();
                break;
        }

        UpdateStateTimer();
    }

    void HandlePatrollingState()
    {
        if (player != null && IsPlayerInRange() && canAttack)
        {
            StartPreparing();
            return;
        }

        HandlePatrolMovement();
        PlayAnimation(idleAnimName);
    }

    void HandlePatrolMovement()
    {
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection > 0;
            UpdateHeadCheckPosition(moveDirection > 0);
        }

        float currentX = transform.position.x;
        float startX = startPosition.x;

        if (moveDirection > 0 && currentX >= startX + patrolDistance)
        {
            moveDirection = -1;
        }
        else if (moveDirection < 0 && currentX <= startX - patrolDistance)
        {
            moveDirection = 1;
        }
    }

    void HandlePreparingState()
    {
        rb.velocity = Vector2.zero;

        if (player != null && spriteRenderer != null)
        {
            bool faceRight = player.position.x > transform.position.x;
            spriteRenderer.flipX = faceRight;
            UpdateHeadCheckPosition(faceRight);
        }
    }

    void HandleAttackingState()
    {
        rb.velocity = Vector2.zero;

        if (currentSpear == null)
        {
            SetState(HeartState.Cooldown);
            PlayAnimation(idleAnimName);
        }
    }

    void HandleCooldownState()
    {
        rb.velocity = Vector2.zero;

        if (stateTimer >= attackCooldown)
        {
            canAttack = true;
            SetState(HeartState.Patrolling);
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

        Vector2 detectionCenter = (Vector2)transform.position + Vector2.up * detectionHeightOffset;
        float distance = Vector2.Distance(detectionCenter, player.position);
        return distance <= detectionRange;
    }

    void StartPreparing()
    {
        canAttack = false;

        if (player != null)
        {
            targetPlayerPosition = player.position;
        }

        SetState(HeartState.Preparing);
        PlayAnimation(attackAnimName);
    }

    public void ThrowSpearFromAnimation()
    {
        Debug.Log("ThrowSpearFromAnimation 호출됨");
        ThrowSpear();
        SetState(HeartState.Attacking);

        if (animator != null)
        {
            animator.speed = 0f;
            Debug.Log("애니메이터 일시정지됨");
        }
    }

    void ThrowSpear()
    {
        Debug.Log("ThrowSpear 메서드 호출됨");

        if (spearPrefab == null)
        {
            Debug.LogError("Spear Prefab이 할당되지 않았습니다!");
            return;
        }

        if (spearSpawnPoint == null)
        {
            Debug.LogError("Spear Spawn Point가 설정되지 않았습니다!");
            return;
        }

        Debug.Log($"창 생성 위치: {spearSpawnPoint.position}");

        GameObject spear = Instantiate(spearPrefab, spearSpawnPoint.position, Quaternion.identity);
        currentSpear = spear;

        Debug.Log($"창 생성 완료: {spear.name}");

        Vector2 direction;
        bool throwingRight = true;

        if (player != null)
        {
            direction = (targetPlayerPosition - (Vector2)spearSpawnPoint.position).normalized;
            throwingRight = direction.x > 0;
            Debug.Log($"목표 위치: {targetPlayerPosition}, 방향: {direction}");
        }
        else
        {
            throwingRight = spriteRenderer.flipX;
            direction = throwingRight ? Vector2.right : Vector2.left;
            Debug.Log($"기본 방향: {direction}");
        }

        Spear spearScript = spear.GetComponent<Spear>();
        if (spearScript != null)
        {
            spearScript.LaunchStraight(direction, spearSpeed, throwingRight);
        }
        else
        {
            Rigidbody2D spearRb = spear.GetComponent<Rigidbody2D>();
            if (spearRb != null)
            {
                spearRb.gravityScale = 0f;
                spearRb.velocity = direction * spearSpeed;
                Debug.Log($"창 속도 설정: {spearRb.velocity}");

                SpriteRenderer spearSpriteRenderer = spear.GetComponent<SpriteRenderer>();
                if (spearSpriteRenderer != null)
                {
                    spearSpriteRenderer.flipX = !throwingRight;
                }

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                spear.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            else
            {
                Debug.LogWarning("창에 Rigidbody2D가 없습니다!");
            }
        }

        Destroy(spear, spearLifetime);
        Debug.Log($"창은 {spearLifetime}초 후 파괴됩니다");
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
        SetState(HeartState.Crushed);

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
        SetState(HeartState.Dead);

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

    void SetState(HeartState newState)
    {
        currentState = newState;
        stateTimer = 0f;

        if (animator != null && newState != HeartState.Attacking && newState != HeartState.Crushed && newState != HeartState.Dead)
        {
            animator.speed = 1f;
        }
    }

    void UpdateStateTimer()
    {
        stateTimer += Time.deltaTime;
    }

    void PlayAnimation(string animationName)
    {
        if (animator == null) return;

        if (currentState == HeartState.Crushed || currentState == HeartState.Dead) return;

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            animator.Play(animationName);
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
        }

        if (headCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
        }

        if (spearSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spearSpawnPoint.position, 0.2f);
        }
    }
}
