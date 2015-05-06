using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class PreferencesAgent : MonoBehaviour {

	private const string spriteStatusString = "SpriteStatus";

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
		/*
		if(result.IsSucceeded) {
			IOSNativePopUpManager.showMessage("iCloud", "Initialization Success!");
		} else {
			IOSNativePopUpManager.showMessage("iCloud", "Initialization Failed!");
		}
		*/

		if( result.IsSucceeded )
			iCloudManager.instance.requestDataForKey( spriteStatusString );
	}

	private void OnCloundDataChangedAction()
	{
		iCloudManager.instance.requestDataForKey( spriteStatusString );
	}

	private void OnCloudDataReceivedAction( iCloudData data )
	{
		/*
		if(data.IsEmpty) {
			IOSNativePopUpManager.showMessage(data.key, "data is empty");
		} else {
			IOSNativePopUpManager.showMessage(data.key, data.stringValue);
		}
		*/

		if( !data.IsEmpty )
			CompareICloudToPlayerPrefs( data.stringValue );
		else
			UpdateICloud();
	}	

	public static void UpdateICloud()
	{
		//IOSNativePopUpManager.showMessage(spriteStatusString, "setting value");

		iCloudManager.instance.setString( spriteStatusString, DictionaryToString( SpriteAgent.GetSpritesUnlockedDictionary() ) );
	}

	private void CompareICloudToPlayerPrefs( string statusString )
	{
		Dictionary<string, bool> localDictionary = SpriteAgent.GetSpritesUnlockedDictionary();
		Dictionary<string, bool> cloudDictionary = StringToDictionary( statusString );

		bool isSame = false;

		foreach( KeyValuePair<string, bool> kvp in localDictionary )
		{
			if( cloudDictionary.ContainsKey( kvp.Key ) )
			{
				if( cloudDictionary[ kvp.Key ] && !kvp.Value )
					SpriteAgent.UnlockSpriteByName( kvp.Key );

				if( !cloudDictionary[ kvp.Key ] && kvp.Value )
					isSame = false;
			}
			else
			{
				isSame = false;
			}
		}

		if( !isSame )
			UpdateICloud();
	}

	private static string DictionaryToString( Dictionary<string, bool> dictionary )
	{
		StringBuilder stringBuilder = new StringBuilder();

		foreach( KeyValuePair<string, bool> kvp in dictionary )
			stringBuilder.AppendFormat( "{0},{1},", kvp.Key, ( kvp.Value ? "1" : "0" ) );

		string statusString = stringBuilder.ToString();

		return statusString.Substring( 0, statusString.Length - 1 );
	}

	private static Dictionary<string, bool> StringToDictionary( string statusString )
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();

		string[] components = statusString.Split( ',' );

		for( int i = 0; i < components.Length; i += 2 )
			dictionary.Add( components[i], ( components[ i + 1 ] == "1" ) );

		return dictionary;
	}
}
