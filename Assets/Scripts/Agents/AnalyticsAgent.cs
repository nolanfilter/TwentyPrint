using UnityEngine;
using UnityEngine.Analytics;
using System.Collections;
using System.Collections.Generic;

public class AnalyticsAgent : MonoBehaviour {

	public enum AnalyticEvent
	{
		PrintFinished = 0,
		UnlockSprite = 1,
		Share = 2,
		StoreScreen = 3,
		SettingsScreen = 4,
		More = 5,
	}

	private int numPrints = 0;
	private string lifetimePrintsString = "LifetimePrints";
	
	private static AnalyticsAgent mInstance;
	public static AnalyticsAgent instance
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
			Debug.LogError( "Only one instance of AnalyticsAgent allowed. Destroying " + gameObject + " and leaving " + mInstance.gameObject );
			Destroy( gameObject );
			return;
		}
		
		mInstance = this;

		if( PlayerPrefs.HasKey( lifetimePrintsString ) )
			numPrints = PlayerPrefs.GetInt( lifetimePrintsString );
	}

	public static void LogAnalyticEvent( AnalyticEvent analyticEvent )
	{
		if( instance )
			instance.internalLogAnalyticEvent( analyticEvent );
	}

	private void internalLogAnalyticEvent( AnalyticEvent analyticEvent )
	{
		switch( analyticEvent )
		{
			case AnalyticEvent.PrintFinished:
			{
				numPrints++;
				PlayerPrefs.SetInt( lifetimePrintsString, numPrints );

				List<string> spritesUsed = SpriteAgent.GetSpritesUsed();

				Dictionary<string, object> spritesUsedDictionary = new Dictionary<string, object>();

				int index = 0;

				while( index < spritesUsed.Count && index < 10 )
				{
					spritesUsedDictionary.Add( "sprite" + index, spritesUsed[ spritesUsed.Count - 1 - index ] );
					index++;
				}

				Analytics.CustomEvent( "printFinished", spritesUsedDictionary );
			} break;

			case AnalyticEvent.UnlockSprite:
			{
				Analytics.CustomEvent( "unlockSprite", new Dictionary<string, object>
				{
					{ "unlockedSprite", SpriteAgent.GetCurrentSprite().name },
				});
			} break;

			case AnalyticEvent.Share:
			{
				Analytics.CustomEvent( "share", new Dictionary<string, object>{} );
			} break;

			case AnalyticEvent.StoreScreen:
			{
				Analytics.CustomEvent( "storeScreen", new Dictionary<string, object>{} );
			} break;

			case AnalyticEvent.SettingsScreen:
			{
				Analytics.CustomEvent( "settingsScreen", new Dictionary<string, object>{} );
			} break;

			case AnalyticEvent.More:
			{
				Analytics.CustomEvent( "more", new Dictionary<string, object>{} );
			} break;
		}
	}

	public static void LogTranscation( IOSStoreKitResponse response )
	{
		if( instance )
			instance.internalLogTranscation( response );
	}

	private void internalLogTranscation( IOSStoreKitResponse response )
	{
		IOSProductTemplate product = IAPAgent.GetProductById( response.productIdentifier );

		if( product != null )
			Analytics.Transaction( response.productIdentifier, decimal.Parse( product.localizedPrice ), product.currencyCode, response.receipt, null );
	}

	public static int GetNumPrints()
	{
		if( instance )
			return instance.numPrints;

		return 0;
	}
}
