using UnityEngine;
using Fusion.Addons.FSM;

public class RollState : StateBehaviour
{
    [SerializeField] private string rollAnimationName = "Roll";

    private PlayerMotor _motor;

    // =========================
    // STATE ENTER
    // =========================
    protected override void OnEnterState()
    {
        BindMotor();
        if (_motor == null || !_motor.HasStateAuthority)
            return;

        // 🚀 gọi logic roll chuẩn trong PlayerMotor
        _motor.StartRoll();

        // 🎬 animation
        _motor.Rpc_PlayAnimation(rollAnimationName);
    }

    // =========================
    // FIXED UPDATE
    // =========================
    protected override void OnFixedUpdate()
    {
        if (_motor == null || !_motor.HasStateAuthority)
            return;

        // ⏹ roll kết thúc → quyết định state tiếp theo
        if (!_motor.IsRolling)
        {
            if (_motor.NetIsMoving)
                Machine.TryActivateState<MoveState>();
            else
                Machine.TryActivateState<IdleState>();
        }
    }

    // =========================
    // STATE EXIT
    // =========================
    protected override void OnExitState()
    {
        // ❗ không reset IsRolling ở đây
        // PlayerMotor.UpdateRoll() tự xử lý
    }

    // =========================
    // HELPER
    // =========================
    private void BindMotor()
    {
        if (_motor == null)
            _motor = GetComponentInParent<PlayerMotor>();
    }
}
