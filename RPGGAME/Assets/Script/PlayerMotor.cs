using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMotor : NetworkBehaviour
{
    private Rigidbody2D rb;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    [Header("Roll")]
    public float rollSpeed = 12f;
    public float rollDuration = 0.35f;

    [Header("Joystick")]
    public float jumpThreshold = 0.6f;

    // ===== NETWORKED =====
    [Networked] public bool IsGrounded { get; set; }

    // ===== FSM FLAGS =====
    [HideInInspector] public bool IsMoving;
    [HideInInspector] public bool JumpRequested;
    [HideInInspector] public bool RollRequested;
    [HideInInspector] public bool IsRolling;
    [HideInInspector] public float RollDirection;

    [Networked] public bool NetIsMoving { get; set; }

    [Networked] public bool IsActiveCharacter { get; set; } = true;


    [Header("Skill Durations")]
    public float skill1Duration = 0.5f;
    public float skill2Duration = 0.8f;
    public float skill3Duration = 1.2f;

    [HideInInspector] public SkillType SkillRequested;


    [HideInInspector] public bool IsUsingSkill;

    private float skillTimer;
    [SerializeField] private AvatarSwitcher avatarSwitcher;
    // ===== INPUT =====
    [HideInInspector] public NetworkInputData InputData;

    // ===== INTERNAL =====
    private bool jumpConsumed;
    private bool rollConsumed;
    private float rollTimer;

    // ===== FSM FLAGS =====
    [HideInInspector] public bool AttackRequested;
    [HideInInspector] public bool IsAttacking;

    // ===== INTERNAL =====
    private bool attackConsumed;
    private float attackTimer;

    [Networked] public int NetFacing { get; set; } // -1 = trái, 1 = phải


    [Header("Attack")]
    public float attackDuration = 0.6f;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void FixedUpdateNetwork()
    {
        ApplyFacing(); // ⭐ chạy cho ALL
        if (!HasStateAuthority) return;
        if (!IsActiveCharacter) return; // ⭐ CỐT LÕI FIX LỖI

        if (!GetInput(out InputData)) return;

        // ✅ SWITCH CHỈ 1 LẦN
        if (avatarSwitcher != null &&
            InputData.Clicked(InputButtons.Switch))
        {
            avatarSwitcher.SwitchAvatar();
        }

        HandleMovement();
        HandleJumpRequest();
        HandleRollRequest();
        UpdateRoll();
        HandleAttackRequest();   // ⭐ THÊM
        UpdateAttack();          // ⭐ THÊM
                                 // ⭐ SKILL
        HandleSkillRequest();
        StartSkill();
        UpdateSkill();

    }

    // =======================
    // MOVE
    // =======================
    private void HandleMovement()
    {
        if (IsRolling || IsAttacking) return;

        float moveX = InputData.movement.x;
        bool moving = Mathf.Abs(moveX) > 0.05f;

        IsMoving = moving;          // local logic
        NetIsMoving = moving;       // SYNC cho client

        rb.linearVelocity = new Vector2(
            moveX * moveSpeed,
            rb.linearVelocity.y
        );

        if (moving)
            RollDirection = Mathf.Sign(moveX);

        SetFacingDirection(moveX);
    }

    public void ApplyFacing()
    {
        if (NetFacing == 0) return;

        transform.localScale = new Vector3(
            NetFacing,
            1f,
            1f
        );
    }

    // =======================
    // JUMP (joystick ↑)
    // =======================
    private void HandleJumpRequest()
    {
        float moveY = InputData.movement.y;
        bool wantsJump = moveY > jumpThreshold;

        if (wantsJump && IsGrounded && !jumpConsumed)
        {
            JumpRequested = true;
            jumpConsumed = true;
        }

        if (!wantsJump)
            jumpConsumed = false;
    }

    public void ApplyJump()
    {
        rb.linearVelocity = new Vector2(
        rb.linearVelocity.x,
        0f // reset trước
    );

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        IsGrounded = false;
        JumpRequested = false;
    }

    // =======================
    // ROLL (joystick ↓ hoặc nút riêng)
    // =======================
    private void HandleRollRequest()
    {
        bool wantsRoll = InputData.movement.y < -0.6f;
        if (IsAttacking) return;

        if (wantsRoll && IsGrounded && !rollConsumed && !IsRolling)
        {
            RollRequested = true;
            rollConsumed = true;
        }

        if (!wantsRoll)
            rollConsumed = false;
    }

    public void StartRoll()
    {
        IsRolling = true;
        RollRequested = false;
        rollTimer = rollDuration;

        rb.linearVelocity = new Vector2(
            RollDirection * rollSpeed,
            0f
        );
    }
    public void Attack()
    {
        // Placeholder for attack logic

    }

    private void UpdateRoll()
    {
        if (!IsRolling) return;

        rollTimer -= Runner.DeltaTime;

        rb.linearVelocity = new Vector2(
            RollDirection * rollSpeed,
            rb.linearVelocity.y
        );

        if (rollTimer <= 0f)
        {
            IsRolling = false;
        }
    }

    // =======================
    // GROUND CHECK
    // =======================
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!HasStateAuthority) return;

        if (col.gameObject.CompareTag("Ground"))
            IsGrounded = true;
    }

    // =======================
    // FACING
    // =======================
    public void SetFacingDirection(float dir)
    {
        if (dir > 0.1f)
            NetFacing = 1;
        else if (dir < -0.1f)
            NetFacing = -1;
    }

    private void HandleAttackRequest()
    {
        bool wantsAttack = InputData.Clicked(InputButtons.Attack);

        if (wantsAttack && !attackConsumed && !IsAttacking)
        {
            AttackRequested = true;
            attackConsumed = true;
        }

        if (!wantsAttack)
            attackConsumed = false;
    }

    public void StartAttack()
    {
        IsAttacking = true;
        AttackRequested = false;
        attackTimer = attackDuration;

        // ⛔ khóa di chuyển
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void UpdateAttack()
    {
        if (!IsAttacking) return;

        attackTimer -= Runner.DeltaTime;

        if (attackTimer <= 0f)
        {
            IsAttacking = false;
        }
    }

    private void HandleSkillRequest()
    {
        if (IsRolling || IsAttacking || IsUsingSkill)
            return;

        if (SkillRequested != SkillType.None)
            return;

        if (InputData.Clicked(InputButtons.Skill1))
            SkillRequested = SkillType.Skill1;
        else if (InputData.Clicked(InputButtons.Skill2))
            SkillRequested = SkillType.Skill2;
        else if (InputData.Clicked(InputButtons.Skill3))
            SkillRequested = SkillType.Skill3;
    }
    private void StartSkill()
    {
        if (SkillRequested == SkillType.None)
            return;

        IsUsingSkill = true;

        // ⛔ khóa di chuyển
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        switch (SkillRequested)
        {
            case SkillType.Skill1:
                skillTimer = skill1Duration;
                Rpc_PlayAnimation("Skill1");
                break;

            case SkillType.Skill2:
                skillTimer = skill2Duration;
                Rpc_PlayAnimation("Skill2");
                break;

            case SkillType.Skill3:
                skillTimer = skill3Duration;
                Rpc_PlayAnimation("Skill3");
                break;
        }

        SkillRequested = SkillType.None;
    }
    private void UpdateSkill()
    {
        if (!IsUsingSkill)
            return;

        skillTimer -= Runner.DeltaTime;

        if (skillTimer <= 0f)
        {
            IsUsingSkill = false;
        }
    }

    // =======================
    // ANIMATION RPC
    // =======================
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void Rpc_PlayAnimation(string animState)
    {
        var characters = GetComponentsInChildren<PlayerCharacterController>(true);

        foreach (var c in characters)
        {
            if (c.gameObject.activeInHierarchy)
            {
                c.PlayAnimation(animState);
                break;
            }
        }
    }
    private void OnCollisionStay2D(Collision2D col)
    {
        if (!HasStateAuthority) return;

        if (col.gameObject.CompareTag("Ground"))
        {
            IsGrounded = true;


        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (!HasStateAuthority) return;

        if (col.gameObject.CompareTag("Ground"))
            IsGrounded = false;
    }


}
