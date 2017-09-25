using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RatingAgent : MonoBehaviour {

	public GameObject popUpRoot;

	public Text messageText;
	public Text positiveText;
	public Text negativeText;

	public TouchDownCallback[] negativeCallbacks;
	public TouchDownCallback positiveCallback;

	private bool hasSaidWouldRate = false;
	private string hasSaidWouldRateString = "hasSaidWouldRate";

	private static RatingAgent mInstance;
	public static RatingAgent instance
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
			Debug.LogError( "Only one instance of RatingAgent allowed. Destroying " + gameObject + " and leaving " + mInstance.gameObject );
			Destroy( gameObject );
			return;
		}
		
		mInstance = this;

		SetPopUpEnabled( false );
		SetPopUpText( "Love 20print?\nLeave us a review!", "Yes!", "Not yet." );

		if( !PlayerPrefs.HasKey( hasSaidWouldRateString ) )
			PlayerPrefs.SetInt( hasSaidWouldRateString, 0 );

		hasSaidWouldRate = ( PlayerPrefs.GetInt( hasSaidWouldRateString ) == 1 );
	}

	void OnEnable()
	{
		for( int i = 0; i < negativeCallbacks.Length; i++ )
			negativeCallbacks[i].OnAreaTouch += OnNegativeAreaTouch;

		if( positiveCallback )
			positiveCallback.OnAreaTouch += OnPositiveAreaTouch;
	}
	
	void OnDisable()
	{
		for( int i = 0; i < negativeCallbacks.Length; i++ )
			negativeCallbacks[i].OnAreaTouch -= OnNegativeAreaTouch;
		
		if( positiveCallback )
			positiveCallback.OnAreaTouch -= OnPositiveAreaTouch;
	}

	private void OnNegativeAreaTouch()
	{
		StartCoroutine( "DoDisableDelay" );
	}

	private void OnPositiveAreaTouch()
	{
		StartCoroutine( "DoDisableDelay" );

//		IOSNativeUtility.RedirectToAppStoreRatingPage();

		hasSaidWouldRate = true;
		PlayerPrefs.SetInt( hasSaidWouldRateString, 1 );
	}

	public static void SetPopUpEnabled( bool enabled )
	{
		if( instance )
			instance.internalSetPopUpEnabled( enabled );
	}

	private void internalSetPopUpEnabled( bool enabled )
	{
		if( popUpRoot )
			popUpRoot.SetActive( enabled );
	}

	public static bool GetPopUpEnabled()
	{
		if( instance )
			return instance.internalGetPopUpEnabled();

		return false;
	}

	private bool internalGetPopUpEnabled()
	{
		if( popUpRoot )
			return popUpRoot.activeInHierarchy;

		return false;
	}

	public static void SetPopUpText( string messageString, string positiveString, string negativeString )
	{
		if( instance )
			instance.internalSetPopUpText( messageString, positiveString, negativeString );
	}

	private void internalSetPopUpText( string messageString, string positiveString, string negativeString )
	{
		if( messageText )
			messageText.text = messageString;

		if( positiveText )
			positiveText.text = positiveString;

		if( negativeText )
			negativeText.text = negativeString;
	}

	public static void CheckForPrompt()
	{
		if( instance )
			instance.internalCheckForPrompt();
	}

	private void internalCheckForPrompt()
	{
		if( hasSaidWouldRate )
			return;

		int numPrints = AnalyticsAgent.GetNumPrints();

		if( numPrints == 10 || numPrints == 25 || numPrints%100 == 50 )
			SetPopUpEnabled( true );
	}

	private IEnumerator DoDisableDelay()
	{
		yield return null;

		SetPopUpEnabled( false );
	}
}
