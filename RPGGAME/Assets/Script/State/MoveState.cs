using UnityEngine;
using Fusion.Addons.FSM;

public class MoveState : StateBehaviour
{
    [SerializeField] private string moveAnimationName = "run";
    private PlayerMotor motor;

    protected override void OnEnterState()
    {
        motor ??= GetComponentInParent<PlayerMotor>();
        if (motor == null) return;

        if (motor.HasStateAuthority)
            motor.Rpc_PlayAnimation(moveAnimationName);
    }

    protected override void OnFixedUpdate()
    {
        if (motor == null) return;

        // ❗ FSM chỉ đọc biến NETWORKED
        if (!motor.NetIsMoving)
        {
            Machine.TryActivateState<IdleState>();
        }
    }

    protected override void OnExitState()
    {
        if (motor && motor.HasStateAuthority)
            motor.Rpc_PlayAnimation(moveAnimationName);
    }
}
