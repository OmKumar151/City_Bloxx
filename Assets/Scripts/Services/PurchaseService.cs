// Real Google Play billing only on the phone. In the Editor, purchases
// stay simulated (instant unlock) even with CITYBLOXX_IAP set, so the
// flow can be tested on the laptop without the store.
#if CITYBLOXX_IAP && !UNITY_EDITOR
#define CITYBLOXX_REAL_STORE
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

#if CITYBLOXX_REAL_STORE
using UnityEngine.Purchasing;
using Unity.Services.Core;
#endif

/// <summary>
/// In-app purchases (Unity IAP 5, Google Play Billing). Used to unlock a
/// city straight away instead of reaching its population requirement.
///
/// Created automatically; nothing needs adding to a scene.
///   - In the Editor, purchases always succeed instantly (for testing).
///   - In an Android build with the CITYBLOXX_IAP define (set in Player
///     Settings) and the In-App Purchasing package, Google Play billing.
///   - In a build without the define, purchases report "store unavailable".
///
/// Every product here is a non-consumable: bought once, owned forever.
/// Ownership is saved through SaveStore (so Play Games cloud save keeps it)
/// and is restored from the store on every launch (reinstall, new phone).
///
/// The grant is client-side only. For a paid game with real revenue at
/// stake, add receipt validation (Unity's CrossPlatformValidator or a
/// server) before granting.
/// </summary>
public class PurchaseService : MonoBehaviour
{
    public static PurchaseService Instance { get; private set; }

    private const string OwnedPrefix = "CityBloxx_Purchased_";

#if UNITY_IOS
    private const string StoreName = "the App Store";
#else
    private const string StoreName = "Google Play";
#endif

    /// <summary>Raised when a product becomes owned (bought or restored).</summary>
    public static event Action<string> OnProductOwned;

    /// <summary>Raised when prices or store availability change.</summary>
    public static event Action OnStoreUpdated;

    // Static so products registered before this object exists still count.
    private static readonly HashSet<string> registeredProducts = new HashSet<string>();

    private Action onBuySucceeded;
    private Action<string> onBuyFailed;
    private string buyingProductId;

#if CITYBLOXX_REAL_STORE
    private StoreController store;
    private bool connected;
    private readonly HashSet<string> fetchedProducts = new HashSet<string>();
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (Instance != null)
            return;

        GameObject go = new GameObject(nameof(PurchaseService));
        DontDestroyOnLoad(go);
        go.AddComponent<PurchaseService>();
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
#if CITYBLOXX_REAL_STORE
        Connect();
#endif
    }

    // =========================================================
    // PUBLIC API
    // =========================================================

    /// <summary>True when purchases can be made right now.</summary>
    public static bool IsStoreAvailable
    {
        get
        {
#if CITYBLOXX_REAL_STORE
            return Instance != null && Instance.connected;
#elif UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }
    }

    public static bool IsBusy => Instance != null && Instance.buyingProductId != null;

    public static bool IsOwned(string productId)
    {
        return !string.IsNullOrWhiteSpace(productId) &&
               PlayerPrefs.GetInt(OwnedPrefix + productId.Trim(), 0) == 1;
    }

    /// <summary>
    /// Tells the store about non-consumable products so their prices are
    /// fetched and previous purchases restored. Safe to call repeatedly.
    /// </summary>
    public static void RegisterProducts(IEnumerable<string> productIds)
    {
        if (productIds == null)
            return;

        bool added = false;

        foreach (string id in productIds)
        {
            if (!string.IsNullOrWhiteSpace(id) && registeredProducts.Add(id.Trim()))
                added = true;
        }

#if CITYBLOXX_REAL_STORE
        if (added && Instance != null)
            Instance.FetchRegisteredProducts();
#else
        _ = added;
#endif
    }

    /// <summary>
    /// The store's localised price (e.g. "₹89.00"), or the fallback while
    /// the store hasn't answered yet.
    /// </summary>
    public static string GetPriceText(string productId, string fallback)
    {
#if CITYBLOXX_REAL_STORE
        if (Instance != null && Instance.store != null && !string.IsNullOrWhiteSpace(productId))
        {
            Product product = Instance.store.GetProductById(productId.Trim());

            if (product != null && product.metadata != null &&
                !string.IsNullOrEmpty(product.metadata.localizedPriceString))
            {
                return product.metadata.localizedPriceString;
            }
        }
#endif
        return fallback;
    }

    /// <summary>
    /// Starts buying a non-consumable. Exactly one of the callbacks runs.
    /// onFailed gets a message for the player, or null if they cancelled.
    /// </summary>
    public static void Buy(string productId, Action onSucceeded, Action<string> onFailed)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            onFailed?.Invoke("This item isn't for sale.");
            return;
        }

        productId = productId.Trim();

        if (IsOwned(productId))
        {
            onSucceeded?.Invoke();
            return;
        }

        if (Instance == null)
        {
            onFailed?.Invoke("The store isn't available right now.");
            return;
        }

        Instance.StartBuy(productId, onSucceeded, onFailed);
    }

    // =========================================================
    // PURCHASE FLOW
    // =========================================================

    private void StartBuy(string productId, Action onSucceeded, Action<string> onFailed)
    {
        if (buyingProductId != null)
        {
            onFailed?.Invoke("A purchase is already in progress.");
            return;
        }

#if CITYBLOXX_REAL_STORE
        if (!connected || store == null)
        {
            onFailed?.Invoke($"Can't reach {StoreName}. Check your connection and try again.");
            Connect();
            return;
        }

        Product product = store.GetProductById(productId);

        if (product == null || !product.availableToPurchase)
        {
            onFailed?.Invoke("This item isn't available right now. Try again later.");
            FetchRegisteredProducts();
            return;
        }

        buyingProductId = productId;
        onBuySucceeded = onSucceeded;
        onBuyFailed = onFailed;

        store.PurchaseProduct(product);
#elif UNITY_EDITOR
        Grant(productId);
        onSucceeded?.Invoke();
#else
        onFailed?.Invoke("The store isn't available in this version of the game.");
#endif
    }

    private static void Grant(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            return;

        string key = OwnedPrefix + productId.Trim();

        if (PlayerPrefs.GetInt(key, 0) == 1)
            return;

        SaveStore.SetInt(key, 1);
        SaveStore.Save();

        OnProductOwned?.Invoke(productId.Trim());
    }

    private void FinishBuy(string productId, bool succeeded, string failureMessage)
    {
        if (buyingProductId == null || productId != buyingProductId)
            return;

        Action success = onBuySucceeded;
        Action<string> failure = onBuyFailed;

        buyingProductId = null;
        onBuySucceeded = null;
        onBuyFailed = null;

        if (succeeded)
            success?.Invoke();
        else
            failure?.Invoke(failureMessage);
    }

#if CITYBLOXX_REAL_STORE
    // =========================================================
    // UNITY IAP 5
    // =========================================================

    private async void Connect()
    {
        if (connected)
            return;

        try
        {
            // Unity Gaming Services first. Not linking a cloud project only
            // logs a warning; Google Play billing still connects.
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                try
                {
                    await UnityServices.InitializeAsync();
                }
                catch (Exception servicesException)
                {
                    Debug.LogWarning("PurchaseService: Unity Services init failed: " +
                                     servicesException.Message);
                }
            }

            if (store == null)
            {
                store = UnityIAPServices.StoreController();
                store.OnPurchasePending += HandlePurchasePending;
                store.OnPurchaseFailed += HandlePurchaseFailed;
                store.OnPurchaseDeferred += HandlePurchaseDeferred;
                store.OnProductsFetched += HandleProductsFetched;
                store.OnProductsFetchFailed += HandleProductsFetchFailed;
                store.OnPurchasesFetched += HandlePurchasesFetched;
                store.OnStoreDisconnected += HandleStoreDisconnected;
            }

            await store.Connect();
            connected = true;

            FetchRegisteredProducts();
            OnStoreUpdated?.Invoke();
        }
        catch (Exception exception)
        {
            connected = false;
            Debug.LogWarning("PurchaseService: Could not connect to the store: " + exception.Message);
        }
    }

    private void FetchRegisteredProducts()
    {
        if (!connected || store == null)
            return;

        List<ProductDefinition> definitions = new List<ProductDefinition>();

        foreach (string id in registeredProducts)
        {
            if (!fetchedProducts.Contains(id))
                definitions.Add(new ProductDefinition(id, ProductType.NonConsumable));
        }

        if (definitions.Count > 0)
            store.FetchProducts(definitions);
    }

    private void HandleProductsFetched(List<Product> products)
    {
        foreach (Product product in products)
        {
            if (product != null && product.definition != null)
                fetchedProducts.Add(product.definition.id);
        }

        // Restores non-consumables bought earlier (reinstall, new phone).
        store.FetchPurchases();
        OnStoreUpdated?.Invoke();
    }

    private void HandleProductsFetchFailed(ProductFetchFailed failure)
    {
        Debug.LogWarning("PurchaseService: Product fetch failed: " + failure);
    }

    private void HandlePurchasesFetched(Orders orders)
    {
        if (orders == null)
            return;

        foreach (ConfirmedOrder order in orders.ConfirmedOrders)
        {
            foreach (string id in GetProductIds(order))
                Grant(id);
        }

        OnStoreUpdated?.Invoke();
    }

    // Also runs on launch for a purchase that was paid for but not
    // confirmed (app closed mid-purchase), so granting must be idempotent.
    private void HandlePurchasePending(PendingOrder order)
    {
        List<string> ids = GetProductIds(order);

        foreach (string id in ids)
            Grant(id);

        store.ConfirmPurchase(order);

        foreach (string id in ids)
            FinishBuy(id, true, null);
    }

    private void HandlePurchaseFailed(FailedOrder order)
    {
        string reason = order == null ? "Unknown" : order.FailureReason.ToString();
        bool cancelled = reason.IndexOf("Cancel", StringComparison.OrdinalIgnoreCase) >= 0;

        Debug.LogWarning("PurchaseService: Purchase failed: " + reason +
                         (order == null ? "" : " - " + order.Details));

        string message = cancelled ? null : "The purchase didn't go through. You weren't charged.";

        List<string> ids = GetProductIds(order);

        if (ids.Count == 0 && buyingProductId != null)
            ids.Add(buyingProductId);

        foreach (string id in ids)
            FinishBuy(id, false, message);
    }

    // Slow payment methods (e.g. cash at a shop): granted later through
    // HandlePurchasePending once the payment clears.
    private void HandlePurchaseDeferred(DeferredOrder order)
    {
        foreach (string id in GetProductIds(order))
            FinishBuy(id, false, "Payment is pending. Your city unlocks as soon as it clears.");
    }

    private void HandleStoreDisconnected(StoreConnectionFailureDescription description)
    {
        connected = false;
        fetchedProducts.Clear();
        Debug.LogWarning("PurchaseService: Store disconnected: " + description);

        if (buyingProductId != null)
            FinishBuy(buyingProductId, false, $"Lost connection to {StoreName}. Try again.");

        OnStoreUpdated?.Invoke();
    }

    private static List<string> GetProductIds(Order order)
    {
        List<string> ids = new List<string>();

        if (order?.CartOrdered == null)
            return ids;

        foreach (CartItem item in order.CartOrdered.Items())
        {
            string id = item?.Product?.definition?.id;

            if (!string.IsNullOrWhiteSpace(id))
                ids.Add(id);
        }

        return ids;
    }
#endif
}
