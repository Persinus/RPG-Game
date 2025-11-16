using UnityEngine;

/// <summary>
/// Quản lý việc khởi chạy game online.
/// Nếu trong scene có NetworkManager, script này chỉ dùng để đảm bảo
/// game mạng được khởi tạo đúng.
/// </summary>
public class GameOnlineManager : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    private async void Start()
    {
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError("❌ NetworkManager not found in the scene!");
                return;
            }
        }

        Debug.Log("🌍 Initializing online session...");
        // Gọi hàm khởi tạo mới trong NetworkManager
        await networkManager.InitializeAndJoin();
    }
}
