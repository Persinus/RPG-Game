using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using TMPro;
using Fusion;

public class RewardedGoldAd_Local_FusionSafe : MonoBehaviour
{
    [Header("UI (gán trong Inspector)")]
    public TextMeshProUGUI goldText;
    public Button watchAdButton;

    [Header("Fusion (tự nhận nếu không gán)")]
    [SerializeField] private NetworkRunner runner;

    private int gold = 0;
    private RewardedAd rewardedAd;
    private bool isAdShowing = false;

#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
    private string _adUnitId = "unused";
#endif

    private void Awake()
    {
        // 🔹 Cách khuyến nghị: lấy runner đang chạy trong scene hiện tại
        if (runner == null)
            runner = NetworkRunner.GetRunnerForScene(gameObject.scene);

        // Nếu vẫn chưa có, log cảnh báo (tránh null)
        if (runner == null)
            Debug.LogWarning("[RewardedGoldAd_Local] ⚠️ Chưa tìm thấy NetworkRunner trong scene này.");
    }

    private void Start()
    {
        if (watchAdButton != null)
            watchAdButton.onClick.AddListener(OnWatchAdButtonClicked);
        else
            Debug.LogWarning("[RewardedGoldAd_Local] ⚠️ watchAdButton chưa được gán!");

        gold = 0;
        UpdateGoldText();

        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("✅ Google Mobile Ads SDK initialized (Fusion-safe mode).");
        });
    }

    private void OnWatchAdButtonClicked()
    {
        Debug.Log("🎯 Player clicked Watch Ad button — preparing ad...");
        LoadAndShowRewardedAd();
    }

    private void LoadAndShowRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        var request = new AdRequest();
        Debug.Log("🔄 Loading rewarded ad...");

        RewardedAd.Load(_adUnitId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("❌ Rewarded ad failed to load: " + error);
                return;
            }

            Debug.Log("✅ Rewarded ad loaded — showing now...");
            rewardedAd = ad;
            RegisterEventHandlers(rewardedAd);

            if (rewardedAd.CanShowAd())
            {
                PauseGame(); // Fusion-safe pause

                rewardedAd.Show((Reward reward) =>
                {
                    gold += 100;
                    UpdateGoldText();
                    Debug.Log($"🏅 Reward granted! +100 gold (total: {gold})");
                });
            }
        });
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("❎ Ad closed — resuming game...");
            ResumeGame();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("🚫 Failed to open ad: " + error);
            ResumeGame();
        };
    }

    private void PauseGame()
    {
        isAdShowing = true;
        Time.timeScale = 0f;

        if (runner != null)
            runner.ProvideInput = false;

        Debug.Log("⏸️ Game paused for ad (Fusion-safe).");
    }

    private void ResumeGame()
    {
        isAdShowing = false;
        Time.timeScale = 1f;

        if (runner != null)
            runner.ProvideInput = true;

        Debug.Log("▶️ Game resumed after ad (Fusion-safe).");
    }

    private void UpdateGoldText()
    {
        if (goldText)
            goldText.text = $"Gold: {gold}";
    }

    private void OnDestroy()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
    }
}
