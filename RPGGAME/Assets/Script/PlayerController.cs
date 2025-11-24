using UnityEngine;
using Fusion;
using Fusion.Addons.FSM;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine.TextCore;

[RequireComponent(typeof(StateMachineController))]
public class PlayerNetWorkController : NetworkBehaviour, IStateMachineOwner
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rigidbody2D;
    [SerializeField] private Animator animator;

    [Header("Prefabs")]
    [SerializeField] private GameObject uiPrefab;       // Canvas MobileInput
    [SerializeField] private GameObject cameraPrefab;   // Cinemachine camera

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public bool _isGrounded;
    public float jumpForce = 8f;        // lực nhảy

    public bool _jumpRequested = false; // request nhảy từ input

    [HideInInspector] public NetworkInputData _inputData;

    [Header("FSM Debug")]
    [SerializeField] private string currentStateName;
    [SerializeField] private float movementX;
    [SerializeField] private float movementY;
    [SerializeField] private Vector2 velocity;

    private StateMachine<StateBehaviour> _stateMachine;
    private string _lastState;

    // Local UI & Camera
    private GameObject playerUIInstance;
    private GameObject camInstance;
    private CinemachineCamera cinemachineCam;

    public Rigidbody2D RigidBody2D => rigidbody2D;

    public override void Spawned()
    {
        // Chỉ player local spawn UI & Camera
        if (Object.HasInputAuthority)
        {
            SpawnLocalUIAndInput();
            SpawnCamera();
        }

        // Set animation ban đầu
        if (HasStateAuthority)
            RPC_SetAnimation("Idle", true);
    }

    private void SpawnLocalUIAndInput()
    {
        if (!uiPrefab) return;
        playerUIInstance = Instantiate(uiPrefab);
        playerUIInstance.name = "LocalPlayerUI";
        DontDestroyOnLoad(playerUIInstance);

        var mobileInput = playerUIInstance.GetComponent<MobileInputCanvas>();
        if (mobileInput != null)
        {
            mobileInput.runner = Object.Runner;
            Object.Runner.AddCallbacks(mobileInput);
        }
    }

    private void SpawnCamera()
    {
        if (!cameraPrefab) return;
        camInstance = Instantiate(cameraPrefab);
        camInstance.name = "LocalCinemachineCam";
        DontDestroyOnLoad(camInstance);

        cinemachineCam = camInstance.GetComponent<CinemachineCamera>();
        if (cinemachineCam != null)
        {
            cinemachineCam.Follow = transform;
            cinemachineCam.LookAt = transform;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !GetInput(out _inputData))
            return;

        // Update inspector debug
        movementX = _inputData.movement.x;

        if (rigidbody2D) velocity = rigidbody2D.linearVelocity;

        // Update facing direction based on horizontal input (allow mid-air turning)
        if (Mathf.Abs(_inputData.movement.x) > 0.05f)
            SetFacingDirection(_inputData.movement.x);

        // Move player
        // Preserve Y velocity (gravity). If FSM is active it should control movement;
        // otherwise apply horizontal movement here as a fallback.
        if (rigidbody2D != null)
        {
            if (_stateMachine == null || _stateMachine.ActiveState == null)
            {
                float currentY = rigidbody2D.linearVelocity.y;
                rigidbody2D.linearVelocity = new Vector2(_inputData.movement.x * moveSpeed, currentY);
            }
        }

        // Nếu kéo joystick lên -> request jump
        if (_inputData.movement.y > 0.5f && _isGrounded)
        {
            _jumpRequested = true;
        }
        // -------------- HANDLE ATTACK -----------------
        if (_inputData.Clicked(InputButtons.Attack))
        {
            _jumpRequested = false; // không nhảy khi attack
            _stateMachine.TryActivateState<AttackState>();
            return;  // dừng xử lý movement
        }
        // -------------- HANDLE ATTACK 1 -----------------
        if (_inputData.Clicked(InputButtons.Skill1))
        {
            _jumpRequested = false; // không nhảy khi attack
            _stateMachine.TryActivateState<Skill1State>();
            return;  // dừng xử lý movement
        }

        // -------------- HANDLE ATTACK 2 -----------------
        if (_inputData.Clicked(InputButtons.Skill2))
        {
            _jumpRequested = false; // không nhảy khi attack
            _stateMachine.TryActivateState<Skill2State>();
            return;  // dừng xử lý movement
        }

        // -------------- HANDLE ATTACK 3 -----------------
        if (_inputData.Clicked(InputButtons.Skill3))
        {
            _jumpRequested = false; // không nhảy khi attack
            _stateMachine.TryActivateState<Skill3State>();
            return;  // dừng xử lý movement
        }
        // -------------- HANDLE DASH -----------------
        if (_inputData.Clicked(InputButtons.Dash))
        {
            _jumpRequested = false; // không nhảy khi dash
            _stateMachine.TryActivateState<DashState>();
            return;  // dừng xử lý movement
        }
        // -------------- HANDLE ROLL -----------------
        if (_inputData.Clicked(InputButtons.Roll))
        {
            _jumpRequested = false; // không nhảy khi roll
            _stateMachine.TryActivateState<RollState>();
            return;  // dừng xử lý movement
        }
        // FSM debug
        if (_stateMachine != null && _stateMachine.ActiveState != null)
        {
            currentStateName = _stateMachine.ActiveState.GetType().Name;
            if (_lastState != currentStateName)
                _lastState = currentStateName;

        }


    }

    public bool HasMovementInput() =>
        Mathf.Abs(_inputData.movement.x) > 0.05f;

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetAnimation(string stateName, bool loop)
    {
        if (animator != null)
            animator.Play(stateName);
    }

    void IStateMachineOwner.CollectStateMachines(List<IStateMachine> stateMachines)
    {
        _stateMachine = new StateMachine<StateBehaviour>("PlayerFSM",
            GetComponent<IdleState>(),
            GetComponent<MoveState>(),
            GetComponent<JumpUpState>(),
            GetComponent<JumpDownState>(),
            GetComponent<AttackState>(),
            GetComponent<Skill1State>(),
            GetComponent<Skill2State>(),
            GetComponent<Skill3State>(),
            GetComponent<DashState>(),
            GetComponent<RollState>()

        );
        stateMachines.Add(_stateMachine);
    }

    public void SetFacingDirection(float x)
    {
        if (x == 0) return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Sign(x) * Mathf.Abs(scale.x); // giữ nguyên size, chỉ đổi dấu
        transform.localScale = scale;
    }
    private void OnDestroy()
    {
        if (playerUIInstance) Destroy(playerUIInstance);
        if (camInstance) Destroy(camInstance);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Kiểm tra nếu chạm đất
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true;
        }
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        // Kiểm tra nếu rời khỏi đất
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = false;
        }
    }


}
