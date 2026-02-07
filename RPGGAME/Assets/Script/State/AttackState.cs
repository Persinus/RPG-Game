using UnityEngine;
using Fusion.Addons.FSM;

public class AttackState : StateBehaviour
{
    [SerializeField] private string attackAnimationName = "Attack";

    private PlayerMotor _motor;

    protected override void OnEnterState()
    {
        BindMotor();
        if (_motor == null || !_motor.HasStateAuthority)
            return;

        _motor.StartAttack();
        _motor.Rpc_PlayAnimation(attackAnimationName);
    }

    protected override void OnFixedUpdate()
    {
        if (_motor == null || !_motor.HasStateAuthority)
            return;

        if (!_motor.IsAttacking)
        {
            if (_motor.NetIsMoving)
                Machine.TryActivateState<MoveState>();
            else
                Machine.TryActivateState<IdleState>();
        }
    }

    protected override void OnExitState()
    {
        // không play animation ở đây
    }

    private void BindMotor()
    {
        if (_motor == null)
            _motor = GetComponentInParent<PlayerMotor>();
    }
}
