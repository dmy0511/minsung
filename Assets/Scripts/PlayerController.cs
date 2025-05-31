using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("�̵� ����")]
    public float moveSpeed = 5f;
    public float jumpForce = 11f;

    [Header("���� ����")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("���� ����")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("�ִϸ��̼� ����")]
    public string idleAnimName = "Idle";
    public string walkAnimName = "Walk";
    public string jumpAnimName = "Jump";

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isGrounded;
    private bool isJumping;
    private float moveInput;
    private bool wasGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("GroundCheck");
            checkObj.transform.parent = transform;
            checkObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = checkObj.transform;
        }

        // �ִϸ����Ͱ� ���� ��� ���
        if (animator == null)
        {
            Debug.LogWarning("Animator ������Ʈ�� �����ϴ�. �ִϸ��̼��� ����Ϸ��� Animator�� �߰����ּ���.");
        }

        wasGrounded = isGrounded;
    }

    void Update()
    {
        HandleInput();

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            isJumping = true;
        }
    }

    void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
        HandleJump();
        UpdateAnimation();
    }

    void HandleInput()
    {
        moveInput = 0;

        if (Input.GetKey(leftKey))
        {
            moveInput -= 1;
        }
        if (Input.GetKey(rightKey))
        {
            moveInput += 1;
        }
    }

    void CheckGrounded()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void HandleMovement()
    {
        if (moveInput != 0)
        {
            spriteRenderer.flipX = moveInput < 0;
        }

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void HandleJump()
    {
        if (isJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = false;
        }
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // ���� ���� Ȯ�� (���߿� ���� ��)
        if (!isGrounded)
        {
            PlayAnimation(jumpAnimName);
        }
        // ���鿡 ���� ��
        else
        {
            // �����̰� �ִ��� Ȯ��
            if (Mathf.Abs(moveInput) > 0.1f)
            {
                PlayAnimation(walkAnimName);
            }
            else
            {
                PlayAnimation(idleAnimName);
            }
        }
    }

    void PlayAnimation(string animationName)
    {
        if (animator == null) return;

        // ���� ��� ���� �ִϸ��̼ǰ� �ٸ� ��쿡�� ���
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            animator.Play(animationName);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
