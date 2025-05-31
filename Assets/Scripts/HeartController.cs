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

    private Vector2 targetPlayerPosition; // 플레이어가 감지되었던 위치 저장

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

    // 상태 관리
    private enum HeartState
    {
        Patrolling,    // 좌우 이동
        Preparing,     // 창 던지기 준비 (차징 모션)
        Attacking,     // 창 던지기 후 대기
        Cooldown,      // 공격 후 쿨다운
        Crushed,       // 눌린 상태
        Dead           // 죽은 상태
    }

    private HeartState currentState = HeartState.Patrolling;
    private float stateTimer;
    private Vector2 startPosition;
    private int moveDirection = 1; // 1: 오른쪽, -1: 왼쪽
    private bool canAttack = true;
    private GameObject currentSpear;

    // 눌림 관련 변수
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

        // 시작 위치 저장
        startPosition = transform.position;

        // 원본 스케일과 콜라이더 정보 저장
        originalScale = transform.localScale;
        if (enemyCollider != null)
        {
            originalColliderSize = enemyCollider.size;
            originalColliderOffset = enemyCollider.offset;
        }

        // 창 스폰 포인트 설정
        if (spearSpawnPoint == null)
        {
            GameObject spawnObj = new GameObject("SpearSpawnPoint");
            spawnObj.transform.parent = transform;
            spawnObj.transform.localPosition = new Vector3(0.5f, 0.5f, 0);
            spearSpawnPoint = spawnObj.transform;
        }

        // 머리 체크 포인트 설정
        if (headCheck == null)
        {
            GameObject checkObj = new GameObject("HeadCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, 0.5f, 0);
            headCheck = checkObj.transform;
        }

        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }

        // 애니메이터 확인
        if (animator == null)
        {
            Debug.LogWarning("Animator 컴포넌트가 없습니다.");
        }

        // 기본 상태 설정
        currentState = HeartState.Patrolling;
        PlayAnimation(idleAnimName);
    }

    void Update()
    {
        // 죽은 상태가 아닐 때만 머리 밟기 체크
        if (currentState != HeartState.Dead)
        {
            CheckIfPlayerOnHead();
        }

        // 눌린 상태나 죽은 상태에서는 AI 동작 중단
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
        // 플레이어 감지 확인
        if (player != null && IsPlayerInRange() && canAttack)
        {
            StartPreparing();
            return;
        }

        // 좌우 이동
        HandlePatrolMovement();
        PlayAnimation(idleAnimName);
    }

    void HandlePatrolMovement()
    {
        // 이동
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);

        // 스프라이트 방향 설정
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection > 0;
        }

        // 순찰 범위 체크 (X축 기준으로만 체크)
        float currentX = transform.position.x;
        float startX = startPosition.x;

        // 오른쪽으로 이동 중일 때
        if (moveDirection > 0 && currentX >= startX + patrolDistance)
        {
            moveDirection = -1; // 왼쪽으로 방향 전환
        }
        // 왼쪽으로 이동 중일 때
        else if (moveDirection < 0 && currentX <= startX - patrolDistance)
        {
            moveDirection = 1; // 오른쪽으로 방향 전환
        }
    }

    void HandlePreparingState()
    {
        // 차징 모션 중에는 움직이지 않음
        rb.velocity = Vector2.zero;

        // 플레이어 방향으로 스프라이트 설정
        if (player != null && spriteRenderer != null)
        {
            bool faceRight = player.position.x > transform.position.x;
            spriteRenderer.flipX = faceRight;
        }
    }

    void HandleAttackingState()
    {
        // 창이 던져진 후 대기 상태 (애니메이션 일시정지)
        rb.velocity = Vector2.zero;

        // 창이 사라졌는지 확인
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

        // 플레이어가 감지된 위치 저장
        if (player != null)
        {
            targetPlayerPosition = player.position;
        }

        SetState(HeartState.Preparing);
        PlayAnimation(attackAnimName);
    }

    // Animation Event에서 호출할 메서드
    public void ThrowSpearFromAnimation()
    {
        Debug.Log("ThrowSpearFromAnimation 호출됨");
        ThrowSpear();
        SetState(HeartState.Attacking);

        // 애니메이터 일시정지
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

        // 창 생성
        GameObject spear = Instantiate(spearPrefab, spearSpawnPoint.position, Quaternion.identity);
        currentSpear = spear;

        Debug.Log($"창 생성 완료: {spear.name}");

        // 감지되었던 플레이어 위치를 향한 방향 계산
        Vector2 direction;
        bool throwingRight = true;

        if (player != null)
        {
            // 감지되었던 플레이어 위치를 향한 방향
            direction = (targetPlayerPosition - (Vector2)spearSpawnPoint.position).normalized;
            throwingRight = direction.x > 0;
            Debug.Log($"목표 위치: {targetPlayerPosition}, 방향: {direction}");
        }
        else
        {
            // 플레이어가 없으면 현재 바라보는 방향으로 던지기
            throwingRight = spriteRenderer.flipX;
            direction = throwingRight ? Vector2.right : Vector2.left;
            Debug.Log($"기본 방향: {direction}");
        }

        // 창에 SpearProjectile 컴포넌트가 있는지 확인하고 설정
        Spear spearScript = spear.GetComponent<Spear>();
        if (spearScript != null)
        {
            // 일직선으로 날아가도록 설정
            spearScript.LaunchStraight(direction, spearSpeed, throwingRight);
        }
        else
        {
            // SpearProjectile이 없으면 기본 방식으로 발사
            Rigidbody2D spearRb = spear.GetComponent<Rigidbody2D>();
            if (spearRb != null)
            {
                // 중력 영향 제거하고 일직선으로 발사
                spearRb.gravityScale = 0f;
                spearRb.velocity = direction * spearSpeed;
                Debug.Log($"창 속도 설정: {spearRb.velocity}");

                // 창 방향 설정
                SpriteRenderer spearSpriteRenderer = spear.GetComponent<SpriteRenderer>();
                if (spearSpriteRenderer != null)
                {
                    spearSpriteRenderer.flipX = !throwingRight;
                }

                // 창 회전 설정 (날아가는 방향으로)
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                spear.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            else
            {
                Debug.LogWarning("창에 Rigidbody2D가 없습니다!");
            }
        }

        // 일정 시간 후 창 파괴
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
                // 플레이어에게 점프 효과 부여
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
        SetState(HeartState.Crushed);

        // 애니메이터 완전히 정지
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

        // 상태 변경 시 애니메이터 속도 복원 (Attacking 상태 제외)
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

        // 눌린 상태나 죽은 상태에서는 애니메이션 재생 중단
        if (currentState == HeartState.Crushed || currentState == HeartState.Dead) return;

        // 현재 재생 중인 애니메이션과 다른 경우에만 재생
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            animator.Play(animationName);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (showDetectionRange)
        {
            // 감지 범위
            Vector2 detectionCenter = (Vector2)transform.position + Vector2.up * detectionHeightOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectionCenter, detectionRange);

            // 감지 중심점
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(detectionCenter, 0.1f);
        }

        // 머리 체크 포인트
        if (headCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
        }

        // 창 스폰 포인트
        if (spearSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spearSpawnPoint.position, 0.2f);
        }
    }
}
