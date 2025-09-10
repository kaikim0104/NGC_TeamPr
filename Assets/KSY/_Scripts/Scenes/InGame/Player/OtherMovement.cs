using BackEnd;
using UnityEngine;
using UnityEngine.InputSystem;
using static BackendFunctionInGame;

public class OtherMovement : Player
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float gravity = 9.8f;

    [SerializeField] private Vector2 groundCheckVecSize = new Vector2(0.5f, 1.05f);
    [SerializeField] private Vector2 groundCheckVec;
    [SerializeField] private LayerMask groundMask;

    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.2f;

    private Rigidbody2D _rbCompo;
    private Vector2 _moveVec;

    private bool _isGrounded;
    #region NetWorkData
    //������ �ߴ°�? (Is Jumping Now? <bool>)
    private bool _usingJump = false;
    //�뽬�� �ϰ� �ִ°�?(Is Dashing Now? <bool>)
    private bool _isDashing;
    //�뽬�� ����(Dash Direction<Vec2>)
    private Vector2 _dashDir;
    //�뽬�� ����ߴ°�? (Use Dash? <bool>)
    private bool _usingDash = false;
    //�̵��ϰ� �ִ� ���� (Now Move.X Direction <Sbyte>)
    private bool _downDashing = false;
    #endregion

    private bool _startDashTimer = false;
    private float _dashTimer;

    private void Start()
    {
        _rbCompo = GetComponent<Rigidbody2D>();
        _rbCompo.gravityScale = 1f;
        groundMask = LayerMask.GetMask("Ground");
        groundCheckVecSize = new Vector2(0.5f, 1.05f);
    }
    private void FixedUpdate()
    {
        if (_startDashTimer)
        {
            _dashTimer += Time.fixedDeltaTime;
            GroundDash();
            AirDash();
        }
        else
        {
            _dashTimer = 0f;
        }
    }

    private void Update()
    {
        OnGround();
        if (!_isDashing)
        {
            Vector2 velocity = _rbCompo.linearVelocity;
            velocity.x = _moveVec.x * speed;
            _rbCompo.linearVelocityX = velocity.x;
        }
    }
    private void DownDash()
    {
        _rbCompo.AddForce(Vector2.down * gravity * 1.5f, ForceMode2D.Impulse);
    }

    private void OnGround()
    {
        Collider2D hit = Physics2D.OverlapBox((Vector2)transform.position + groundCheckVec, groundCheckVecSize, 0, groundMask);
        _isGrounded = hit != null;

        if (_isGrounded)
        {
            _downDashing = false;
            _usingJump = false;
            _usingDash = false;
        }
    }
    public void OnJump()
    {
        _usingJump = true;
        _rbCompo.linearVelocityY = jumpForce;
    }

    private void GroundDash()
    {
        if (_isDashing && _isGrounded)
        {
            _startDashTimer = true;
            _rbCompo.AddForce(new Vector2(_dashDir.x, 0) * dashForce, ForceMode2D.Impulse);
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
                _startDashTimer = false;
            }
            return;
        }
    }

    private void AirDash()
    {
        if (_isDashing && !_isGrounded)
        {
            _startDashTimer = true;
            _rbCompo.linearVelocity = _dashDir * dashForce / 2f;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
                _startDashTimer = false;
            }
            //air dash effect
            return;
        }
    }

    public void OnDash()
    {
        if (!_usingDash)
        {

            Vector2 inputDir = _moveVec.normalized;
            if (_isGrounded)
            {
                _rbCompo.linearVelocityX = 0;
                //CanDash = true;
                if (inputDir == Vector2.zero)
                    inputDir = Vector2.down;
            }
            else
            {
                //CanDash = false;
                _rbCompo.linearVelocity = Vector2.zero;
            }
            if (_isDashing) return;
            else
            {
                inputDir = new Vector2(Mathf.Sign(_moveVec.x), 0);
            }

            _dashDir = inputDir.normalized;
            _isDashing = true;
            _dashTimer = dashDuration;
            GroundDash();
            AirDash();
        }
    }
    public override void ApplyByteData(byte state)
    {
        bool usingDash = (state & (byte)flagPlayerMovementState.UsingDash) != 0;
        _usingDash = usingDash;

        if (!_isDashing && usingDash)
        {
            Debug.Log($"isDash : {usingDash}");
            OnDash();
            GroundDash();
            AirDash();
        }

        bool isDashing = (state & (byte)flagPlayerMovementState.IsDashing) != 0;
        _isDashing = isDashing;

        bool UsingJump = (state & (byte)flagPlayerMovementState.UsingJump) != 0;
        if (UsingJump)
        {
            Debug.Log($"isjumping : {UsingJump}");
            OnJump();
        }

        bool usingDownDash = (state & (byte)flagPlayerMovementState.UsingDownDash) != 0;

        Debug.Log($"usingDownDash : {usingDownDash}");

        if (!_downDashing && usingDownDash)
        {
            Debug.Log($"usingDownDash : {usingDownDash}");
            _downDashing = usingDownDash;
            DownDash();
        }
    }
    public override void ApplySbyteData(sbyte moveX, sbyte dashX, sbyte dashY)
    {
        float _moveX = moveX;
        _moveVec.x = _moveX;
        _dashDir = new Vector2(dashX, dashY);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + (Vector3)groundCheckVec, groundCheckVecSize);
    }
#endif
}