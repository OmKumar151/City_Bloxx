using System;
using UnityEngine;

#if CITYBLOXX_ADMOB
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
#endif

/// <summary>
/// AdMob: the main-menu banner and the ad shown before a restart.
///
/// Created automatically; nothing needs adding to a scene. Without the
/// Google Mobile Ads plugin and the CITYBLOXX_ADMOB scripting define, no
/// ads are shown and restarts go straight through.
///
/// Uses Google's test ad units until the real IDs below are filled in.
/// Never tap your own live ads: it can get the AdMob account banned.
/// </summary>
public class AdsService : MonoBehaviour
{
    public static AdsService Instance { get; private set; }

    // ---- Replace with your AdMob ad unit IDs before release. ----
    private const string BannerUnitId = "ca-app-pub-3940256099942544/6300978111";   // test
    private const string RewardedUnitId = "ca-app-pub-3940256099942544/5224354917"; // test

    // Static so a request made before this object exists still counts.
    private static bool wantBanner;

#if CITYBLOXX_ADMOB
    private bool initialised;
    private BannerView banner;
    private RewardedAd rewarded;
    private bool loadingRewarded;
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (Instance != null)
            return;

        GameObject go = new GameObject(nameof(AdsService));
        DontDestroyOnLoad(go);
        go.AddComponent<AdsService>();
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
#if CITYBLOXX_ADMOB
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        // GDPR/UK consent: shows Google's consent form only where required.
        ConsentInformation.Update(new ConsentRequestParameters(), updateError =>
        {
            if (updateError != null)
                Debug.LogWarning("AdsService: Consent update failed: " + updateError.Message);

            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                if (formError != null)
                    Debug.LogWarning("AdsService: Consent form failed: " + formError.Message);

                if (ConsentInformation.CanRequestAds())
                    Initialise();
            });
        });
#endif
    }

    // =========================================================
    // BANNER (main menu)
    // =========================================================

    public static void ShowBanner()
    {
        wantBanner = true;

        if (Instance == null)
            return;
#if CITYBLOXX_ADMOB
        Instance.CreateBannerIfReady();
#endif
    }

    public static void HideBanner()
    {
        wantBanner = false;

        if (Instance == null)
            return;
#if CITYBLOXX_ADMOB
        Instance.banner?.Destroy();
        Instance.banner = null;
#endif
    }

    // =========================================================
    // RESTART AD
    // =========================================================

    /// <summary>
    /// Plays a full-screen ad, then calls onAllowed if it was watched to the
    /// end. If no ad is available (offline, nothing to fill), onAllowed is
    /// called straight away so the player is never stuck. onCancelled is
    /// called if the player closes the ad early.
    /// </summary>
    public static void ShowRestartAd(Action onAllowed, Action onCancelled = null)
    {
#if CITYBLOXX_ADMOB
        if (Instance != null && Instance.TryShowRewarded(onAllowed, onCancelled))
            return;
#endif
        onAllowed?.Invoke();
    }

#if CITYBLOXX_ADMOB
    private void Initialise()
    {
        MobileAds.Initialize(_ =>
        {
            initialised = true;
            CreateBannerIfReady();
            LoadRewarded();
        });
    }

    private void CreateBannerIfReady()
    {
        if (!initialised || !wantBanner || banner != null)
            return;

        AdSize size = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(
            AdSize.FullWidth);

        banner = new BannerView(BannerUnitId, size, AdPosition.Bottom);
        banner.LoadAd(new AdRequest());
    }

    private void LoadRewarded()
    {
        if (!initialised || loadingRewarded || rewarded != null)
            return;

        loadingRewarded = true;

        RewardedAd.Load(RewardedUnitId, new AdRequest(), (ad, error) =>
        {
            loadingRewarded = false;

            if (error != null || ad == null)
            {
                Debug.LogWarning("AdsService: Rewarded ad failed to load: " + error);
                return;
            }

            rewarded = ad;
        });
    }

    private bool TryShowRewarded(Action onAllowed, Action onCancelled)
    {
        if (rewarded == null || !rewarded.CanShowAd())
        {
            LoadRewarded();
            return false;
        }

        RewardedAd ad = rewarded;
        rewarded = null;

        bool earned = false;

        ad.OnAdFullScreenContentClosed += () =>
        {
            ad.Destroy();
            LoadRewarded();

            if (earned)
                onAllowed?.Invoke();
            else
                onCancelled?.Invoke();
        };

        ad.OnAdFullScreenContentFailed += error =>
        {
            ad.Destroy();
            LoadRewarded();
            Debug.LogWarning("AdsService: Rewarded ad failed to show: " + error);

            // Our fault, not the player's: let them restart.
            onAllowed?.Invoke();
        };

        ad.Show(_ => earned = true);
        return true;
    }
#endif
}
