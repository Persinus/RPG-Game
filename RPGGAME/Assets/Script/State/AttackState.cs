using UnityEngine;
using Fusion.Addons.FSM;

public class AttackState : StateBehaviour
{
    private PlayerNetWorkController _player;

    [SerializeField] private float attackDuration = 1f; // Thời gian animation Attack1

    protected override void OnEnterState()
    {
        _player = GetComponent<PlayerNetWorkController>();

        if (_player.HasStateAuthority)
        {
            // Gọi animation Attack1
            _player.RPC_SetAnimation("Attack1", false);
        }
    }

    protected override void OnFixedUpdate()
    {
        // Khi animation đã chạy xong → về Idle
        if (Machine.StateTime > attackDuration)
        {
            Machine.TryActivateState<IdleState>();
        }
    }
}
