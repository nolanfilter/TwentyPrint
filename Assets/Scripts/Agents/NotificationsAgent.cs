using UnityEngine;
using System.Collections;

public class NotificationsAgent : MonoBehaviour {

	private static NotificationsAgent mInstance;
	public static NotificationsAgent instance
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
			Debug.LogError( "Only one instance of NotificationsAgent allowed. Destroying " + gameObject + " and leaving " + mInstance.gameObject );
			Destroy( gameObject );
			return;
		}
		
		mInstance = this;
	}
}
