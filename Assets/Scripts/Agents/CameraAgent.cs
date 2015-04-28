using UnityEngine;
using System.Collections;

public class CameraAgent : MonoBehaviour {
	
	public static GameObject MainCameraObject
	{
		get
		{
			return Camera.main.gameObject;
		}
	}
	
	private static CameraAgent mInstance;
	public static CameraAgent instance
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
			Debug.LogError( "Only one instance of CameraAgent allowed. Destroying " + gameObject + " and leaving " + mInstance.gameObject );
			Destroy( gameObject );
			return;
		}
		
		mInstance = this;
		
		MainCameraObject.GetComponent<Camera>().orthographic = true;
		MainCameraObject.GetComponent<Camera>().orthographicSize = Screen.height * 0.5f;
		MainCameraObject.transform.root.position = new Vector3( Screen.width * 0.5f, Screen.height * 0.5f, -10f );
	}
}
