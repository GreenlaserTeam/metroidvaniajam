using System;
using System.Collections;
using System.Collections.Generic;
using scr_Interfaces;
using UnityEngine;

public class PlayerMovement : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField]
    private float maxHp;

    [SerializeField]
    private float currentHp;

    [Header("Speed and Force")]
    [SerializeField]
    private float speed = 0;

    [SerializeField]
    protected internal float defaultSpeed = 10;

    [SerializeField]
    private float crouchSpeed = 5;

    [SerializeField]
    protected internal float jumpForce = 25;

    [SerializeField]
    private float rollForce = 1000;

    private float
        playerHeight,
        crouchHeight;

    protected internal bool
        isGrounded,
        isCrouching,
        isRolling;

    private bool
        isInputJump,
        isInputCrouch,
        isInputRoll,
        isInputMoveLeft,
        isInputMoveRight;

    private bool
        isFacingRight,
        isFacingLeft;

    [SerializeField]
    protected internal float fallMultiplier = 7;

    private bool 
        isInsideMech, 
        isReadyForMech, 
        isCollidingWithMech;
   
    [SerializeField]
    private float rollTime;
    [SerializeField]
    private float defaultRollTime = 0.5f;

    protected internal Rigidbody2D _rb;

    [Header("Ground Check")]
    [SerializeField]
    protected internal float groundCheckRadius = 0.5f;
    [SerializeField]
    protected internal float offsetRadius = -1.7f;

    [SerializeField]
    private LayerMask whatIsGround;
    private Vector2 _groundCheckPos;
    private Transform mech;

    public static event Action MechActivated;
    public static event Action MechDeactivated;

    private void SetPlayerRbSettings()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 10;
        _rb.freezeRotation = true;

        playerHeight = transform.localScale.y;
        crouchHeight = playerHeight / 2;
        rollTime = defaultRollTime;

        speed = defaultSpeed;

        isFacingRight = true;
        isFacingLeft = false;
    }

    private void CheckIfGrounded()
    {
        _groundCheckPos = new Vector2(transform.position.x, transform.position.y + offsetRadius);
        var groundCheck =
            Physics2D.OverlapCircle(_groundCheckPos, groundCheckRadius, whatIsGround);

        if (groundCheck)
        {
            isGrounded = true;
        }
    }
    public void TakeDamage(float damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
        {
            // Play death animation

            // Reset level on death
            //SceneManager.LoadScene(1);

            currentHp = 0;
        }
    }
#region MonoBehavior Cycles
    private void Awake()
    {
        SetPlayerRbSettings();
    }
    private void Update()
    {
        CheckRollInput();
        CheckCrouchInput();
        CheckJumpInput();        
        CheckMoveInput();
        CheckMechInput();
        
    }

    protected virtual void FixedUpdate()
    {
        CheckIfGrounded();
        Move();        
        Jump();
        Crouch();
        Roll();
        SwitchToMechAndBack();
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(_groundCheckPos, groundCheckRadius);
    }
#endregion
#region Triggers
    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        isCollidingWithMech = collider.gameObject.GetComponent<MechMovement>();
        mech = collider.gameObject.gameObject.GetComponent<Transform>();

        if (!isInsideMech && isCollidingWithMech)
        {
            isReadyForMech = true;
        }
    }
    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        if (!isInsideMech && isCollidingWithMech)
        {
            isReadyForMech = false;
            isCollidingWithMech = false;
        }
    }
#endregion

#region Inputs
    protected virtual void CheckMechInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (isReadyForMech)
            {
                MechActivated?.Invoke();
                ActivateMech();
            }

            else if (isInsideMech)
            {
                MechDeactivated?.Invoke();
                DeactivateMech();
            }
        }
    }
    protected virtual void CheckCrouchInput()
    {
        if (Input.GetKey(KeyCode.S))
        {
            isInputCrouch = true;
        }

        else
        {
            isInputCrouch = false;
        }
    }

    protected virtual void CheckRollInput()
    {
        if (!isCrouching)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                isInputRoll = true;
            }
        }

        if (isRolling)
        {
            rollTime -= Time.deltaTime;
            if (rollTime <= 0)
            {
                rollTime = 0;
                isRolling = false;
                isInputRoll = false;
            }
        }

        else
        {
            rollTime = defaultRollTime;
        }
    }
    private void CheckMoveInput()
    {
        if (!isRolling)
        {
            if (Input.GetKey(KeyCode.A))
            {
                isInputMoveLeft = true;
            }

            else
            {
                isInputMoveLeft = false;
            }

            if (Input.GetKey(KeyCode.D))
            {
                isInputMoveRight = true;
            }

            else
            {
                isInputMoveRight = false;
            }
        }
    }    
    private void CheckJumpInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            isInputJump = true;
        }

        else
        {
            isInputJump = false;
        }

    }

    #endregion
#region Rigidbody
    protected void FreezeRigidBody()
    {
        _rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }


    protected void UnFreezeRigidBody()
    {
        _rb.constraints = RigidbodyConstraints2D.None;
        _rb.freezeRotation = true;
    }
    #endregion
#region Movement Methods
    protected virtual void Move()
    {
        if (isInputMoveLeft)
        {
            isFacingLeft = true;
            isFacingRight = false;

            _rb.velocity = new Vector2(-speed, _rb.velocity.y);
        }

        else if (isInputMoveRight)
        {
            isFacingRight = true;
            isFacingLeft = false;

            _rb.velocity = new Vector2(speed, _rb.velocity.y);
        }

        else
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
        }
    }
    private void Jump()
    {
        if (isInputJump && isGrounded && !isRolling)
        {
            isGrounded = false;
            _rb.velocity = Vector2.up * jumpForce;
        }
        else if (_rb.velocity.y < 0 && !isInputJump)
        {
            _rb.velocity += (fallMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;
        }
    }
    private void Crouch()
    {
        bool canCrouch = isInputCrouch && isGrounded && !isRolling;

        if (canCrouch)
        {
            isCrouching = true;

            if (transform.localScale.y != crouchHeight)
            {
                transform.localScale = new Vector2(transform.localScale.x, crouchHeight);
            }
            speed = crouchSpeed;
        }
        else
        {
            transform.localScale = new Vector2(transform.localScale.x, playerHeight);
            speed = defaultSpeed;

            isCrouching = false;
        }
    }
    private void Roll()
    {
        Vector2 rollDirection = isFacingLeft ? Vector2.left : Vector2.right;
        bool canRoll = isInputRoll && isGrounded && !isCrouching;

        if (canRoll)
        {
            isRolling = true;

            if (isRolling)
            {
                _rb.AddForce(rollDirection * rollForce, ForceMode2D.Impulse);
                transform.localScale = new Vector2(transform.localScale.x, crouchHeight);
            }
        }

        else if (!isCrouching)
        {
            transform.localScale = new Vector2(transform.localScale.x, playerHeight);
        }
    }
    #endregion        
#region Mech Methods
    private void ActivateMech()
    {        
        isReadyForMech = false;
        isInsideMech = true;
    }

    private void DeactivateMech()
    {
        SetPlayerRbSettings();
        _rb.GetComponent<CapsuleCollider2D>().isTrigger = false;

        isReadyForMech = false;
        isInsideMech = false;        
    }

    private void SwitchToMechAndBack()
    {
        if (isInsideMech)
        {
            this.transform.position = mech.transform.position;
            _rb.GetComponent<CapsuleCollider2D>().isTrigger = true;
            _rb.Sleep();
        }
    }
#endregion
}