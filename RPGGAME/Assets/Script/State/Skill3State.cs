using UnityEngine;
using Fusion.Addons.FSM;

public class Skill3State : StateBehaviour
{
private PlayerMotor _motor;

    [Header("Skill 1")]
    [SerializeField] private float attackDuration ;
    [SerializeField] private string skill1AnimationName ;

    // =========================
    protected override void OnEnterState()
    {
        _motor = GetComponent<PlayerMotor>();
        if (_motor == null || !_motor.HasStateAuthority)
            return;

        // Khóa di chuyển khi đánh
        _motor.IsMoving = false;

        // Bật animation
        _motor.Rpc_PlayAnimation(skill1AnimationName);
    }

    // =========================
    protected override void OnFixedUpdate()
    {
        if (_motor == null || !_motor.HasStateAuthority)
            return;

        if (Machine.StateTime >= attackDuration)
        {
            Machine.TryActivateState<IdleState>();
        }
    }

    // =========================
    protected override void OnExitState()
    {
        if (_motor == null || !_motor.HasStateAuthority)
            return;

        // Tắt animation
        _motor.Rpc_PlayAnimation(skill1AnimationName);

        // Mở lại movement
        _motor.IsMoving = true;
    }
}
