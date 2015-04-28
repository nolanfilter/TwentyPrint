using UnityEngine;
using System.Collections;

[RequireComponent( typeof( RectTransform ) )]
public class TouchDownCallback : MonoBehaviour {
	
	public delegate void AreaTouch();
	public event AreaTouch OnAreaTouch;
	
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
		if( OnAreaTouch != null && RectTransformUtility.RectangleContainsScreenPoint( rectTransform, fingerPos, null ) )
			OnAreaTouch();
	}
}
