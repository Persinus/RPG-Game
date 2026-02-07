using Fusion;
using UnityEngine;

public class PlayerCameraBinder : NetworkBehaviour
{
    [SerializeField] private LocalCinemachineCamera localCamera;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            // LOCAL PLAYER
            localCamera.gameObject.SetActive(true);
            localCamera.Enable(transform);
        }
        else
        {
            // REMOTE PLAYER
            localCamera.Disable();
            localCamera.gameObject.SetActive(false);
        }
    }
}