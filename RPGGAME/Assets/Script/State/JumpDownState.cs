using UnityEngine;
using Fusion.Addons.FSM;

public class JumpDownState : StateBehaviour
{
    private PlayerNetWorkController _player;
    private Rigidbody2D rb;

    protected override void OnEnterState()
    {
        _player = GetComponent<PlayerNetWorkController>();
        rb = _player.RigidBody2D;

        if (_player.HasStateAuthority)
        {
            // Animation rơi
            _player.RPC_SetAnimation("Jump_Down", false);
        }
    }

    protected override void OnFixedUpdate()
    {
        // Khi chạm đất → về Idle
        if (_player._isGrounded)
        {
            Machine.TryActivateState<IdleState>();
        }
        // 👉 Flip hướng khi rơi
        if (Mathf.Abs(_player._inputData.movement.x) > 0.05f)
        {
            _player.SetFacingDirection(_player._inputData.movement.x);
        }
    }
}
