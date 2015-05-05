using UnityEngine;
using System.Collections;

public class PreferencesAgent : MonoBehaviour {

	private static PreferencesAgent mInstance;
	public static PreferencesAgent instance
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
			Debug.LogError( "Only one instance of PreferencesAgent allowed. Destroying " + gameObject + " and leaving " + mInstance.gameObject );
			Destroy( gameObject );
			return;
		}
		
		mInstance = this;

		iCloudManager.instance.OnCloundInitAction += OnCloundInitAction;
		iCloudManager.instance.OnCloundDataChangedAction += OnCloundDataChangedAction;
		iCloudManager.instance.OnCloudDataReceivedAction += OnCloudDataReceivedAction;
		
		iCloudManager.instance.init();
	}

	void OnDestroy()
	{
		if( iCloudManager.HasInstance )
		{
			iCloudManager.instance.OnCloundInitAction -= OnCloundInitAction;
			iCloudManager.instance.OnCloundDataChangedAction -= OnCloundDataChangedAction;
			iCloudManager.instance.OnCloudDataReceivedAction -= OnCloudDataReceivedAction;
		}
	}

	private void OnCloundInitAction( ISN_Result result )
	{
		if( result.IsSucceeded )
		{
			CompareICloudToPlayerPrefs();
		}

		/*
		if( result.IsSucceeded ) {
			IOSNativePopUpManager.showMessage("iCloud", "Initialization Success!");
		} else {
			IOSNativePopUpManager.showMessage("iCloud", "Initialization Failed!");
		}
		*/

	}

	private void OnCloudDataReceivedAction( iCloudData data )
	{
		/*
		if( data.IsEmpty ) {
			IOSNativePopUpManager.showMessage(data.key, "data is empty");
		} else {
			IOSNativePopUpManager.showMessage(data.key, data.stringValue);
		}
		*/
	}	

	private void OnCloundDataChangedAction()
	{
		CompareICloudToPlayerPrefs();

		//IOSNativePopUpManager.showMessage("iCloud", "Cloud Data Was Changed On Other Device");
	}

	public static void WriteToICloud( string key, int value )
	{

	}

	private void CompareICloudToPlayerPrefs()
	{

	}
}
