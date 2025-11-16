using UnityEngine;
using Fusion;
using Fusion.Addons.FSM;
using Unity.Cinemachine;
using System.Collections.Generic;

[RequireComponent(typeof(StateMachineController))]
public class PlayerController : NetworkBehaviour, IStateMachineOwner
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

        // Move player
        if (rigidbody2D != null)  
            rigidbody2D.linearVelocity = new Vector2(_inputData.movement.x * moveSpeed, rigidbody2D.linearVelocity.y);

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
            GetComponent<MoveState>()
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
}
