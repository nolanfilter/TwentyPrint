using UnityEngine;
using System.Collections;

[RequireComponent( typeof( RectTransform ) )]
public class TouchDownCallback : MonoBehaviour {
	
	public delegate void AreaTouch();
	public event AreaTouch OnAreaTouch;

	public delegate void AreaTouchWithCallback( TouchDownCallback callback );
	public event AreaTouchWithCallback OnAreaTouchWithCallback;

	public ColorController colorController;

	public bool ignoreOnPopUp = false;

	private RectTransform rectTransform;
	
	void Awake()
	{
		rectTransform = GetComponent<RectTransform>() as RectTransform;

		if( colorController )
		{
			FingerGestures.OnFingerDown += OnTouchDown;
			FingerGestures.OnFingerMove += OnTouchMove;
		}

		FingerGestures.OnFingerUp += OnTouchUp;
	}

	void OnDestroy()
	{
		if( colorController )
		{
			FingerGestures.OnFingerDown -= OnTouchDown;
			FingerGestures.OnFingerMove -= OnTouchMove;
		}

		FingerGestures.OnFingerUp -= OnTouchUp;
	}

	private void OnTouchDown( int fingerIndex, Vector2 fingerPos )
	{
		if( !gameObject.activeInHierarchy || ( ignoreOnPopUp && RatingAgent.GetPopUpEnabled() ) )
			return;

		if( RectTransformUtility.RectangleContainsScreenPoint( rectTransform, fingerPos, null ) && !GameAgent.GetWasHolding() )
			colorController.SetColor( ColorAgent.GetCurrentColorPack().midColor );
	}

	private void OnTouchMove( int fingerIndex, Vector2 fingerPos )
	{
		if( !gameObject.activeInHierarchy || ( ignoreOnPopUp && RatingAgent.GetPopUpEnabled() ) )
			return;

		if( RectTransformUtility.RectangleContainsScreenPoint( rectTransform, fingerPos, null ) && !GameAgent.GetWasHolding() )
			colorController.SetColor( ColorAgent.GetCurrentColorPack().midColor );
		else
			colorController.SetColor( ColorAgent.GetCurrentColorPack().TypeToColor( colorController.colorType ) );
	}

	private void OnTouchUp( int fingerIndex, Vector2 fingerPos, float timeHeldDown )
	{
		if( !gameObject.activeInHierarchy || ( ignoreOnPopUp && RatingAgent.GetPopUpEnabled() ) )
			return;

		if( colorController )
			colorController.SetColor( ColorAgent.GetCurrentColorPack().TypeToColor( colorController.colorType ) );

		if( GameAgent.GetWasHolding() )
			return;

		if( RectTransformUtility.RectangleContainsScreenPoint( rectTransform, fingerPos, null ) )
		{
			if( OnAreaTouch != null )
				OnAreaTouch();
			
			if( OnAreaTouchWithCallback != null )
				OnAreaTouchWithCallback( this );

			AudioAgent.PlaySoundEffect( AudioAgent.SoundEffectType.ButtonTap );
		}
	}
}
