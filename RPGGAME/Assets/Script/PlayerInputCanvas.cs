using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;

public class MobileInputCanvas : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Joystick")]
    public Joystick moveJoystick;

    [Header("Buttons")]
    public Button attackButton;
    public Button skill1Button;
    public Button skill2Button;
    public Button skill3Button;
    public Button rollButton;
    public Button switchButton;   // 👈 THÊM


    private NetworkInputData currentInput;
    private NetworkRunner runner;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log("[MobileInputCanvas] Awake");

        attackButton?.onClick.AddListener(() =>
        {
            currentInput.Click(InputButtons.Attack);
            Debug.Log("[INPUT] Attack button clicked");
        });

        rollButton?.onClick.AddListener(() =>
        {
            currentInput.Click(InputButtons.Roll);
            Debug.Log("[INPUT] Roll button clicked");
        });

        skill1Button?.onClick.AddListener(() =>
        {
            currentInput.Click(InputButtons.Skill1);
            Debug.Log("[INPUT] Skill1 button clicked");
        });

        skill2Button?.onClick.AddListener(() =>
        {
            currentInput.Click(InputButtons.Skill2);
            Debug.Log("[INPUT] Skill2 button clicked");
        });

        skill3Button?.onClick.AddListener(() =>
        {
            currentInput.Click(InputButtons.Skill3);
            Debug.Log("[INPUT] Skill3 button clicked");
        });
        switchButton?.onClick.AddListener(() =>
        {
            currentInput.Click(InputButtons.Switch);
            Debug.Log("[INPUT] Switch button clicked");
        });
    }

    /// <summary>
    /// PHẢI được gọi sau khi Runner tạo xong
    /// </summary>
    public void Register(NetworkRunner networkRunner)
    {
        runner = networkRunner;
        runner.AddCallbacks(this);
        Debug.Log("[MobileInputCanvas] Registered to NetworkRunner");
    }

    // ===== FUSION INPUT =====
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        Vector2 move = new Vector2(
            moveJoystick ? moveJoystick.Horizontal : 0,
            moveJoystick ? moveJoystick.Vertical : 0
        );

        currentInput.movement = move;

        Debug.Log($"[OnInput] Movement: {move}");

        input.Set(currentInput);
        currentInput.Reset();
    }

    #region Unused callbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Runner] Player joined: {player}");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Runner] Player left: {player}");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"[Runner] Shutdown: {shutdownReason}");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("[Runner] Connected to server");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"[Runner] Disconnected: {reason}");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
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
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }}
    #endregion