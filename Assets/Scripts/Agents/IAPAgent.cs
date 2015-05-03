using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IAPAgent : MonoBehaviour {

	private const string removeAdsProductString = "2001";

	public string purchaseText = "Remove Ads & Unlock All";

	public Text[] buttonTexts;

	private bool paidToRemoveAds;
	private string paidToRemoveAdsString = "paidToRemoveAds";

	private static IAPAgent mInstance;
	public static IAPAgent instance
	{
		get
		{
			return mInstance;
		}
	}
	
	void Awake()
	{
		if( mInstance != null )
		{
			Debug.LogError( "Only one instance of IAPAgent allowed. Destroying " + gameObject + " and leaving " + mInstance.gameObject );
			Destroy( gameObject );
			return;
		}
		
		mInstance = this;

		if( !PlayerPrefs.HasKey( paidToRemoveAdsString ) )
			PlayerPrefs.SetInt( paidToRemoveAdsString, 0 );

		paidToRemoveAds = ( PlayerPrefs.GetInt( paidToRemoveAdsString ) == 1 );

		for( int i = 0; i < buttonTexts.Length; i++ )
		{
			if( !paidToRemoveAds )
				buttonTexts[i].text = purchaseText;
			else
				buttonTexts[i].gameObject.SetActive( false );
		}

		IOSInAppPurchaseManager.instance.OnStoreKitInitComplete += OnStoreKitInitComplete;
		IOSInAppPurchaseManager.instance.OnTransactionComplete += OnTransactionComplete;
		IOSInAppPurchaseManager.instance.OnRestoreComplete += OnRestoreComplete;

		IOSInAppPurchaseManager.instance.loadStore();

		IOSInAppPurchaseManager.instance.addProductId( removeAdsProductString );
	}

	public static void PurchaseRemoveAds()
	{
		if( IOSInAppPurchaseManager.instance.IsStoreLoaded )
			IOSInAppPurchaseManager.instance.buyProduct( removeAdsProductString );
	}

	public static void RestorePurchases()
	{
		IOSInAppPurchaseManager.instance.restorePurchases();
	}

	public static bool GetPaidToRemoveAds()
	{
		if( instance )
			return instance.paidToRemoveAds;

		return false;
	}

	private static void OnStoreKitInitComplete( ISN_Result result )
	{
		IOSInAppPurchaseManager.instance.OnStoreKitInitComplete -= OnStoreKitInitComplete;
		
		if( result.IsSucceeded )
		{
			//Debug.Log("Inited successfully, Avaliable products cound: " + IOSInAppPurchaseManager.instance.products.Count.ToString());
			
			IOSProductTemplate product = IOSInAppPurchaseManager.instance.GetProductById( removeAdsProductString );

			if( product != null )
			{
				if( instance )
					instance.SetPurchaseText( "" + product.currencySymbol + product.localizedPrice );
			}
		}
		else
		{
			//Debug.Log("StoreKit Init Failed.  Error code: " + result.error.code + "\n" + "Error description:" + result.error.description);
		}
	}

	private static void OnTransactionComplete( IOSStoreKitResponse response )
	{
		//Debug.Log("OnTransactionComplete: " + response.productIdentifier);
		//Debug.Log("OnTransactionComplete: state: " + response.state);
		
		switch( response.state ) 
		{
			case InAppPurchaseState.Purchased: case InAppPurchaseState.Restored:
			{
				if( response.productIdentifier == removeAdsProductString )
				{
					if( instance )
						instance.RemoveAds();
				}
			} break;

			case InAppPurchaseState.Deferred:
			{
				//iOS 8 introduces Ask to Buy, which lets 
				//parents approve any purchases initiated by children
				//You should update your UI to reflect this 
				//deferred state, and expect another Transaction 
				//Complete  to be called again with a new transaction state 
				//reflecting the parent's decision or after the 
				//transaction times out. Avoid blocking your UI 
				//or gameplay while waiting for the transaction to be updated.
			} break;
		
			case InAppPurchaseState.Failed:
			{
				//Our purchase flow is failed.
				//We can unlock interface and report user that the purchase is failed. 
			  	//Debug.Log("Transaction failed with error, code: " + response.error.code);
			   	//Debug.Log("Transaction failed with error, description: " + response.error.description);
			} break;
		}
		
		//IOSNativePopUpManager.showMessage("Store Kit Response", "product " + responce.productIdentifier + " state: " + responce.state.ToString());
	}

	private static void OnRestoreComplete( ISN_Result res )
	{
		if( res.IsSucceeded )
		{
			//IOSNativePopUpManager.showMessage("Success", "Restore Compleated");
		}
		else
		{
			//IOSNativePopUpManager.showMessage("Error: " + res.error.code, res.error.description);
		}
	}	

	private void SetPurchaseText( string priceString )
	{
		if( !string.IsNullOrEmpty( priceString ) )
		{
			for( int i = 0; i < buttonTexts.Length; i++ )
				buttonTexts[i].text = purchaseText + " - " + priceString;
		}
	}

	private void RemoveAds()
	{
		SpriteAgent.UnlockAllSprites();

		paidToRemoveAds = true;
		PlayerPrefs.SetInt( paidToRemoveAdsString, 1 );

		for( int i = 0; i < buttonTexts.Length; i++ )
			buttonTexts[i].gameObject.SetActive( false );
	}
}
