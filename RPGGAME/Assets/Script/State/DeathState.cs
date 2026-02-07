using Fusion;
using UnityEngine;
using Fusion.Addons.FSM;
public class DeathState : StateBehaviour
{
    [SerializeField] private string deathAnimationName = "Death";
    private PlayerMotor motor;

    protected override void OnEnterState()
    {
        motor ??= GetComponentInParent<PlayerMotor>();
        if (!motor || !motor.HasStateAuthority) return;

        motor.Rpc_PlayAnimation(deathAnimationName);
    }

    protected override void OnFixedUpdate()
    {
        // Không làm gì trong trạng thái chết
    }

    protected override void OnExitState()
    {
        // Không làm gì khi thoát trạng thái chết
    }
}