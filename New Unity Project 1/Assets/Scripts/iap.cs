using System;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Advertisements;
using UnityEngine.UI;


// Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
public class iap : MonoBehaviour, IStoreListener
{
    // The Unity Purchasing system.
    private static IStoreController m_StoreController;
    // The store-specific Purchasing subsystems.         
    private static IExtensionProvider m_StoreExtensionProvider; 

        
    //

    public static string kProductIDConsumable = "consumable";
    //public static string kproductidconsumable2 = "consumable";
    public static string kProductIDNonConsumable = "nonconsumable";
    public static string kProductIDSubscription = "subscription";

    // Apple App Store-specific product identifier for the products (get it from the developer account).

    private static string kProductNameAppleConsumable = "com.unity3d.test.services.purchasing.consumable";
    private static string kProductNameAppleNonConsumable = "com.unity3d.test.services.purchasing.nonconsumable";
    private static string kProductNameAppleSubscription = "com.unity3d.subscription.new";

    // Google Play Store-specific product identifier for the products (get it from the developer account).

    private static string kProductNameGooglePlayConsumable = "stuff";
    private static string kProductNameGooglePlayNonConsumable = "com.unity3d.test.services.purchasing.nonconsumable";
    private static string kProductNameGooglePlaySubscription = "com.unity3d.subscription.original";

    //Windows Store-specific product identifier for the products (get it from the developer account).

    private static string kProductNameWindowsConsumable = "com.unity3d.test.services.purchasing.nonconsumable";
    private static string kProductNameWindowsNonConsumable = "com.unity3d.test.services.purchasing.nonconsumable";
    private static string kProductNameWindowsSubscription = "com.unity3d.subscription.original";

    //Amazon Store-specific product identifier for the products (get it from the developer account). 

    private static string kProductNameAmazonConsumable = "com.unity3d.test.services.purchasing.nonconsumable";
    private static string kProductNameAmazonNonConsumable = "com.unity3d.test.services.purchasing.nonconsumable";
    private static string kProductNameAmazonSubscription = "com.unity3d.subscription.original";



    //UI variables
    public Text stuffText = null;



    void Start()
    {
        // If we haven't set up the Unity Purchasing reference
        if (m_StoreController == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();
        }
    }

    public void InitializePurchasing()
    {
        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            // ... we are done here.
            return;
        }

        // Creates a builder, first passing in a suite of Unity provided stores.
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Add a product to sell / restore by way of its identifier, associating the general identifier with its store-specific identifiers.
        builder.AddProduct(kProductIDConsumable, ProductType.Consumable, new IDs() {
            { kProductNameAppleConsumable, AppleAppStore.Name },
            { kProductNameGooglePlayConsumable, GooglePlay.Name },
            { kProductNameWindowsConsumable, WindowsStore.Name},
            { kProductNameAmazonConsumable, AmazonApps.Name },
        });

        builder.AddProduct(kProductIDNonConsumable, ProductType.NonConsumable, new IDs() {
            { kProductNameAppleNonConsumable, AppleAppStore.Name },
            { kProductNameGooglePlayNonConsumable, GooglePlay.Name },
            { kProductNameWindowsNonConsumable, WindowsStore.Name },
            { kProductNameAmazonNonConsumable, AmazonApps.Name},
        });

        builder.AddProduct(kProductIDSubscription, ProductType.Subscription, new IDs() {
            { kProductNameAppleSubscription, AppleAppStore.Name },
            { kProductNameGooglePlaySubscription, GooglePlay.Name },
            { kProductNameWindowsSubscription, WindowsStore.Name },
            { kProductNameAmazonSubscription, AmazonApps.Name},
        });

        UnityPurchasing.Initialize(this, builder);
    }


    private bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }


    public void BuyConsumable()
    {
        // Buy the consumable product using its general identifier. Expect a response either 
        // through ProcessPurchase or OnPurchaseFailed asynchronously.
        BuyProductID(kProductIDConsumable);
    }


    public void BuyNonConsumable()
    {
        // Buy the non-consumable product using its general identifier. Expect a response either 
        // through ProcessPurchase or OnPurchaseFailed asynchronously.
        BuyProductID(kProductIDNonConsumable);
    }


    public void BuySubscription()
    {
        // Buy the subscription product using its the general identifier. Expect a response either 
        // through ProcessPurchase or OnPurchaseFailed asynchronously.
        // Notice how we use the general product identifier in spite of this ID being mapped to
        // custom store-specific identifiers above.
        BuyProductID(kProductIDSubscription);
    }

    void BuyProductID(string productId)
    {
        // If Purchasing has been initialized
        if (IsInitialized())
        {
            //look up the Product reference with the general product identifier and the Purchasing system's products collection.
            Product product = m_StoreController.products.WithID(productId);

            // If the look up found a product for this device's store and that product is ready to be sold
            if (product != null && product.availableToPurchase)
            {
                // buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));

                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                //report the product look-up failure situation  
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        else
        {
            //report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or retrying initiailization.
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }


    // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
    // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
    public void RestorePurchases()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            //begin restoring purchases
            Debug.Log("RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
            // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) => {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }


    //  
    // --- IStoreListener
    //

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.Log("OnInitialized: PASS");

        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;
    }


    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        // A consumable product has been purchased by this user.
        if (String.Equals(args.purchasedProduct.definition.id, kProductIDConsumable, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));// The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
            //ScoreManager.score += 100;
            stuffText.text = "You got Stuff";
        }
        // Or ... a non-consumable product has been purchased by this user.
        else if (String.Equals(args.purchasedProduct.definition.id, kProductIDNonConsumable, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));// TODO: The non-consumable item has been successfully purchased, grant this item to the player.
        }
        // Or ... a subscription product has been purchased by this user.
        else if (String.Equals(args.purchasedProduct.definition.id, kProductIDSubscription, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));// TODO: The subscription item has been successfully purchased, grant this to the player.
        }
        // Or ... an unknown product has been purchased by this user. Fill in additional products here....
        else
        {
            Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
        }

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed. 
        return PurchaseProcessingResult.Complete;
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
        // this reason with the user to guide their troubleshooting actions.
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }



    //Checks if ad is ready to be played
    public void ShowRewardedAd()
    {
        if (Advertisement.IsReady("rewardedVideo"))
        {
            var options = new ShowOptions { resultCallback = HandleShowResult };
            Advertisement.Show("rewardedVideo", options);
        }
    }
    //Plays Ad
    private void HandleShowResult(ShowResult result)
    {
        //Chooses what will happen based on how the user interacted with the ad
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                //Add currency here.
                stuffText.text = "You got Stuff from ad";

                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                break;
        }
    }
}
