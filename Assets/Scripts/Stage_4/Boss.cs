using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    [Header("보스 설정")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("애니메이션 설정")]
    public string idleAnimName = "Idle";
    public string attack1AnimName = "Attack_1";
    public string attack2AnimName = "Attack_2";
    public string grogyAnimName = "Grogy";

    [Header("스킬 1 설정")]
    public GameObject staffObject;
    public GameObject nodePrefab;
    public float nodeSpawnY = 1.51f;
    public float nodeSpawnXMin = -7.2f;
    public float nodeSpawnXMax = 3.35f;
    public int nodeCount = 4;

    [Header("스킬 2 설정")]
    public Transform spadeSpawnpoint;
    public Transform heartSpawnpoint;
    public GameObject spadePrefab;
    public GameObject heartPrefab;
    public GameObject warningImagePrefab;
    public float warningDuration = 2f;

    [Header("그로기 설정")]
    public float grogginessDuration = 5f;

    [Header("머리 밟기 감지")]
    public Transform headCheck;
    public float headCheckRadius = 0.5f;
    public LayerMask playerLayer = 1;
    public Vector3 bouncePosition = new Vector3(-7.53f, -3.857931f, 0f);

    [Header("화면 흔들림")]
    public float shakeIntensity = 0.3f;
    public float shakeDuration = 0.5f;

    [Header("페이드 효과 설정")]
    public float deathDelay = 1f;
    public float fadeInDuration = 1f;
    public Color fadeColor = Color.black;

    private enum BossState
    {
        Idle,
        Skill1,
        Skill2,
        Groggy,
        WaitingForEnemies,
        Dying
    }

    private BossState currentState = BossState.Idle;
    private float stateTimer = 0f;
    private bool isGroggy = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private Vector3 originalCameraPosition;

    private List<GameObject> activeNodes = new List<GameObject>();
    private List<GameObject> activeWarnings = new List<GameObject>();
    private bool hasSpawnedEnemies = false;
    private bool hasUsedSkill1 = false;
    private bool hasUsedSkill2 = false;
    private bool animationStopped = false;
    private int lastEnemyCount = -1;

    private GameObject fadeCanvas;
    private Image fadeImage;

    void Start()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;

        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.position;
        }

        if (headCheck == null)
        {
            GameObject checkObj = new GameObject("HeadCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, 1f, 0);
            headCheck = checkObj.transform;
        }

        if (staffObject != null)
        {
            staffObject.SetActive(false);
        }

        CreateFadeUI();

        SetState(BossState.Idle);
    }

    void CreateFadeUI()
    {
        fadeCanvas = new GameObject("FadeCanvas");
        Canvas canvas = fadeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = fadeCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        fadeCanvas.AddComponent<GraphicRaycaster>();

        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(fadeCanvas.transform);

        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        fadeCanvas.SetActive(false);
    }

    void Update()
    {
        stateTimer += Time.deltaTime;

        if (isGroggy)
        {
            CheckHeadStomp();
        }

        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleState();
                break;

            case BossState.Skill1:
                HandleSkill1State();
                break;

            case BossState.Skill2:
                HandleSkill2State();
                break;

            case BossState.WaitingForEnemies:
                HandleWaitingForEnemiesState();
                break;

            case BossState.Groggy:
                HandleGroggyState();
                break;

            case BossState.Dying:
                break;
        }
    }

    void HandleIdleState()
    {
        if (stateTimer >= 1f)
        {
            StartSkill1();
        }
    }

    void HandleSkill1State()
    {
        activeNodes.RemoveAll(node => node == null);

        if (activeNodes.Count == 0 && hasUsedSkill1)
        {
            StartGroggy();
        }
        else if (hasUsedSkill1 && stateTimer >= 2f && !animationStopped)
        {
            if (animator != null)
            {
                animator.speed = 0f;
                animationStopped = true;
            }
        }
    }

    void HandleSkill2State()
    {
        if (hasUsedSkill2 && !animationStopped)
        {
            if (animator != null)
            {
                animator.speed = 0f;
                animationStopped = true;
            }
            SetState(BossState.WaitingForEnemies);
        }
    }

    void HandleWaitingForEnemiesState()
    {
        if (!hasSpawnedEnemies)
        {
            return;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            StartGroggy();
        }
    }

    void HandleGroggyState()
    {
        if (stateTimer >= grogginessDuration)
        {
            EndGroggy();
        }
    }

    void StartSkill1()
    {
        isGroggy = false;

        SetState(BossState.Skill1);
        PlayAnimation(attack1AnimName);
        hasUsedSkill1 = false;
        animationStopped = false;

        if (staffObject != null)
        {
            staffObject.SetActive(true);
        }
    }

    public void ExecuteSkill1()
    {
        hasUsedSkill1 = true;
        StartCoroutine(SpawnNodes());
    }

    public void ExecuteSkill2()
    {
        Debug.Log("=== 스킬 2 실행! 적 생성 시작 ===");
        hasUsedSkill2 = true;
        StartCoroutine(SpawnEnemiesWithWarning());
    }

    void StartSkill2()
    {
        Debug.Log("보스가 스킬 2를 시전합니다!");

        isGroggy = false;

        SetState(BossState.Skill2);
        PlayAnimation(attack2AnimName);

        hasUsedSkill2 = false;
        animationStopped = false;
        hasSpawnedEnemies = false;

        if (animator != null)
        {
            animator.speed = 1f;
        }

        if (staffObject != null)
        {
            staffObject.SetActive(false);
        }
    }

    void StartGroggy()
    {
        Debug.Log("=== 보스가 그로기 상태에 진입합니다! ===");

        isGroggy = true;
        SetState(BossState.Groggy);
        PlayAnimation(grogyAnimName);

        if (animator != null)
        {
            animator.speed = 1f;
        }

        if (staffObject != null)
        {
            staffObject.SetActive(false);
        }
    }

    void EndGroggy()
    {
        if (hasUsedSkill1 && !hasUsedSkill2)
        {
            StartSkill2();
        }
        else if (hasUsedSkill2)
        {
            hasUsedSkill1 = false;
            hasUsedSkill2 = false;
            StartSkill1();
        }
        else
        {
            StartSkill1();
        }
    }

    IEnumerator SpawnNodes()
    {
        List<float> spawnPositions = new List<float>();

        while (spawnPositions.Count < nodeCount)
        {
            float randomX = Random.Range(nodeSpawnXMin, nodeSpawnXMax);

            bool tooClose = false;
            foreach (float pos in spawnPositions)
            {
                if (Mathf.Abs(randomX - pos) < 1f)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                spawnPositions.Add(randomX);
            }
        }

        foreach (float xPos in spawnPositions)
        {
            if (nodePrefab != null)
            {
                Vector3 spawnPos = new Vector3(xPos, nodeSpawnY, 0);
                GameObject node = Instantiate(nodePrefab, spawnPos, Quaternion.identity);
                activeNodes.Add(node);

                StartCoroutine(CameraShake());
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    IEnumerator SpawnEnemiesWithWarning()
    {
        if (hasSpawnedEnemies) yield break;

        GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (existingEnemies.Length > 0)
        {
            yield break;
        }

        List<Transform> spawnPoints = new List<Transform>();
        List<GameObject> prefabs = new List<GameObject>();

        if (spadeSpawnpoint != null && spadePrefab != null)
        {
            spawnPoints.Add(spadeSpawnpoint);
            prefabs.Add(spadePrefab);
        }

        if (heartSpawnpoint != null && heartPrefab != null)
        {
            spawnPoints.Add(heartSpawnpoint);
            prefabs.Add(heartPrefab);
        }

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (warningImagePrefab != null)
            {
                GameObject warning = Instantiate(warningImagePrefab, spawnPoints[i].position, Quaternion.identity);
                activeWarnings.Add(warning);
            }
        }

        yield return new WaitForSeconds(warningDuration);

        foreach (GameObject warning in activeWarnings)
        {
            if (warning != null)
            {
                Destroy(warning);
            }
        }
        activeWarnings.Clear();

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (spawnPoints[i] != null && prefabs[i] != null)
            {
                Instantiate(prefabs[i], spawnPoints[i].position, Quaternion.identity);
            }
        }

        hasSpawnedEnemies = true;
    }

    IEnumerator CameraShake()
    {
        if (mainCamera == null) yield break;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
            randomOffset.z = 0;

            mainCamera.transform.position = originalCameraPosition + randomOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalCameraPosition;
    }

    void CheckHeadStomp()
    {
        if (!isGroggy)
        {
            return;
        }

        Collider2D playerCollider = Physics2D.OverlapCircle(headCheck.position, headCheckRadius, playerLayer);

        if (playerCollider != null)
        {
            Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();
            if (playerRb != null && playerRb.velocity.y < 0)
            {
                playerRb.transform.position = bouncePosition;
                playerRb.velocity = Vector2.zero;

                PlayerController playerController = playerCollider.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.ForcePlayJumpAnimation();
                }

                TakeDamage(1);
            }
        }
    }

    void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (currentHealth <= 0)
        {
            SetState(BossState.Dying);
            StartCoroutine(BossDeathSequence());
        }
    }

    IEnumerator BossDeathSequence()
    {
        Debug.Log("=== 보스가 죽습니다! ===");

        if (animator != null)
        {
            animator.speed = 0f;
        }

        yield return new WaitForSeconds(deathDelay);

        BossCleanup();

        yield return StartCoroutine(FadeIn());

        SceneManager.LoadScene("EndingStory");
    }

    IEnumerator FadeIn()
    {
        if (fadeCanvas != null && fadeImage != null)
        {
            fadeCanvas.SetActive(true);

            float elapsedTime = 0f;
            Color startColor = fadeImage.color;
            Color targetColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(startColor.a, targetColor.a, elapsedTime / fadeInDuration);

                Color currentColor = fadeImage.color;
                currentColor.a = alpha;
                fadeImage.color = currentColor;

                yield return null;
            }

            fadeImage.color = targetColor;
        }
    }

    void BossCleanup()
    {
        foreach (GameObject node in activeNodes)
        {
            if (node != null) Destroy(node);
        }
        activeNodes.Clear();

        foreach (GameObject warning in activeWarnings)
        {
            if (warning != null) Destroy(warning);
        }
        activeWarnings.Clear();

        if (staffObject != null)
        {
            staffObject.SetActive(false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == BossState.Dying)
            return;

        if (!isGroggy && collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsInvincible())
            {
                playerHealth.TakeDamage(1);
            }
        }
    }

    void SetState(BossState newState)
    {
        currentState = newState;
        stateTimer = 0f;
    }

    void PlayAnimation(string animationName)
    {
        if (animator != null)
        {
            animator.Play(animationName);
        }
    }

    void OnDestroy()
    {
        if (fadeCanvas != null)
        {
            Destroy(fadeCanvas);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (headCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
        }

        Gizmos.color = Color.yellow;
        Vector3 leftPoint = new Vector3(nodeSpawnXMin, nodeSpawnY, 0);
        Vector3 rightPoint = new Vector3(nodeSpawnXMax, nodeSpawnY, 0);
        Gizmos.DrawLine(leftPoint, rightPoint);

        if (spadeSpawnpoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(spadeSpawnpoint.position, 0.5f);
        }

        if (heartSpawnpoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(heartSpawnpoint.position, 0.5f);
        }
    }
}
