using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;

/// <summary>
/// PlayerSpawner — quản lý việc sinh (spawn) và huỷ (despawn) player trong môi trường mạng.
/// - Khi có player mới join: server/host sẽ spawn prefab tương ứng.
/// - Khi player rời phòng: server sẽ despawn đối tượng đó.
/// - Chỉ server có quyền spawn / despawn.
/// </summary>
public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Prefab cấu hình trong Project")]
    [SerializeField] private NetworkPrefabRef playerPrefab; // prefab của player

    // Lưu danh sách player đang tồn tại (key = PlayerRef, value = NetworkObject)
    private Dictionary<PlayerRef, NetworkObject> spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();


    //========================================
    // ⚙️ Hệ thống callback mặc định của Unity
    //========================================
    void Start()
    {
        // Có thể dùng để khởi tạo data tĩnh, hiển thị UI chờ kết nối, v.v...
    }


    //========================================
    // 🧩 Fusion Callbacks — xử lý sự kiện mạng
    //========================================

    /// <summary>
    /// Gọi khi player mới tham gia phòng.
    /// </summary>
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // Chỉ host/server mới có quyền spawn player
        if (!runner.IsServer)
        {
            Debug.Log($"Client {player} joined (no spawn authority).");
            return;
        }

        Debug.Log($"Player {player} joined. Spawning character...");

        // Lấy tổng số player trong phòng để tính vị trí spawn tương đối
        int totalPlayers = Mathf.Max(1, runner.Config.Simulation.PlayerCount);
        int index = player.RawEncoded % totalPlayers;

        // Lấy thứ tự player join (bắt đầu từ 1)
        int playerIndex = spawnedCharacters.Count + 1;
        string playerName = $"Player {playerIndex}";
        
        // 🔹 Tạo vị trí spawn xung quanh gốc toạ độ (0, 0)
        float radius = 4f;
        float angle = (index / (float)totalPlayers) * Mathf.PI * 2f;

        float x = Mathf.Cos(angle) * radius + Random.Range(-1f, 1f);
        float y = 1.0f; // cao hơn mặt đất một chút

        Vector2 spawnPosition = new Vector2(x, y);

        // 🔹 Spawn player prefab cho người chơi này
        var playerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);

        // Ghi lại vào danh sách quản lý
        spawnedCharacters[player] = playerObject;


        //---------------------------------------------------
        // THÊM DÒNG QUAN TRỌNG: GÁN TÊN QUA RPC
        //---------------------------------------------------
        var controller = playerObject.GetComponent<Player_Name_NetWorkController>();
        controller.RPC_SetPlayerName(playerName);

        Debug.Log($"✅ Spawned Player {playerName} | PlayerRef {player}");
        Debug.Log($"✅ Spawned PlayerRef {player} at {spawnPosition} - InputAuthority: {playerObject.InputAuthority}");
    }


    /// <summary>
    /// Gọi khi player rời phòng.
    /// </summary>
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (spawnedCharacters.TryGetValue(player, out NetworkObject playerObject))
        {
            runner.Despawn(playerObject);
            spawnedCharacters.Remove(player);
            Debug.Log($"🚪 Player {player} left and object despawned.");
        }
    }


    //========================================
    // 📡 Các callback còn lại (tuỳ chọn)
    //========================================

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.LogWarning($"⚠️ Disconnected from server: {reason}");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"❌ Connect failed to {remoteAddress}: {reason}");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
