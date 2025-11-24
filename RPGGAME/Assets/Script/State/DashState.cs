using UnityEngine;
using Fusion.Addons.FSM;

public class DashState : StateBehaviour
{
    private PlayerNetWorkController _player;
    private Rigidbody2D rb;

    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    private float dashTimer;
    private Vector2 dashDirection;

    protected override void OnEnterState()
    {
        _player = GetComponent<PlayerNetWorkController>();
        rb = _player.RigidBody2D;

        if (_player.HasStateAuthority)
        {
            // Animation dash
            _player.RPC_SetAnimation("Dash", false);

            // Xác định hướng dash dựa vào input hoặc facing
            dashDirection = new Vector2(
                _player._inputData.movement.x != 0 ? Mathf.Sign(_player._inputData.movement.x) : Mathf.Sign(_player.transform.localScale.x),
                0
            );

            // Reset timer
            dashTimer = 0f;
        }
    }

    protected override void OnFixedUpdate()
    {
        if (!_player.HasStateAuthority) return;

        // Di chuyển theo hướng dash
        rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, rb.linearVelocity.y);

        // Tăng timer
        dashTimer += Runner.DeltaTime;

        // Kết thúc dash sau dashDuration
        if (dashTimer >= dashDuration)
        {
            Machine.TryActivateState<IdleState>();
        }
    }

    protected override void OnExitState()
    {
        // Reset velocity sau khi dash
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
}
