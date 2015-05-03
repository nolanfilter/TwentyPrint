using UnityEngine;
using System.Collections;

public class VersionAgent : MonoBehaviour {

	public float version;
	public bool deletePlayerPrefs = false;

	private string versionString = "version";

	private static VersionAgent mInstance;
	public static VersionAgent instance
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
			Debug.LogError( "Only one instance of VersionAgent allowed. Destroying " + gameObject + " and leaving " + mInstance.gameObject );
			Destroy( gameObject );
			return;
		}
		
		mInstance = this;

		if( PlayerPrefs.HasKey( versionString ) )
		{
			float foundVersion = PlayerPrefs.GetFloat( versionString );

			if( foundVersion < version )
				deletePlayerPrefs = true;
		}

		if( deletePlayerPrefs )
			PlayerPrefs.DeleteAll();

		PlayerPrefs.SetFloat( versionString, version );
	}
}
