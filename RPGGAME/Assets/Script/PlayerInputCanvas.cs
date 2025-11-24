using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;

public class MobileInputCanvas : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Joystick")]
    public Joystick moveJoystick;

    [Header("Action Buttons")]
    public Button attackButton;
    public Button skill1Button;
    public Button skill2Button;
    public Button skill3Button;

   [Header ("Skill")]
    public Button DashButton;
    public Button RollButton;  

    [HideInInspector] public NetworkRunner runner;

    private NetworkInputData currentInput;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        attackButton?.onClick.AddListener(() => currentInput.Click(InputButtons.Attack));
        skill1Button?.onClick.AddListener(() => currentInput.Click(InputButtons.Skill1));
        skill2Button?.onClick.AddListener(() => currentInput.Click(InputButtons.Skill2));
        skill3Button?.onClick.AddListener(() => currentInput.Click(InputButtons.Skill3));
        DashButton?.onClick.AddListener(() => currentInput.Click(InputButtons.Dash));
        RollButton?.onClick.AddListener(() => currentInput.Click(InputButtons.Roll));
    }

    private void Update()
    {
        // Lấy giá trị joystick
        currentInput.movement = new Vector2(moveJoystick.Horizontal, moveJoystick.Vertical);
    }

    // Fusion gọi mỗi tick
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
          NetworkInputData inputData = new NetworkInputData
    {
        movement = new Vector2(moveJoystick.Horizontal, moveJoystick.Vertical)
    };
    input.Set(inputData);
        input.Set(currentInput);
        currentInput.Reset(); // Reset click mỗi frame
    }

    #region Dummy callbacks
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner,NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, System.Collections.Generic.Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    #endregion
}
