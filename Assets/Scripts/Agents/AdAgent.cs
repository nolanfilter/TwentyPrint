using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;

public class AdAgent : MonoBehaviour {
	
	private const string iosInterstitialVideoZoneId = "329741430525809";
	private const string iosIncentivizedZoneId = "329741430525828";
	private string interstitialVideoZoneId;
	private string incentivizedZoneId;

	private const string iosGameID = "32974";
	private const string androidGameID = "33171";
	private string gameID;

	private bool hasIAd = false;

	private static AdAgent mInstance;
	public static AdAgent instance
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
			Debug.LogError( "Only one instance of AdAgent allowed. Destroying " + gameObject + " and leaving " + mInstance.gameObject );
			Destroy( gameObject );
			return;
		}
		
		mInstance = this;
	}

	void Start()
	{
#if UNITY_IOS
		interstitialVideoZoneId = iosInterstitialVideoZoneId;
		incentivizedZoneId = iosIncentivizedZoneId;

		gameID = iosGameID;
#elif UNITY_ANDROID
		gameID = androidGameID;
#endif
		
		if( Advertisement.isSupported )
		{
			Advertisement.Initialize( gameID, true );
			Advertisement.allowPrecache = true;
		}

		iAdBannerController.instance.addEventListener( iAdEvent.INTERSTITIAL_AD_DID_LOAD, OnInterstitialLoaded );

		iAdBannerController.instance.LoadInterstitialAd();
	}

	public static bool GetIsShowing()
	{
		return Advertisement.isShowing;
	}

	public static void ShowInterstitialImage()
	{
		if( instance )
			instance.internalShowInterstitialImage();
	}

	private void internalShowInterstitialImage()
	{
		if( hasIAd )
		{
			iAdBannerController.instance.ShowInterstitialAd();

			StartCoroutine( "DoPostInterstitialImage" );
		}
		else
		{
			GameAgent.ChangeState( GameAgent.State.Ready );
		}
	}

	public static void ShowInterstitialVideo()
	{
		if( instance )
			instance.internalShowInterstitialVideo();
	}

	private void internalShowInterstitialVideo()
	{
		if( Advertisement.isReady( interstitialVideoZoneId ) )
		{
			Advertisement.Show( interstitialVideoZoneId, new ShowOptions {
				pause = true,
				resultCallback = result => {
					switch( result )
					{
						case ShowResult.Finished: GameAgent.ChangeState( GameAgent.GetAfterAdState() ); break;
						case ShowResult.Failed: GameAgent.ChangeState( GameAgent.GetAfterAdState() ); break;
						case ShowResult.Skipped: GameAgent.ChangeState( GameAgent.GetAfterAdState() ); break;
						//case ShowResult.Skipped: GameAgent.ChangeState( GameAgent.State.Finished ); break;
					}
				}
			} );
		}
		else
		{
			GameAgent.ChangeState( GameAgent.GetAfterAdState() );
		}
	}

	private void OnInterstitialLoaded()
	{
		hasIAd = true;
	}
	
	private IEnumerator DoPostInterstitialImage()
	{
		hasIAd = false;

		yield return new WaitForSeconds( 0.3f );

		GameAgent.ChangeState( GameAgent.State.Ready );

		while( GameAgent.GetCurrentState() == GameAgent.State.Ready )
			yield return null;

		iAdBannerController.instance.LoadInterstitialAd();
	}
}
