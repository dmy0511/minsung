using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigMushroom : MonoBehaviour
{
    [Header("����")]
    public float bounceForce = 20f;
    public float bounceCooldown = 0.5f;

    [Header("�ð� ȿ�� ����")]
    public float squishAmount = 0.2f;
    public float squishDuration = 0.1f;
    public float returnDuration = 0.2f;

    private bool canBounce = true;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;

        if (tag != "BigMushroom")
        {
            Debug.LogWarning("���� ������Ʈ�� 'BigMushroom' �±װ� �����Ǿ� ���� �ʽ��ϴ�!");
            gameObject.tag = "BigMushroom";
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && canBounce)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        playerRb.velocity = new Vector2(playerRb.velocity.x, 0); // ���� ���� �ӵ� ����
                        playerRb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);

                        StartCoroutine(SquishAndReturn());

                        StartCoroutine(BounceCooldown());

                        break;
                    }
                }
            }
        }
    }

    private IEnumerator SquishAndReturn()
    {
        float elapsed = 0;
        while (elapsed < squishDuration)
        {
            float t = elapsed / squishDuration;
            float scaleY = Mathf.Lerp(originalScale.y, originalScale.y * (1 - squishAmount), t);
            float scaleX = Mathf.Lerp(originalScale.x, originalScale.x * (1 + squishAmount * 0.5f), t);

            transform.localScale = new Vector3(scaleX, scaleY, originalScale.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0;
        while (elapsed < returnDuration)
        {
            float t = elapsed / returnDuration;
            float scaleY = Mathf.Lerp(originalScale.y * (1 - squishAmount), originalScale.y, t);
            float scaleX = Mathf.Lerp(originalScale.x * (1 + squishAmount * 0.5f), originalScale.x, t);

            transform.localScale = new Vector3(scaleX, scaleY, originalScale.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private IEnumerator BounceCooldown()
    {
        canBounce = false;
        yield return new WaitForSeconds(bounceCooldown);
        canBounce = true;
    }
}
