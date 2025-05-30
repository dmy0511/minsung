using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    [Header("적 상태")]
    public bool isCrushed = false;
    public float crushedScale = 0.5f;
    public float crushAnimationTime = 0.2f;

    [Header("충돌 감지")]
    public Transform headCheck;
    public float headCheckRadius = 0.3f;
    public LayerMask playerLayer;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D enemyCollider;

    private Vector3 originalScale;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private bool isBeingCrushed = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
    }

    private void Update()
    {
        CheckIfPlayerOnHead();
    }

    private void CheckIfPlayerOnHead()
    {
        if (isBeingCrushed)
            return;

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

    private IEnumerator CrushAnimation()
    {
        isBeingCrushed = true;

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

    private IEnumerator DestroyEnemy()
    {
        isBeingCrushed = true;

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

    private void OnDrawGizmosSelected()
    {
        if (headCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
        }
    }
}
