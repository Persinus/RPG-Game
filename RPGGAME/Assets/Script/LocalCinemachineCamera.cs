using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera))]
public class LocalCinemachineCamera : MonoBehaviour
{
    private CinemachineCamera cam;

    private void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
        cam.enabled = false; // mặc định tắt
    }

    public void Enable(Transform target)
    {
        cam.Follow = target;
        cam.LookAt = target;
        cam.enabled = true;

        Debug.Log("🎥 Camera ENABLE for " + target.name);
    }

    public void Disable()
    {
        cam.enabled = false;
    }
}