using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Quản lý việc khởi chạy game online thông qua NetworkManager.
/// Khi scene khởi động, script sẽ tự động kiểm tra và gọi StartGame() từ NetworkManager.
/// </summary>
public class GameOnlineManager : MonoBehaviour
{
    // Tham chiếu đến NetworkManager, cần gán trong Inspector
    [SerializeField] private NetworkManager networkManager;

    // Cảnh báo: Hàm Start() này bị đánh dấu obsolete
    // vì nên gọi StartGame() trực tiếp từ NetworkManager thay vì qua GameOnlineManager
    [System.Obsolete("This method is obsolete. Use StartGame() in NetworkManager instead.")]
    void Start()
    {
        // Nếu có gán NetworkManager -> bắt đầu game
        if (networkManager != null)
        {
            networkManager.StartGame();
            Debug.Log("Game started with NetworkManager.");
        }
        else
        {
            // Nếu không có NetworkManager -> báo lỗi
            Debug.LogError("NetworkManager not found in the scene.");
        }
    }
}