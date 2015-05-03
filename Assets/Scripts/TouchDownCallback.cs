using UnityEngine;
using System.Collections;

[RequireComponent( typeof( RectTransform ) )]
public class TouchDownCallback : MonoBehaviour {
	
	public delegate void AreaTouch();
	public event AreaTouch OnAreaTouch;

	public delegate void AreaTouchWithCallback( TouchDownCallback callback );
	public event AreaTouchWithCallback OnAreaTouchWithCallback;
	
	private RectTransform rectTransform;
	
	void Awake()
	{
		rectTransform = GetComponent<RectTransform>() as RectTransform;
		
		FingerGestures.OnFingerDown += OnTouchDown;
	}
	
	void OnDestroy()
	{
		FingerGestures.OnFingerDown -= OnTouchDown;
	}
	
	private void OnTouchDown( int fingerIndex, Vector2 fingerPos )
	{
		if( RectTransformUtility.RectangleContainsScreenPoint( rectTransform, fingerPos, null ) )
		{

			if( OnAreaTouch != null )
				OnAreaTouch();

			if( OnAreaTouchWithCallback != null )
				OnAreaTouchWithCallback( this );
		}
	}
}
