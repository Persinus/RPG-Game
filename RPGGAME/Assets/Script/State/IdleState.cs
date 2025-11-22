using UnityEngine;
using Fusion.Addons.FSM;

public class IdleState : StateBehaviour
{
    private PlayerNetWorkController _player;
    protected override void OnEnterState()
    {
        _player = GetComponent<PlayerNetWorkController>();
        if (_player.HasStateAuthority)
        {
            _player.RPC_SetAnimation("Idle", true); // Đồng bộ hoạt ảnh đứng yên cho tất cả client
        }
    }
    protected override void OnFixedUpdate()
    {
       // 1. nếu có hướng → Move
        if (_player.HasMovementInput())
        {
            Machine.TryActivateState<MoveState>();
            return;
        }

        // 2. nếu request Jump → JumpUp
        if (_player._jumpRequested && _player._isGrounded)
        {
            Machine.TryActivateState<JumpUpState>();
            return;
        }
    
    }
}