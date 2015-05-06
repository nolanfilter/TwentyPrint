using UnityEngine;
using UnityEngine.Cloud.Analytics;
using System.Collections;

public class AnalyticsAgent : MonoBehaviour {

	public enum AnalyticEvent
	{
		PrintFinished = 0,
	}

	private int numPrints = 0;
	private string lifetimePrintsString = "LifetimePrints";

	private const string projectId = "666c8c0f-582f-4219-a39b-e55e36cd4702";

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

		UnityAnalytics.StartSDK (projectId);
	}

	public static void LogAnalyticEvent( AnalyticEvent analyticEvent )
	{
		if( instance )
			instance.internalLogAnalyticEvent( analyticEvent );
	}

	private void internalLogAnalyticEvent( AnalyticEvent analyticEvent )
	{
		if( analyticEvent == AnalyticEvent.PrintFinished )
			numPrints++;
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
			UnityAnalytics.Transaction( response.productIdentifier, decimal.Parse( product.localizedPrice ), product.currencyCode, response.receipt, null );
	}

	public static int GetNumPrints()
	{
		if( instance )
			return instance.numPrints;

		return 0;
	}
}
