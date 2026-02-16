using Unity.Services.LevelPlay;
using UnityEngine.Purchasing;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class AdManager : MonoBehaviour
{
    [SerializeField] float _timeBtwAds = 20;

    static public AdManager Instance;

    public const string kProductNoAdsId = "removeads"; // ID défini dans Google Console
    public const string kAppKey = "249adda8d"; //App Id LevelPlay
    public const string kInterstitialAdUnitId = "soacqgjpcxz7xrh5"; //Ad Unit Id LevelPlay

    StoreController _storeController;
    public bool _IAPIsInit { get; protected set; }

    float _timer;
    public bool _adsRemoved { get; protected set; } = false;

    public string _localizedRemoveAdsPrice { get; private set; }

    public Action<bool> GotPurchases;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        InitializeIAP();
    }

    void Start()
    {
        // Active la Test Suite officielle IronSource
        Debug.Log("LEVELPLAY TEST MODE ENABLED");
        LevelPlay.SetMetaData("is_test_suite", "enable");

        // Register OnInitFailed and OnInitSuccess listeners
        LevelPlay.OnInitSuccess += OnSdkInitializationCompleted;
        LevelPlay.OnInitFailed += OnSdkInitializationFailed;
        // SDK init
        LevelPlay.Init(kAppKey);
    }

    void Update()
    {
        if (_adsRemoved) return;
        _timer += Time.unscaledDeltaTime;
    }

    public void RequestIntersticeIfNeeded(Action<bool> pCallBack = null)
    {
        Debug.Log("vho _IAPIsInit : " + _IAPIsInit + " _adsRemoved  : " + _adsRemoved + " _timer : " + _timer);
        if (_IAPIsInit && !_adsRemoved)
            if (_timer < _timeBtwAds)
            {
                pCallBack?.Invoke(false);
                return;
            }
            else
            {
                Debug.Log("vho add needed");
                _timer = 0;
                RequestInterstice(pCallBack);
            }
        else pCallBack.Invoke(false);
    }

    void RequestInterstice(Action<bool> pCallBack = null)
    {
        bool vFinished = false;

        Debug.Log("Loading interstitial...");
        LevelPlayInterstitialAd vInterstitialAd = new LevelPlayInterstitialAd(kInterstitialAdUnitId);
        StartCoroutine(Timeout());

        void Finish(bool pSuccess)
        {
            if (vFinished) return;
            vFinished = true;

            StopCoroutine(Timeout());
            pCallBack?.Invoke(pSuccess);

            vInterstitialAd.DestroyAd();
        }
        IEnumerator Timeout()
        {
            yield return new WaitForSecondsRealtime(3);

            Debug.LogWarning("Interstitial timeout reached");
            Finish(false);
        }

        vInterstitialAd.OnAdLoaded += (LevelPlayAdInfo pAdInfo) =>
        {
            Debug.Log("Ad loaded");
            vInterstitialAd.ShowAd();
        };
        vInterstitialAd.OnAdLoadFailed += (LevelPlayAdError pInfo) =>
        {
            Debug.Log("interstitial loading failed : " + pInfo.ErrorMessage);
            Finish(false);
        };
        vInterstitialAd.OnAdDisplayFailed += (LevelPlayAdInfo pAdInfo, LevelPlayAdError pInfo) =>
        {
            Debug.Log("interstitial display failed : " + pInfo.ErrorMessage);
            Finish(false);
        };
        vInterstitialAd.OnAdClosed += (LevelPlayAdInfo pInfo) =>
        {
            Finish(true);
        };

        vInterstitialAd.LoadAd();
    }

    async void InitializeIAP()
    {
        _storeController = UnityIAPServices.StoreController();

        _storeController.OnStoreDisconnected += (StoreConnectionFailureDescription _) => { Debug.LogError("Initialize IAP store controller connection fail :  " + _.message); };
        _storeController.OnProductsFetched += OnProductsFetched;
        _storeController.OnProductsFetchFailed += (ProductFetchFailed _) => { return; };
        _storeController.OnPurchasesFetched += OnPurchasesFetched;
        _storeController.OnPurchasesFetchFailed += (PurchasesFetchFailureDescription _) => { return; };
        _storeController.OnPurchasePending += OnPurchasePending;
        _storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;

        await _storeController.Connect();

        var vProductsToFetch = new List<ProductDefinition> { new(kProductNoAdsId, ProductType.NonConsumable) };
        _storeController.FetchProducts(vProductsToFetch);
        _storeController.FetchPurchases();

        _IAPIsInit = true;
    }


    void OnProductsFetched(List<Product> pProducts)
    {
        foreach (Product lProduct in pProducts)
            if (lProduct.definition.id == kProductNoAdsId)
            {
                _localizedRemoveAdsPrice = lProduct.metadata.localizedPriceString;
                break;
            }
    }

    void OnPurchasesFetched(Orders pOrders)
    {
        _adsRemoved = false;

        foreach (ConfirmedOrder order in pOrders.ConfirmedOrders)
        {
            foreach (IPurchasedProductInfo lProductInfo in order.Info.PurchasedProductInfo)
            {
                if (lProductInfo.productId == kProductNoAdsId)
                {
                    _adsRemoved = true;
                    GotPurchases?.Invoke(_adsRemoved);
                    break;
                }
            }
        }
    }

    void OnPurchaseConfirmed(Order pOrder)
    {
        _storeController.FetchPurchases();
    }

    void OnPurchasePending(Order pOrder)
    {
        return;
    }


    void OnSdkInitializationCompleted(LevelPlayConfiguration pConfig)
    {
        Debug.Log("Mobile Ads initialization complete.");
    }

    void OnSdkInitializationFailed(LevelPlayInitError pError)
    {
        Debug.LogError("Mobile Ads initialization failed.");
    }
}


