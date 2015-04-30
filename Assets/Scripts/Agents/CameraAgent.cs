using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CameraAgent : MonoBehaviour {
	
	public static GameObject MainCameraObject
	{
		get
		{
			return Camera.main.gameObject;
		}
	}

	private float fadeDuration = 0.2f;

	private Camera camera;

	private List<Image> backgroundImages;

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

		camera = MainCameraObject.GetComponent<Camera>();
	
		camera.orthographic = true;
		camera.orthographicSize = Screen.height * 0.5f;
		MainCameraObject.transform.root.position = new Vector3( Screen.width * 0.5f, Screen.height * 0.5f, -10f );

		backgroundImages = new List<Image>();
	}

	public static void RegisterBackgroundImage( Image image )
	{
		if( instance )
			instance.internalRegisterBackgroundImage( image );
	}

	private void internalRegisterBackgroundImage( Image image )
	{
		if( !backgroundImages.Contains( image ) )
			backgroundImages.Add( image );
	}

	public static void UnregisterBackgroundImage( Image image )
	{
		if( instance )
			instance.internalUnregisterBackgroundImage( image );
	}

	private void internalUnregisterBackgroundImage( Image image )
	{
		if( backgroundImages.Contains( image ) )
			backgroundImages.Remove( image );
	}

	public static void SetBackgroundColor( Color newColor )
	{
		if( instance )
			instance.internalSetBackgroundColor( newColor );
	}

	private void internalSetBackgroundColor( Color newColor )
	{
		StartCoroutine( "DoBackgroundFade", newColor );
	}

	private IEnumerator DoBackgroundFade( Color toColor )
	{
		if( camera.backgroundColor == toColor )
			yield break;

		Color fromColor = camera.backgroundColor;
		float currentTime = 0f;
		float lerp;
		Color color;

		do
		{
			currentTime += Time.deltaTime;
			lerp = Mathf.Clamp01( currentTime / fadeDuration );
			
			lerp = 3f * Mathf.Pow( lerp, 2f ) - 2f * Mathf.Pow( lerp, 3f );

			color = Color.Lerp( fromColor, toColor, lerp );

			camera.backgroundColor = color;

			for( int i = 0; i < backgroundImages.Count; i++ )
				backgroundImages[i].color = color;
			
			yield return null;
			
		} while( currentTime < fadeDuration );
		
		camera.backgroundColor = toColor;

		for( int i = 0; i < backgroundImages.Count; i++ )
			backgroundImages[i].color = toColor;
    }
}
