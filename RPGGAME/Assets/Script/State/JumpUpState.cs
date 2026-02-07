using UnityEngine;
using Fusion.Addons.FSM;
public class JumpUpState : StateBehaviour
{
    [SerializeField] private string jumpUpAnimationName = "JumpUp";

    private PlayerMotor motor;
    private Rigidbody2D rb;

    protected override void OnEnterState()
    {
        Bind();
        if (!motor || !motor.HasStateAuthority) return;

        motor.ApplyJump(); // ⭐ CHỈ GỌI 1 LẦN Ở ĐÂY
        motor.Rpc_PlayAnimation(jumpUpAnimationName);
    }

    protected override void OnFixedUpdate()
    {
        if (!motor || !motor.HasStateAuthority) return;

        float x = motor.InputData.movement.x;
        if (Mathf.Abs(x) > 0.05f)
            motor.SetFacingDirection(x);

        if (rb.linearVelocity.y <= 0f)
            Machine.TryActivateState<JumpDownState>();
    }

    protected override void OnExitState() { }

    private void Bind()
    {
        motor ??= GetComponentInParent<PlayerMotor>();
        rb ??= GetComponentInParent<Rigidbody2D>();
    }
}
