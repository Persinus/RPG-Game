using UnityEngine;
using Fusion.Addons.FSM;

public class JumpUpState : StateBehaviour
{
    private PlayerNetWorkController _player;
    private Rigidbody2D rb;

    protected override void OnEnterState()
    {
        _player = GetComponent<PlayerNetWorkController>();
        rb = _player.RigidBody2D;

        if (_player.HasStateAuthority)
        {
            // Animation nhảy lên
            _player.RPC_SetAnimation("Jump_Up", false);

            // Reset Y để không double force
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

            // Add lực nhảy
            rb.AddForce(Vector2.up * _player.jumpForce, ForceMode2D.Impulse);

            // Tắt request jump
            _player._jumpRequested = false;
        }
    }

    protected override void OnFixedUpdate()
    {
        // 👉 Cho flip hướng khi đang nhảy
        if (Mathf.Abs(_player._inputData.movement.x) > 0.05f)
        {
            _player.SetFacingDirection(_player._inputData.movement.x);
        }
        // Khi bắt đầu rơi → chuyển JumpDown
        if (rb.linearVelocity.y < 0)
        {
            Machine.TryActivateState<JumpDownState>();
            return;
        }
    }
    protected override void OnExitState()
    {
        // Reset jump request khi rời state
        _player._jumpRequested = false;
    }
}
