using UnityEngine;
using Fusion.Addons.FSM;

public class IdleState : StateBehaviour
{
    [SerializeField] private string idleAnimationName = "idle";

    private PlayerMotor _motor;

    // =========================
    // 🔹 STATE ENTER
    // =========================
    protected override void OnEnterState()
    {
        BindMotor();
        if (_motor == null) return;

        if (_motor.HasStateAuthority)
            _motor.Rpc_PlayAnimation(idleAnimationName);
    }

    // =========================
    // 🔹 FIXED UPDATE
    // =========================
    protected override void OnFixedUpdate()
    {
        if (_motor == null) return;

        // ❗ FSM CHỈ ĐỌC BIẾN NETWORKED
        if (_motor.NetIsMoving)
        {
            Machine.TryActivateState<MoveState>();
            return;
        }

        if (_motor.JumpRequested && _motor.HasStateAuthority)
        {
            _motor.JumpRequested = false;
            Machine.TryActivateState<JumpUpState>();
            return;
        }

        if (_motor.RollRequested && _motor.HasStateAuthority)
        {
            Machine.TryActivateState<RollState>();
            return;
        }
        if (_motor.AttackRequested && _motor.HasStateAuthority)
        {
            _motor.AttackRequested = false;
            Machine.TryActivateState<AttackState>();
            return;
        }
        if (_motor.SkillRequested != SkillType.None && _motor.HasStateAuthority)
        {
            switch (_motor.SkillRequested)
            {
                case SkillType.Skill1:
                    Machine.TryActivateState<Skill1State>();
                    break;
                case SkillType.Skill2:
                    Machine.TryActivateState<Skill2State>();
                    break;
                case SkillType.Skill3:
                    Machine.TryActivateState<Skill3State>();
                    break;
            }
            return;
        }
    }

    // =========================
    // 🔹 EXIT
    // =========================
    protected override void OnExitState()
    {
        if (_motor && _motor.HasStateAuthority)
            _motor.Rpc_PlayAnimation(idleAnimationName);
    }

    // =========================
    // 🔹 HELPER
    // =========================
    private void BindMotor()
    {
        if (_motor == null)
            _motor = GetComponentInParent<PlayerMotor>();
    }
}
