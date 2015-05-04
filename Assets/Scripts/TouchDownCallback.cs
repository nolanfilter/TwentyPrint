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
		FingerGestures.OnFingerUp += OnTouchUp;
	}
	
	void OnDestroy()
	{
		FingerGestures.OnFingerDown -= OnTouchDown;
		FingerGestures.OnFingerUp -= OnTouchUp;
	}

	private void OnTouchDown( int fingerIndex, Vector2 fingerPos )
	{

	}

	private void OnTouchUp( int fingerIndex, Vector2 fingerPos, float timeHeldDown )
	{
		if( GameAgent.GetWasDragging() )
			return;

		if( RectTransformUtility.RectangleContainsScreenPoint( rectTransform, fingerPos, null ) )
		{
			if( OnAreaTouch != null )
				OnAreaTouch();
			
			if( OnAreaTouchWithCallback != null )
				OnAreaTouchWithCallback( this );
		}
	}
}
