using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý toàn bộ quá trình khởi tạo mạng trong Fusion.
/// 
/// - Instance đầu tiên chạy sẽ trở thành Host.
/// - Những instance sau tự động join làm Client.
/// - Tự động join lobby để lấy session list thật từ Photon.
/// </summary>
public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    [Header("Session Config")]
    [SerializeField] private string sessionName = "RoomTest"; // Tên session mặc định
    [SerializeField] private int delayBeforeJoinMs = 300;      // Delay nhỏ tránh race condition
    private bool _hasStarted = false; // Ngăn gọi StartGame nhiều lần

    private async void Start()
    {
        DontDestroyOnLoad(gameObject);
        await InitializeAndJoin();
    }

    /// <summary>
    /// Khởi tạo runner, join lobby, và xử lý tạo/join session.
    /// </summary>
    public async Task InitializeAndJoin()
    {
        if (_hasStarted) return;
        _hasStarted = true;

        Debug.Log("🚀 Initializing Fusion NetworkRunner...");

        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
        // Join lobby trước — để nhận session list
        var lobbyResult = await _runner.JoinSessionLobby(SessionLobby.Shared);
        if (!lobbyResult.Ok)
        {
            Debug.LogError($"❌ Failed to join lobby: {lobbyResult.ShutdownReason}");
            return;
        }

        Debug.Log("✅ Joined lobby, waiting for session list from Photon...");
    }

    /// <summary>
    /// Được gọi khi Photon trả về danh sách session trong lobby.
    /// Từ đây ta sẽ quyết định join hay tạo session.
    /// </summary>
    public async void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"📋 Session list updated: {sessionList.Count} sessions");

        // Nếu session tồn tại => join client, không thì tạo host
        bool sessionExists = sessionList.Any(s => s.Name == sessionName);

        // Delay nhỏ để tránh race khi nhiều instance join đồng thời
        await Task.Delay(delayBeforeJoinMs);

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        var args = new StartGameArgs()
        {
            GameMode = sessionExists ? GameMode.Client : GameMode.Host,
            SessionName = sessionName,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        };

        var result = await _runner.StartGame(args);

        if (result.Ok)
            Debug.Log($"✅ {(_runner.GameMode == GameMode.Host ? "Host" : "Client")} started successfully!");
        else
            Debug.LogError($"❌ Failed to start NetworkRunner: {result.ShutdownReason}");
    }

    // ============================================================
    // 🔧 Fusion Callbacks
    // ============================================================

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        => Debug.Log($"👋 Player joined: {player}");

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        => Debug.Log($"🚪 Player left: {player}");

    public void OnConnectedToServer(NetworkRunner runner)
        => Debug.Log("✅ Connected to server");

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        => Debug.LogWarning($"⚠️ Disconnected from server: {reason}");

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        => Debug.LogError($"❌ Connect failed: {reason}");

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"🛑 NetworkRunner shut down: {shutdownReason}");
        CleanupRunner();
    }

    private void CleanupRunner()
    {
        if (_runner != null)
        {
            _runner.Shutdown();
            Destroy(_runner);
            _runner = null;
        }
    }

    // Các callback còn lại (không cần logic riêng)
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
