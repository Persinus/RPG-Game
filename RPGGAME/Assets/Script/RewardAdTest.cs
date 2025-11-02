using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using TMPro;

public class RewardedGoldAd_Local : MonoBehaviour
{
    [Header("UI (gán trong Inspector)")]
    public TextMeshProUGUI goldText;
    public Button watchAdButton;

    private int gold = 0;
    private RewardedAd rewardedAd;

#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-3940256099942544/5224354917"; // Test ID Android
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-3940256099942544/1712485313"; // Test ID iOS
#else
    private string _adUnitId = "unused";
#endif

    private void Start()
    {
        // Gán listener cho nút xem quảng cáo
        if (watchAdButton)
        {
            watchAdButton.onClick.AddListener(OnWatchAdButtonClicked);
        }
        else
        {
            Debug.LogWarning("[RewardedGoldAd_Local] ⚠️ watchAdButton chưa được gán!");
        }

        gold = 0;
        UpdateGoldText();

        // Khởi tạo SDK nhưng không load sẵn quảng cáo
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("✅ Google Mobile Ads SDK initialized (no preloading).");
        });
    }

    // Khi người chơi bấm nút xem quảng cáo
    private void OnWatchAdButtonClicked()
    {
        Debug.Log("🎯 Player clicked Watch Ad button — preparing ad...");
        LoadAndShowRewardedAd();
    }

    // Tải và hiển thị quảng cáo khi có sẵn
    private void LoadAndShowRewardedAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        AdRequest request = new AdRequest();
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

            // Chỉ hiển thị sau khi load thành công
            if (rewardedAd.CanShowAd())
            {
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
            Debug.Log("❎ Ad closed — ready for next click.");
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("🚫 Failed to open ad: " + error);
        };
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
