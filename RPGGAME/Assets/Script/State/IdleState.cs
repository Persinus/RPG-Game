using UnityEngine;
using Fusion.Addons.FSM;

public class IdleState : StateBehaviour
{
    private PlayerController _player;
    protected override void OnEnterState()
    {
        _player = GetComponent<PlayerController>();
        if (_player.HasStateAuthority)
        {
            _player.RPC_SetAnimation("Idle", true); // Đồng bộ hoạt ảnh đứng yên cho tất cả client
        }
    }
    protected override void OnFixedUpdate()
    {
       if (_player.HasMovementInput())
        {
            Machine.TryActivateState<MoveState>();
        }
    }
}
