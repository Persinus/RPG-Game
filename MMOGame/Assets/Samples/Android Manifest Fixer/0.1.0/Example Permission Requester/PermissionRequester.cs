using UnityEngine;
using UnityEngine.Android;

/// <summary>
/// Ví dụ minh họa cách xin quyền ở Android runtime.
/// Tool sẽ tự thêm permission vào AndroidManifest nếu thiếu.
/// </summary>
public class PermissionRequester : MonoBehaviour
{
    void Start()
    {
#if UNITY_ANDROID
        // Xin quyền camera nếu chưa được cấp
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Debug.Log("[PermissionRequester] Chưa có quyền CAMERA, đang xin...");
            Permission.RequestUserPermission(Permission.Camera);
        }

        // Ví dụ xin quyền đọc bộ nhớ
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Debug.Log("[PermissionRequester] Chưa có quyền READ_EXTERNAL_STORAGE, đang xin...");
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
#endif
    }
}
