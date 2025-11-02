using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý toàn bộ quá trình khởi tạo mạng trong Fusion.
/// 
/// - Instance đầu tiên chạy sẽ trở thành Host.
/// - Những instance sau tự động join làm Client.
/// - Có delay nhỏ để tránh lỗi khi nhiều instance start đồng thời.
/// 
/// ⚙️ Hỗ trợ auto-random tên session nếu cần test nhiều host.
/// </summary>
public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;

    [Header("Session Config")]
    [SerializeField] private string sessionName = "RoomTest"; // Tên session mặc định
    [SerializeField] private bool autoRandomSession = false;   // Nếu bật, sẽ thêm GUID vào session name
    [SerializeField] private int delayBeforeJoinMs = 500;      // Thời gian delay (ms) cho client join

    private bool sessionAvailable = false;

    private void Awake()
    {
        // Giữ lại khi load scene mới (tránh bị destroy)
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Khởi tạo Fusion Network Runner và bắt đầu game.
    /// 
    /// - Nếu chưa có session: chạy ở mode Host.
    /// - Nếu đã có session tồn tại: tự động join vào làm Client.
    /// 
    /// 🧩 Tự động add NetworkSceneManagerDefault để load scene.
    /// </summary>
    public async void StartGame()
    {
        // Nếu runner cũ đang chạy thì shutdown trước
        if (_runner != null)
        {
            if (_runner.IsRunning)
                _runner.Shutdown();

            _runner = null;
        }

        // Tạo runner mới
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        // Lấy scene hiện tại để đồng bộ cho player khác
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        // Chế độ auto host/client (Fusion tự chọn)
        GameMode mode = GameMode.AutoHostOrClient;

        // Nếu bật random session name -> thêm GUID
        string actualSessionName = sessionName;
        if (autoRandomSession && mode == GameMode.Host)
            actualSessionName += "_" + Guid.NewGuid().ToString("N");

        // Delay nhỏ để giảm khả năng va chạm session khi test nhiều instance
        await Task.Delay(delayBeforeJoinMs);

        Debug.Log($"🚀 Starting Fusion: Mode={mode}, Session='{actualSessionName}'");

        // Cấu hình khởi tạo runner
        var startArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = actualSessionName,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        };

        // Bắt đầu game
        var result = await _runner.StartGame(startArgs);

        if (result.Ok)
            Debug.Log("✅ NetworkRunner started successfully!");
        else
            Debug.LogError($"❌ Failed to start NetworkRunner: {result.ShutdownReason}");
    }

    /// <summary>
    /// Dọn runner cũ (shutdown và hủy component)
    /// </summary>
    private void CleanupRunner()
    {
        if (_runner != null)
        {
            Debug.Log("🧹 Cleaning up NetworkRunner...");
            _runner.Shutdown();
            Destroy(_runner);
            _runner = null;
        }
    }

    #region INetworkRunnerCallbacks (Fusion Events)
    // Các hàm callback của Fusion — được gọi tự động khi có event mạng xảy ra

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"🛑 NetworkRunner shut down. Reason: {shutdownReason}");
        CleanupRunner();
    }

    public void OnConnectedToServer(NetworkRunner runner)
        => Debug.Log("✅ Connected to server");

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        => Debug.LogWarning($"⚠️ Disconnected from server: {reason}");

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        => Debug.LogError($"❌ Connect failed to {remoteAddress}: {reason}");

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        => Debug.Log($"👋 Player joined: {player}");

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        => Debug.Log($"🚪 Player left: {player}");

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        => Debug.LogWarning($"Missing input from player: {player}");

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        => Debug.Log($"🔗 Connection request from {request.RemoteAddress}");

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        => Debug.Log("📨 Simulation message received");

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        // Cập nhật danh sách session từ server
        sessionAvailable = false;
        foreach (var session in sessionList)
        {
            if (session.Name == sessionName)
            {
                sessionAvailable = true;
                break;
            }
        }
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        => Debug.Log("🔐 Custom authentication response received");

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
        => Debug.Log("✅ Scene load done");

    public void OnSceneLoadStart(NetworkRunner runner)
        => Debug.Log("⏳ Scene load started");

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        => Debug.Log($"👁️ Object {obj.name} exited AOI for player {player}");

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        => Debug.Log($"👁️ Object {obj.name} entered AOI for player {player}");

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        => Debug.Log($"📡 Reliable data received from {player}");

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        => Debug.Log($"📡 Reliable data progress from {player}: {progress * 100}%");
    #endregion
}