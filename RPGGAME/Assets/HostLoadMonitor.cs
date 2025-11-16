using UnityEngine;
using Fusion;
using Fusion.Statistics;
using System.Linq;

public class FusionHostLoadMonitor : MonoBehaviour
{
    public NetworkRunner runner;

    [Header("NETWORK (KB/s)")]
    public float txKB;
    public float rxKB;

    [Header("NETWORK (Packets)")]
    public int outPackets;
    public int inPackets;

    [Header("SIMULATION (tick)")]
    public int forwardTicks;

    [Header("Players")]
    public int playerCount;

    private FusionStatisticsManager statsManager;

    void Start()
    {
        if (runner == null)
        {
            Debug.LogError("NetworkRunner chưa gán cho FusionHostLoadMonitor!");
            enabled = false;
            return;
        }

        // Lấy FusionStatisticsManager
        if (!runner.TryGetFusionStatistics(out statsManager))
        {
            Debug.LogWarning("Không tìm thấy FusionStatisticsManager trên Runner. Hãy add component FusionStatistics vào Runner.");
        }
    }

    void Update()
    {
        if (statsManager == null)
            return;

        // Snapshot mới
        var snap = statsManager.CompleteSnapshot;

        // Thống kê mạng
        txKB = snap.OutBandwidth / 1024f;
        rxKB = snap.InBandwidth / 1024f;

        outPackets = snap.OutPackets;
        inPackets = snap.InPackets;

        // Tick / simulation
        forwardTicks = snap.ForwardTicks;

        // Số người chơi
        playerCount = runner.ActivePlayers != null ? runner.ActivePlayers.Count() : 0;
    }
}
