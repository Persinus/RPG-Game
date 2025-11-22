using UnityEngine;
using Fusion.Addons.FSM;

public class MoveState : StateBehaviour
{
    private PlayerNetWorkController _player;
    private Rigidbody2D _rb;

    private const float MOVE_THRESHOLD = 0.05f;

    protected override void OnEnterState()
    {
        _player = GetComponent<PlayerNetWorkController>();
        _rb = _player.RigidBody2D;

        if (_player.HasStateAuthority)
            _player.RPC_SetAnimation("Run", true);
    }

    protected override void OnFixedUpdate()
    {
        if (!_player.HasStateAuthority || !GetInput(out _player._inputData))
            return;

        var rb = _player.RigidBody2D;

        // Lấy Y hiện tại (gravity tự xử lý)
        float currentY = rb.linearVelocity.y;

        // Chỉ điều khiển X
        float targetX = _player._inputData.movement.x * _player.moveSpeed;

        rb.linearVelocity = new Vector2(targetX, currentY);

          // ⭐ Flip hướng bằng hàm trong PlayerController
        _player.SetFacingDirection(_player._inputData.movement.x);

        // Chuyển về trạng thái Idle nếu không có input di chuyển
        if (Mathf.Abs(_player._inputData.movement.x) < MOVE_THRESHOLD)
        {
            Machine.TryActivateState<IdleState>();
        }
    }
}
