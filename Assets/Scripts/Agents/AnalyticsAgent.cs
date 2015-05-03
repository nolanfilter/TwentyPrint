using UnityEngine;
using System.Collections;

public class AnalyticsAgent : MonoBehaviour {

	public enum AnalyticEvent
	{
		PrintFinished = 0,
	}

	private int numPrints = 0;

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

	public static int GetNumPrints()
	{
		if( instance )
			return instance.numPrints;

		return 0;
	}
}
