using UnityEngine;
using Fusion.Addons.FSM;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(StateMachineController))]
public class PlayerCharacterController : MonoBehaviour, IStateMachineOwner
{
    [SerializeField] private Animator animator;
    [Header("Visual")]
    [SerializeField] private SpriteRenderer sprite;


    public Coroutine FadeRoutine;
    [Header("FSM")]
    [SerializeField] private StateMachineController fsm;
    [SerializeField] private PlayerMotor motor;
    private string _currentAnim;

    private StateMachine<StateBehaviour> _fsm;

    public void SetActive(bool active)
    {
        enabled = active;

        var fsm = GetComponent<StateMachineController>();
        if (fsm) fsm.enabled = active;

        if (animator) animator.enabled = active;
    }

    void IStateMachineOwner.CollectStateMachines(List<IStateMachine> list)
    {
        _fsm = new StateMachine<StateBehaviour>(
            "CharacterFSM",
            GetComponent<IdleState>(),   // 👈 state đầu tiên
            GetComponent<MoveState>(),
            GetComponent<JumpUpState>(),
            GetComponent<JumpDownState>(),
            GetComponent<AttackState>(),
            GetComponent<RollState>(),
            GetComponent<Skill1State>(),
            GetComponent<Skill2State>(),
            GetComponent<Skill3State>(),
            GetComponent<DeathState>()
        );

        list.Add(_fsm);
    }

    public void ResetFSM()
    {
        var fsm = GetComponent<StateMachineController>();
        if (fsm)
        {
            fsm.enabled = false;
            fsm.enabled = true;
        }
    }

    // =========================
    // 🔹 ANIMATION API (BẮT BUỘC)
    // =========================
    public void PlayAnimation(string stateName)
    {
        if (!animator) return;

        animator.CrossFade(stateName, 0.1f);
    }
    // 🔁 BẬT / TẮT NHÂN VẬT (KHÔNG TẮT GAMEOBJECT)
    // =========================
    public void SetActiveCharacter(bool active)
    {
        gameObject.SetActive(active);

        if (active)
        {
            ResetFSM();
        }
    }
    public void SetAlpha(float a)
    {
        if (!sprite) return;
        var c = sprite.color;
        c.a = a;
        sprite.color = c;
    }

    public IEnumerator FadeOut(float duration)
    {
        if (!sprite) yield break;

        float t = 0f;
        Color c = sprite.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / duration);
            sprite.color = c;
            yield return null;
        }

        c.a = 0f;
        sprite.color = c;
    }

    public IEnumerator FadeIn(float duration)
    {
        if (!sprite) yield break;

        float t = 0f;
        Color c = sprite.color;
        c.a = 0f;
        sprite.color = c;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            sprite.color = c;
            yield return null;
        }

        c.a = 1f;
        sprite.color = c;
    }
}
