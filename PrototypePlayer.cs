using UnityEngine;
using UnityEngine.InputSystem;

public class PrototypePlayer : Entity
{
    // Enum to keep track of player state. It's crude, but it works.
    internal enum PlayerState
    {
        Moving,
        Attacking,
        Hitstun
    }
    #region Inspector Setup
    [Header("Player Data")]
    [SerializeField] private PlayerData m_playerData;
    [SerializeField] private PlayerInputHandler m_playerInputHandler;

    // Rendering
    [Header("Appearance")]
    [SerializeField] private Animator m_animator;
    [SerializeField] private string m_animatorSpeedParameter = "Input";
    [SerializeField] private string m_animatorAttackParameter = "Attack";
    [SerializeField] private string m_animatorComboParameter = "In Combo";
    [SerializeField] private SpriteRenderer m_playerSprite;

    [Header("Hitboxes")]
    [SerializeField] private Hitbox[] playerHitboxes;
    private int animatorSpeedParamHash = 0;
    private int animatorAttackHash = 0;
    private int animatorComboHash = 0;
    #endregion

    #region Setup
    private void CalculateVelocities()
    {
        // calculate gravity for maxJumpHeight
        jumpGravity = -(2 * m_playerData.maxJumpHeight) / Mathf.Pow(m_playerData.timeToJumpApex, 2);
        gravity = jumpGravity * m_playerData.fallGravityMultiplier;

        jumpForce = Mathf.Abs(jumpGravity) * m_playerData.timeToJumpApex;

        // calculate accl / deaccl
        accelerationValue = m_playerData.baseSpeed / m_playerData.accelerationTime;
        deaccelerationValue = m_playerData.baseSpeed / m_playerData.deaccelerationTime;
    }

    private void SetupAnimator()
    {
        animatorSpeedParamHash = Animator.StringToHash(m_animatorSpeedParameter);
        animatorAttackHash = Animator.StringToHash(m_animatorAttackParameter);
        animatorComboHash = Animator.StringToHash(m_animatorComboParameter);
    }

    private void SetupHealth()
    {
        maxHP = currentHP = m_playerData.baseHitPoints;
        foreach (var hitbox in playerHitboxes)
        {
            hitbox.SetOwner(this);
        }
    }
    #endregion

    #region Combat
    private bool m_isAttacking = false;
    [SerializeField] private bool m_canAttack = true;

    private void Die()
    {
        GameManager.Instance.DoHitstun(1f);
        Destroy(gameObject);
    }

    private void StartAttack()
    {
        m_isAttacking = true;
        m_animator.SetBool(animatorComboHash, m_isAttacking);
    }

    private void StopAttack()
    {
        m_isAttacking = false;
        m_animator.SetBool(animatorComboHash, m_isAttacking);
    }
    #endregion

    #region Movement
    private float accelerationValue;
    private float deaccelerationValue;
    private float jumpGravity = -20f;
    private float gravity = -20f;
    private float jumpForce = 20f;
    private Vector2 velocity = new(0, 0);
    private bool overrideJumpGravity = false;

    private void HandleMovement(Vector2 input)
    {
        // Don't allow movement on ground while attacking
        if (m_isAttacking && controller.IsGrounded)
        {
            input.x = 0;
        }
        // Modify x velocity
        var velX = velocity.x;

        // Acceleration-based movement. Accelerate towards max speed on input, deaccelerate to 0 on no input.
        if (input.x != 0)
        {
            // Air acceleration
            var accel = accelerationValue;
            accel *= controller.IsGrounded ? 1 : m_playerData.airAccelerationModifier;

            // Move player
            velX += input.x * Time.deltaTime * accel;
            velX = Mathf.Clamp(velX, -m_playerData.baseSpeed, m_playerData.baseSpeed);
        }
        else
        {
            // Air acceleration
            var accel = deaccelerationValue;
            accel *= controller.IsGrounded ? 1 : m_playerData.airAccelerationModifier;


            if (Mathf.Abs(velX) >= accel * Time.deltaTime)
                velX += -Mathf.Sign(velX) * accel * Time.deltaTime;
            else
                velX = 0;
        }

        // Gravity handling + coyote time handling
        var velY = velocity.y;
        var g = (!overrideJumpGravity && velY > 0) ? jumpGravity : gravity;
        currentCoyoteTime += Time.deltaTime;

        // If we are colliding with something above us, set velocity to zero
        if (controller.collisionState.Above)
        {
            velY = 0f;
        }

        // Gravity
        if (!controller.IsGrounded)
            velY += Time.deltaTime * g;

        // Grounded velocity correction
        if (velY <= 0 && controller.IsGrounded)
        {
            // Don't set this to zero or it will mess with the grounded boolean!
            velY = -0.002f;
            currentCoyoteTime = 0f;
            overrideJumpGravity = hasJumped = false;
        }

        // If we're grounded and we are pushing down, let's jump down this platform
        if (controller.IsGrounded && shouldDropNextFrame)
        {
            velY += Time.deltaTime * g * 4;
            controller.ignoreOneWayPlatformsThisFrame = true;
            shouldDropNextFrame = false;
        }

        // Clamp our velocity to a max value
        velY = Mathf.Clamp(velY, gravity, float.MaxValue);
        velocity.y = velY;
        velocity.x = velX;

        // If we're in the air, don't let the player turn
        canTurn = controller.IsGrounded;

        // Sprite flipping
        if (canTurn && velocity.x != 0)
        {
            m_playerSprite.flipX = velocity.x < 0;
        }

        foreach (var hitbox in playerHitboxes)
        {
            var pos = hitbox.transform.localPosition;
            pos.x = Mathf.Abs(pos.x);
            if (m_playerSprite.flipX) pos.x *= -1;
            hitbox.transform.localPosition = pos;
        }

        controller.Move(velocity * Time.deltaTime);
    }
    #endregion

    #region Jumping
    private float currentCoyoteTime = 0f;
    private bool hasJumped = false;
    private bool hasCanceledJump = false;
    private bool shouldDropNextFrame = false;

    private bool CanJump()
    {
        return !hasJumped && (controller.IsGrounded || currentCoyoteTime < m_playerData.coyoteTime);
    }

    private void CancelJump()
    {
        overrideJumpGravity = true;
        hasCanceledJump = true;
    }

    private void DropThroughPlatform()
    {
        if (controller.IsGrounded)
        {
            shouldDropNextFrame = true;
        }
    }
    #endregion

    #region Components & Flags
    private bool canTurn = true;
    #endregion

    #region Unity
    private new void Start()
    {
        base.Start();
        CalculateVelocities();
        SetupAnimator();
        SetupHealth();
        GameManager.Instance.Initialize();
    }

    private new void OnEnable()
    {
        base.OnEnable();
        m_playerInputHandler.OnJumpCanceled += CancelJump;
        m_playerInputHandler.OnDrop += DropThroughPlatform;
        OnDeath += Die;
    }

    private new void OnDisable()
    {
        base.OnDisable();
        m_playerInputHandler.OnJumpCanceled -= CancelJump;
        m_playerInputHandler.OnDrop -= DropThroughPlatform;
        OnDeath -= Die;
    }

    private void Update()
    {
        if (m_playerInputHandler.bufferedInput == InputBufferType.Jump)
        {
            if (!CanJump()) return;
            overrideJumpGravity = false;
            m_playerInputHandler.ConsumeBuffer();
            velocity.y = jumpForce;
            hasJumped = true;
            hasCanceledJump = m_playerInputHandler.JumpPhase == InputActionPhase.Canceled;
        }

        else if (m_playerInputHandler.bufferedInput == InputBufferType.Attack)
        {
            if (!m_canAttack) return;
            m_playerInputHandler.ConsumeBuffer();
            m_animator.SetTrigger(animatorAttackHash);
        }

        if (m_playerInputHandler.JumpPhase == InputActionPhase.Performed)
        {
            if (!hasCanceledJump)
                overrideJumpGravity = false;
            else
            {
                overrideJumpGravity = true;
                hasCanceledJump = true;
            }
        }

        if (hasCanceledJump || m_playerInputHandler.JumpPhase == InputActionPhase.Canceled)
        {
            overrideJumpGravity = true;
            hasCanceledJump = true;
        }
    }
    private new void FixedUpdate()
    {
        base.FixedUpdate();
        // Get input
        var input = m_playerInputHandler.dpadInput;
        HandleMovement(input);
        m_animator.SetFloat(animatorSpeedParamHash, Mathf.Abs(input.x));
    }
    #endregion


#if ENABLE_DEBUG_UI
        private void OnGUI()
        {
            // Debug stats
            GUI.Label(new Rect(0, 0, 200, 20), $"Coyote time: {currentCoyoteTime}");
            GUI.Label(new Rect(0, 20, 200, 20), $"Has jumped: {hasJumped}");
            GUI.Label(new Rect(0, 40, 200, 20), $"Is grounded: {controller.IsGrounded}");
            GUI.Label(new Rect(0, 60, 200, 20), $"Can jump: {CanJump()}");
            GUI.Label(new Rect(0, 80, 200, 20), $"Cancelled jump: {hasCanceledJump}");
            GUI.Label(new Rect(0, 140, 200, 20), $"Velocity: {velocity}");
        }
#endif
}