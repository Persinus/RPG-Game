using UnityEngine;
using Fusion.Addons.FSM;

public class RollState : StateBehaviour
{
    private PlayerNetWorkController _player;
    private Rigidbody2D rb;

    [Header("Roll Settings")]
    public float rollDistance = 5f;
    public float rollDuration = 0.3f;
    private float rollTimer;
    private Vector2 rollDirection;

    protected override void OnEnterState()
    {
        _player = GetComponent<PlayerNetWorkController>();
        rb = _player.RigidBody2D;

        if (_player.HasStateAuthority)
        {
            // Animation roll
            _player.RPC_SetAnimation("Roll", false);

            // Xác định hướng roll dựa vào facing
            rollDirection = new Vector2(Mathf.Sign(_player.transform.localScale.x), 0);

            // Reset timer
            rollTimer = 0f;
        }
    }

    protected override void OnFixedUpdate()
    {
        if (!_player.HasStateAuthority) return;

        // Di chuyển theo hướng roll
        rb.linearVelocity = new Vector2(rollDirection.x * rollDistance / rollDuration, rb.linearVelocity.y);

        // Tăng timer
        rollTimer += Runner.DeltaTime;

        // Kết thúc roll sau rollDuration
        if (rollTimer >= rollDuration)
        {
            Machine.TryActivateState<IdleState>();
        }
    }

    protected override void OnExitState()
    {
        // Reset velocity sau khi roll
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
}
