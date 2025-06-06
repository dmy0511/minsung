using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("�̵� ����")]
    public float moveSpeed = 5f;
    public float jumpForce = 11f;

    [Header("���� ����")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, -0.5f, 0);
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

        CreateGroundCheckIfNeeded();

        if (animator == null)
        {
            Debug.LogWarning("Animator ������Ʈ�� �����ϴ�. �ִϸ��̼��� ����Ϸ��� Animator�� �߰����ּ���.");
        }

        wasGrounded = isGrounded;
    }

    void Update()
    {
        UpdateGroundCheckPosition();

        HandleInput();
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            isJumping = true;
            PlayAnimation(jumpAnimName);
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

        if (!isGrounded)
        {
            PlayAnimation(jumpAnimName);
        }
        else
        {
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

        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            animator.Play(animationName);
        }
    }

    public void ForcePlayJumpAnimation()
    {
        if (animator != null)
        {
            animator.Play(jumpAnimName, 0, 0f);
            Debug.Log("���� �Ӹ��� ��Ƽ� ���� �ִϸ��̼� ���");
        }
    }

    private void UpdateGroundCheckPosition()
    {
        if (groundCheck != null)
        {
            groundCheck.localPosition = groundCheckOffset;
        }
    }

    void OnValidate()
    {
        if (groundCheck != null)
        {
            UpdateGroundCheckPosition();
        }
        else if (!Application.isPlaying)
        {
            CreateGroundCheckIfNeeded();
        }
    }

    private void CreateGroundCheckIfNeeded()
    {
        if (groundCheck == null)
        {
            GameObject checkObj = new GameObject("Check");
            checkObj.transform.parent = transform;
            groundCheck = checkObj.transform;
            UpdateGroundCheckPosition();
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 gizmoPosition = transform.position + groundCheckOffset;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gizmoPosition, groundCheckRadius);

        if (groundCheck != null && groundCheck.localPosition != groundCheckOffset)
        {
            UpdateGroundCheckPosition();
        }
    }
}
