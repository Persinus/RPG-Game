using UnityEngine;
using Fusion.Addons.FSM;

public class JumpDownState : StateBehaviour
{
    [SerializeField] private string jumpDownAnimationName = "JumpDown";
    private PlayerMotor motor;

    protected override void OnEnterState()
    {
        motor ??= GetComponentInParent<PlayerMotor>();
        if (!motor || !motor.HasStateAuthority) return;

        motor.Rpc_PlayAnimation(jumpDownAnimationName);
    }

    protected override void OnFixedUpdate()
    {
        if (!motor || !motor.HasStateAuthority) return;

        float x = motor.InputData.movement.x;
        if (Mathf.Abs(x) > 0.05f)
            motor.SetFacingDirection(x);

        if (motor.IsGrounded)
            Machine.TryActivateState<IdleState>();
    }

    protected override void OnExitState() { }
}

