using UnityEngine;
using Fusion;
using Unity.Cinemachine;

/// <summary>
/// Thiết lập player khi được spawn trong môi trường mạng (Fusion).
/// - Gán UI và camera riêng cho player local.
/// - Camera dùng Cinemachine v3 mới (CinemachineCamera).
/// </summary>
public class NetworkPlayerSetup2D : NetworkBehaviour
{
    [Header("Prefabs (Kéo vào từ Project)")]
    [SerializeField] private GameObject uiPrefab;         // UI của player local
    [SerializeField] private GameObject cameraPrefab;     // Prefab chứa CinemachineCamera

    private GameObject playerUIInstance;
    private GameObject camInstance;
    private CinemachineCamera cinemachineCam;             // Camera Cinemachine v3

    public override void Spawned()
    {
        // Chỉ tạo UI và camera cho player local (người điều khiển)
        if (!Object.HasInputAuthority)
            return;

        Debug.Log("[NetworkPlayerSetup2D] 👤 Local player spawned — spawning UI & camera...");

        // 🧩 Khởi tạo UI
        if (uiPrefab != null)
        {
            playerUIInstance = Instantiate(uiPrefab);
            playerUIInstance.name = "LocalPlayerUI";
            DontDestroyOnLoad(playerUIInstance);
        }
        else
        {
            Debug.LogWarning("[UI] ⚠️ uiPrefab chưa được gán!");
        }

        // 🎥 Khởi tạo camera
        if (cameraPrefab != null)
        {
            camInstance = Instantiate(cameraPrefab);
            camInstance.name = "LocalCinemachineCam";
            DontDestroyOnLoad(camInstance);

            // Lấy component CinemachineCamera trong prefab
            cinemachineCam = camInstance.GetComponent<CinemachineCamera>();
            if (cinemachineCam != null)
            {
                // Gán player làm target để camera bám theo
                cinemachineCam.Follow = transform;
                cinemachineCam.LookAt = transform;
                Debug.Log("[Camera] ✅ CinemachineCamera follow player.");
            }
            else
            {
                Debug.LogWarning("[Camera] ⚠️ Prefab camera không chứa CinemachineCamera!");
            }
        }
        else
        {
            Debug.LogWarning("[Camera] ⚠️ cameraPrefab chưa được gán!");
        }
    }

    private void OnDestroy()
    {
        // Chỉ dọn dẹp tài nguyên cho player local
        if (!Object.HasInputAuthority)
            return;

        if (playerUIInstance != null)
            Destroy(playerUIInstance);

        if (camInstance != null)
            Destroy(camInstance);
    }
}
