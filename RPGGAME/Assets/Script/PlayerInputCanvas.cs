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
    public Button jumpButton;

    [HideInInspector] public NetworkRunner runner;

    private NetworkInputData currentInput;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        attackButton?.onClick.AddListener(() => currentInput.Click(InputButtons.Attack));
        jumpButton?.onClick.AddListener(() => currentInput.Click(InputButtons.Jump));
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
    public void OnDisconnectedFromServer(NetworkRunner runner, Fusion.Sockets.NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, Fusion.Sockets.NetAddress remoteAddress, Fusion.Sockets.NetConnectFailedReason reason) { }
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
