using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float gravity = 9.8f;

    [SerializeField] private Vector2 groundCheckVecSize = new Vector2(0.5f, 1.05f);
    [SerializeField] private Vector2 groundCheckVec;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.2f;

    [SerializeField] private int maxJumpCount = 3;
    private int currentJumpCount;

    private Rigidbody2D _rbCompo;
    private Vector2 _moveVec;

    private bool _isGrounded;
    #region NetWorkData
    //점프를 했는가? (Is Jumping Now? <bool>)
    private bool _isJumpNow = false;
    //대쉬를 하고 있는가?(Is Dashing Now? <bool>)
    private bool _isDashing;
    //대쉬할 방향(Dash Direction<Vec2>)
    private Vector2 _dashDirection;
    //대쉬를 사용했는가? (Use Dash? <bool>)
    private bool CanDash = false;
    //이동하고 있는 방향 (Now Move.X Direction <Sbyte>)
    private sbyte nowMoveDirection;
    #endregion

    private float _dashTimer;

    private void Start()
    {
        _rbCompo = GetComponent<Rigidbody2D>();
        _rbCompo.gravityScale = 1f;
        currentJumpCount = maxJumpCount;
        groundMask = LayerMask.GetMask("Ground");
        groundCheckVecSize = new Vector2(0.5f, 1.05f);
    }

    private void FixedUpdate()
    {
        OnGround();
        GroundDash();
        AirDash();
        if (!_isDashing)
        {
            Vector2 velocity = _rbCompo.linearVelocity;
            velocity.x = _moveVec.x * speed;
            _rbCompo.linearVelocityX = velocity.x;
        }
    }

    private void Update()
    {
        if (Keyboard.current.sKey.wasPressedThisFrame && _isGrounded)
        {
            _rbCompo.AddForce(Vector2.down * gravity * 1.5f, ForceMode2D.Impulse);
        }
    }

    private void OnGround()
    {
        Collider2D hit = Physics2D.OverlapBox((Vector2)transform.position + groundCheckVec, groundCheckVecSize, 0, groundMask);
        _isGrounded = hit == null;

        if (!_isGrounded)
        {
            currentJumpCount = maxJumpCount;
            _isJumpNow = false;
            CanDash = false;
        }
    }

    public void OnMove(InputValue value)
    {
        _moveVec = value.Get<Vector2>();
        if (_moveVec.x >= 0.1f)
        {
            nowMoveDirection = 1;
        }
        else if (_moveVec.x <= -0.1f)
        {
            nowMoveDirection = -1;
        }
        else
        {
            nowMoveDirection = 0;
        }
    }

    public void OnJump()
    {
        if (currentJumpCount > 0)
        {
            _isJumpNow = true;
            _rbCompo.linearVelocityY = jumpForce;
            currentJumpCount--;
        }
    }

    private void GroundDash()
    {
        if (_isDashing && _isGrounded)
        {
            _rbCompo.AddForce(new Vector2(_dashDirection.x,0) * dashForce,ForceMode2D.Impulse);
            _dashTimer -= Time.fixedDeltaTime;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
            }
            return;
        }
    }

    private void AirDash()
    {
        if (_isDashing && !_isGrounded)
        {
            _rbCompo.linearVelocity = _dashDirection * dashForce / 2f;
            _dashTimer -= Time.fixedDeltaTime;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
            }
            GetComponent<SpriteRenderer>().color = new Color(1,1,1,0.5f);
            return;
        }
    }

    public void OnDash(InputValue value)
    {
        if (currentJumpCount <= 0) return;

        if (!CanDash)
        {
            if (_isGrounded)
            {
                currentJumpCount--;
                _rbCompo.linearVelocityX = 0;
                CanDash = true;
            }
            else
            {
                CanDash = false;
                _rbCompo.linearVelocity = Vector2.zero;
            }
            if (_isDashing) return;

            Vector2 inputDir = _moveVec.normalized;

            if (_isGrounded)
            {
                if (inputDir == Vector2.zero)
                    inputDir = Vector2.down;
            }
            else
            {
                inputDir = new Vector2(Mathf.Sign(_moveVec.x), 0);
            }

            _dashDirection = inputDir.normalized;
            _isDashing = true;
            _dashTimer = dashDuration;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + (Vector3)groundCheckVec, groundCheckVecSize);
    }
#endif
}