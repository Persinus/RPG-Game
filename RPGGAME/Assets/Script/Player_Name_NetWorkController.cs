using TMPro;
using Fusion;
using UnityEngine;

public class Player_Name_NetWorkController : NetworkBehaviour
{
    [Header("Name UI")]
    [SerializeField] private TextMeshProUGUI playerNameTMP;

    [Header("Optional: Holder for flipping")]
    [SerializeField] private Transform nameHolder; // parent chứa TextMeshPro, giữ scale ổn định

    [Networked] public string PlayerName { get; set; }

    public override void Render()
    {
        if (playerNameTMP != null)
        {
            // Cập nhật tên
            playerNameTMP.text = PlayerName;

            if (nameHolder != null)
            {
                Vector3 holderScale = nameHolder.localScale;
                float originalX = Mathf.Abs(holderScale.x); // giữ magnitude ban đầu
                holderScale.x = originalX * Mathf.Sign(transform.localScale.x);
                nameHolder.localScale = holderScale;
            }
            else
            {
                Vector3 textScale = playerNameTMP.transform.localScale;
                float originalX = Mathf.Abs(textScale.x);
                textScale.x = originalX * Mathf.Sign(transform.localScale.x);
                playerNameTMP.transform.localScale = textScale;
            }

        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetPlayerName(string name)
    {
        PlayerName = name;
    }
}
