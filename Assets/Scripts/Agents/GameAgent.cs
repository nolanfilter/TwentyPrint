using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameAgent : MonoBehaviour {
	
	public enum State
	{
		Ready = 0,
		Printing = 1,
		Paused = 2,
		FastForwarding = 3,
		Finished = 4,
		Advertising = 5,
		Invalid = 6,
	}
	private State currentState = State.Invalid;
	private State afterAdState;

	public CanvasScaler canvasScaler; 
	public RectTransform scrollPanelRectTransform;

	public TouchDownCallback shareCallback;
	public Text shareText;
	public Image shareImage;

	public TouchDownCallback restartCallback;
	public Text restartText;
	public Image restartImage;

	private string shareCopy = "Look what I made! Make something cool with #20print! https://itunes.apple.com/us/app/20print/id900236159?mt=8";

	public Image navigationImage;
	public Image settingsIndent;
	public Image mainIndent;
	public Image storeIndent;
	public Image settingsHighlight;
	public Image mainHighlight;
	public Image storeHighlight;
	
	public TouchDownCallback restorePurchasesCallback;
	public TouchDownCallback supportCallback;
	public TouchDownCallback moreCallback;

	private float fillTime = 10f;
	private float speed;

	private int index = 0;

	private int[] deck;

	private float colorOffset;

	private int mode = 1;

	private int offscreenWidth;

	private float navigationDuration = 0.2f;
	private float currentScreenX = 0f;
	private float targetScreenX = 0f;
	private float extraScreenPercent = 0.25f;

	private float widthRatio = 1f;
	private float dragBeginX;
	private float dragDeltaX = 0f;
	private float dragThreshold = 15f;
	private float swipeThreshold = 20f;

	private bool showUI = true;
	private bool wasFastForwarding = false;
	private bool wasSharing = false;
	private bool wasRestarting = false;
	private bool wasDragging = false;

	private float lastTouchUpTime;
	private float doubleTapMaxDuration = 0.1f;

	private static GameAgent mInstance = null;
	public static GameAgent instance
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
			Debug.LogError( string.Format( "Only one instance of GameAgent allowed! Destroying:" + gameObject.name +", Other:" + mInstance.gameObject.name ) );
			Destroy( gameObject );
			return;
		}
		
		mInstance = this;

		Application.targetFrameRate = 60;
	}

	void Start()
	{
		if( canvasScaler )
			widthRatio = canvasScaler.referenceResolution.x / Screen.width;

		deck = new int[ BoardAgent.BoardSize ];

		offscreenWidth = Mathf.CeilToInt( ( (float)( BoardAgent.NumScreens - 1 ) * 0.5f ) * BoardAgent.ScreenWidth );

		afterAdState = ( Application.isEditor ? State.Advertising : State.Ready );

		ChangeState( State.Ready );
	}

	void OnEnable()
	{
		if( shareCallback )
			shareCallback.OnAreaTouch += OnShareAreaTouch;

		if( restartCallback )
			restartCallback.OnAreaTouch +=OnRestartAreaTouch;

		if( restorePurchasesCallback )
			restorePurchasesCallback.OnAreaTouch += OnRestorePurchasesAreaTouch;

		if( supportCallback )
			supportCallback.OnAreaTouch += OnSupportAreaTouch;

		if( moreCallback )
			moreCallback.OnAreaTouch += OnMoreAreaTouch;

		FingerGestures.OnFingerUp += OnTouchUp;
		FingerGestures.OnFingerLongPress += OnLongPress;
		FingerGestures.OnFingerDoubleTap += OnDoubleTap;
		FingerGestures.OnFingerDragBegin += OnDragDown;
		FingerGestures.OnFingerDragMove += OnDragMove;
		FingerGestures.OnFingerDragEnd += OnDragUp;
	}

	void OnDisable()
	{
		if( shareCallback )
			shareCallback.OnAreaTouch -= OnShareAreaTouch;

		if( restartCallback )
			restartCallback.OnAreaTouch -= OnRestartAreaTouch;
		
		if( restorePurchasesCallback )
			restorePurchasesCallback.OnAreaTouch -= OnRestorePurchasesAreaTouch;
		
		if( supportCallback )
			supportCallback.OnAreaTouch -= OnSupportAreaTouch;
        
        if( moreCallback )
			moreCallback.OnAreaTouch -= OnMoreAreaTouch;

		FingerGestures.OnFingerUp -= OnTouchUp;
		FingerGestures.OnFingerLongPress -= OnLongPress;
		FingerGestures.OnFingerDoubleTap -= OnDoubleTap;
		FingerGestures.OnFingerDragBegin -= OnDragDown;
		FingerGestures.OnFingerDragMove -= OnDragMove;
		FingerGestures.OnFingerDragEnd -= OnDragUp;
	}

	void OnApplicationPause( bool pauseStatus )
	{
		if( pauseStatus )
		{ 
			if( currentState == State.Printing )
			{
				ChangeState( State.Paused );
				TipAgent.ShowFirstTip();
			}

			if( currentState == State.Paused || currentState == State.Finished )
			{
				TipAgent.ShowFirstTip();
			}
		}

		if( !pauseStatus && currentState == State.Advertising )
		{
			ChangeState( State.Finished );
		}
	}

	/*
	void OnGUI()
	{
		GUI.Label( new Rect( 10f, 10f, 1000f, 1000f ), "" + currentState );
	}
	*/

	private void OnRestartAreaTouch()
	{
		wasRestarting = true;

		if( currentState != State.Finished )
		{
			AudioAgent.StopSoundEffect( AudioAgent.SoundEffectType.Print );
			AnalyticsAgent.LogAnalyticEvent( AnalyticsAgent.AnalyticEvent.PrintFinished );
		}

		ChangeState( State.Advertising );
	}

	private void OnShareAreaTouch()
	{
		wasSharing = true;

		AnalyticsAgent.LogAnalyticEvent( AnalyticsAgent.AnalyticEvent.Share );

		StartCoroutine( "DoShare" );
	}

	private void OnRestorePurchasesAreaTouch()
	{
		IAPAgent.RestorePurchases();
    }

	private void OnSupportAreaTouch()
	{
		if( Application.isEditor )
			Application.OpenURL( "http://www.twentypercentgames.com/contact" );
		else
			IOSSharedApplication.instance.OpenUrl( "http://www.twentypercentgames.com/contact" );
    }

	private void OnMoreAreaTouch()
	{
		AnalyticsAgent.LogAnalyticEvent( AnalyticsAgent.AnalyticEvent.More );

		if( Application.isEditor )
			Application.OpenURL( "http://itunes.com/TwentyPercent" );
		else
			IOSSharedApplication.instance.OpenUrl( "itms-apps://itunes.com/TwentyPercent" );
    }

	private void OnTouchUp( int fingerIndex, Vector2 fingerPos, float timeHeldDown )
	{
		bool didDoubleTap = ( Time.time - lastTouchUpTime < doubleTapMaxDuration );

		if( didDoubleTap )
			OnDoubleTap( fingerIndex, fingerPos );

		if( !didDoubleTap && !wasSharing && !wasRestarting && !wasDragging && !RatingAgent.GetPopUpEnabled() && CameraAgent.MainCameraObject.transform.localPosition.x == 0f )
		{
			switch( currentState )
			{
				case State.Ready:
				{
					ChangeState( State.Printing );
				} break;

				case State.Printing:
				{
					if( !wasFastForwarding )
						ChangeState( State.Paused );
				} break;

				case State.Paused:
				{
					ChangeState( State.Printing );
				} break;

				case State.FastForwarding:
				{
					ChangeState( State.Printing );
				} break;

				case State.Finished:
				{
					if( !wasFastForwarding )
					{
						showUI = !showUI;
						SetUIEnabled( true );
					}
				} break;

				case State.Advertising:
				{
					if( Application.isEditor && !AdAgent.GetIsShowing() )
						ChangeState( State.Ready );
				} break;
			}
		}

		wasFastForwarding = false;
		wasSharing = false;
		wasRestarting = false;

		lastTouchUpTime = Time.time;
	}

	private void OnLongPress( int fingerIndex, Vector2 fingerPos )
	{
		if( CameraAgent.MainCameraObject.transform.localPosition.x == 0f )
		{
			if( currentState == State.Ready || currentState == State.Paused )
			{
				ChangeState( State.Printing );
			}

			if( currentState == State.Printing )
			{
				ChangeState( State.FastForwarding );
			}
		}
	}

	private void OnDoubleTap( int fingerIndex, Vector2 fingerPos )
	{
		if( ( currentState == State.Printing || currentState == State.Paused ) && CameraAgent.MainCameraObject.transform.localPosition.x == 0f )
		{
			StopCoroutine( "DoPrint" );
			AudioAgent.StopSoundEffect( AudioAgent.SoundEffectType.Print );

			while( index < BoardAgent.BoardSize )
			{
				SinglePrint();
				
				index++;
			}

			FinishPrint();
		}
	}

	private void OnDragDown( int fingerIndex, Vector2 fingerPos, Vector2 startPos )
	{
		dragBeginX = startPos.x;
		dragDeltaX = 0f;
	}
	
	private void OnDragMove( int fingerIndex, Vector2 fingerPos, Vector2 delta )
	{
		if( RatingAgent.GetPopUpEnabled() )
			return;

		if( !wasDragging )
		{
			if( Mathf.Abs( fingerPos.x - dragBeginX ) > dragThreshold )
			{
				if( currentState == State.Printing || currentState == State.FastForwarding )
					ChangeState( State.Paused );

				showUI = true;
				SetUIEnabled( currentState != State.Ready );

				StopCoroutine( "DoNavigation" );

				wasDragging = true;

				targetScreenX = CameraAgent.MainCameraObject.transform.localPosition.x;
				StartCoroutine( "DoDragNavigation" );
			}

			if( !wasDragging )
				return;
		}

		float newPosition = targetScreenX - delta.x;

		float absPosition = Mathf.Abs( newPosition );

		if( absPosition < Screen.width * ( 1f + extraScreenPercent ) )
			targetScreenX = newPosition;
		else
			targetScreenX = Screen.width * ( 1f + extraScreenPercent ) * Mathf.Sign( newPosition );

		UpdateNavigationHighlight( true );

		dragDeltaX = delta.x;
	}
	
	private void OnDragUp( int fingerIndex, Vector2 fingerPos )
	{
		if( RatingAgent.GetPopUpEnabled() )
			return;

		if( Mathf.Abs( dragDeltaX ) > swipeThreshold )
		{
			currentScreenX -= Screen.width * Mathf.Sign( dragDeltaX );
			AudioAgent.PlaySoundEffect( AudioAgent.SoundEffectType.Swipe );
		}
		else
		{
			currentScreenX = Mathf.Round( CameraAgent.MainCameraObject.transform.localPosition.x / Screen.width ) * Screen.width;
		}

		currentScreenX = Mathf.Clamp( currentScreenX, Screen.width * -1f, Screen.width );
		StopCoroutine( "DoDragNavigation" );
		StartCoroutine( "DoNavigation", currentScreenX );

		wasDragging = false;
	}

	public static State GetCurrentState()
	{
		if( instance )
			return instance.currentState;
		
		return State.Invalid;
	}

	public static State GetAfterAdState()
	{
		if( instance )
			return instance.afterAdState;
		
		return State.Invalid;
	}

	public static bool GetWasHolding()
	{
		if( instance )
			return instance.wasDragging || instance.wasFastForwarding;

		return false;
	}

	public static void ChangeState( State newState )
	{
		if( instance )
			instance.internalChangeState( newState );
	}

	private void internalChangeState( State newState )
	{
		if( currentState == newState )
			return;

		currentState = newState;

		switch( currentState )
		{
			case State.Ready:
			{
				showUI = true;
				SetUIEnabled( false );
				TipAgent.ShowFirstTip();
				UpdateNavigationHighlight( true );

				ColorAgent.AdvanceColorPack();

				SpriteAgent.ClearSpriteNames();

				BoardAgent.ResetBoard();
				ShuffleDeck();
				colorOffset = Random.Range( 0f, 360f );
				SpriteAgent.Randomize();

				index = 0;
			} break;
				
			case State.Printing:
			{
				SetUIEnabled( false );
			
				speed = (float)BoardAgent.BoardSize / fillTime;

				SpriteAgent.LogSpriteName();

				AudioAgent.PlaySoundEffect( AudioAgent.SoundEffectType.Print, fillTime );
				AudioAgent.PitchSoundEffect( AudioAgent.SoundEffectType.Print, 1f );

				if( index == 0 )
					StartCoroutine( "DoPrint" );
			} break;
				
			case State.Paused:
			{				
				TipAgent.ShowNextTip();
				SetUIEnabled( true );
				
				speed = 0f;

				AudioAgent.PauseSoundEffect( AudioAgent.SoundEffectType.Print );
			} break;
				
			case State.FastForwarding:
			{				
				SetUIEnabled( false );
				
				speed = (float)BoardAgent.BoardSize / fillTime * 5f;

				AudioAgent.PitchSoundEffect( AudioAgent.SoundEffectType.Print, 2f );

				wasFastForwarding = true;
			} break;
				
			case State.Finished:
			{
				showUI = true;
				TipAgent.ShowNextTip();
				SetUIEnabled( true );
			} break;
				
			case State.Advertising:
			{
				SetUIEnabled( false );
				RatingAgent.CheckForPrompt();
				AdAgent.ShowInterstitialImage();
			} break;
		}
	}

	private void SinglePrint()
	{	
		switch( mode )
		{
			case 0:
			{ 
				if( index % BoardAgent.ScreenWidth == 0 )
				{
					int height = BoardAgent.BoardHeight - 1 - index / BoardAgent.ScreenWidth;
					
					for( int i = 0; i < offscreenWidth; i++ )
						ActivateSprite( new Vector2( i % offscreenWidth, height ) );
				}

				ActivateSprite( new Vector2( index % BoardAgent.ScreenWidth + offscreenWidth, BoardAgent.BoardHeight - 1 - index / BoardAgent.ScreenWidth ) );

				if( index % BoardAgent.ScreenWidth == BoardAgent.ScreenWidth - 1 )
				{
					int height = BoardAgent.BoardHeight - 1 - index / BoardAgent.ScreenWidth;
					
					for( int i = 0; i < offscreenWidth; i++ )
						ActivateSprite( new Vector2( i % offscreenWidth + BoardAgent.ScreenWidth + offscreenWidth, height ) );
				}
			} break; 

			case 1: 
			{
				int deckIndex = deck[index];

				if( offscreenWidth < BoardAgent.ScreenWidth )
				{
					if( deckIndex % BoardAgent.ScreenWidth >= offscreenWidth )
						ActivateSprite( new Vector2( deckIndex % BoardAgent.ScreenWidth - offscreenWidth + 1, BoardAgent.BoardHeight - 1 - deckIndex / BoardAgent.ScreenWidth ) );

					if( deckIndex % BoardAgent.ScreenWidth < offscreenWidth )
						ActivateSprite( new Vector2( deckIndex % BoardAgent.ScreenWidth + BoardAgent.ScreenWidth + offscreenWidth, BoardAgent.BoardHeight - 1 - deckIndex / BoardAgent.ScreenWidth ) );
				}
				else if( offscreenWidth == BoardAgent.ScreenWidth )
				{
					ActivateSprite( new Vector2( deckIndex % offscreenWidth, BoardAgent.BoardHeight - 1 - deckIndex / BoardAgent.ScreenWidth ) );
					ActivateSprite( new Vector2( deckIndex % offscreenWidth + BoardAgent.ScreenWidth + offscreenWidth, BoardAgent.BoardHeight - 1 - deckIndex / BoardAgent.ScreenWidth ) );
				}

				ActivateSprite( new Vector2( deckIndex % BoardAgent.ScreenWidth + offscreenWidth, BoardAgent.BoardHeight - 1 - deckIndex / BoardAgent.ScreenWidth ) );
				
			} break;
		}
	}

	private void FinishPrint()
	{
		AnalyticsAgent.LogAnalyticEvent( AnalyticsAgent.AnalyticEvent.PrintFinished );
		AudioAgent.StopSoundEffect( AudioAgent.SoundEffectType.Print );
		AudioAgent.PlaySoundEffect( AudioAgent.SoundEffectType.PrintFinish );

		ChangeState( State.Finished );
	}

	private void SetUIEnabled( bool enabled )
	{
		if( shareText )
			shareText.enabled = enabled && showUI;
		
		if( shareImage )
			shareImage.enabled = enabled && showUI;
		
		if( shareCallback )
			shareCallback.gameObject.SetActive( enabled && showUI );

		if( restartText )
			restartText.enabled = enabled && showUI;

		if( restartImage )
			restartImage.enabled = enabled && showUI;

		if( restartCallback )
			restartCallback.gameObject.SetActive( enabled && showUI );

		if( navigationImage )
			navigationImage.enabled = enabled;

		if( settingsIndent )
			settingsIndent.enabled = enabled;

		if( mainIndent )
			mainIndent.enabled = enabled;

		if( storeIndent )
			storeIndent.enabled = enabled;

		if( settingsHighlight )
			settingsHighlight.enabled = enabled;

		if( mainHighlight )
			mainHighlight.enabled = enabled;

		if( storeHighlight )
			storeHighlight.enabled = enabled;

		UpdateNavigationHighlight( enabled );
	}

	private void UpdateNavigationHighlight( bool canShow )
	{
		bool shouldShowTip = canShow && CameraAgent.MainCameraObject.transform.localPosition.x == 0f && !wasDragging && showUI;

		TipAgent.SetTipEnabled( shouldShowTip );

		if( navigationImage )
			navigationImage.enabled = shouldShowTip;

		bool shouldShowDots = canShow && ( CameraAgent.MainCameraObject.transform.localPosition.x != 0f || showUI );

		if( settingsIndent )
			settingsIndent.enabled = shouldShowDots;
		
		if( mainIndent )
			mainIndent.enabled = shouldShowDots;

		if( storeIndent )
			storeIndent.enabled = shouldShowDots;

		if( settingsHighlight )
			settingsHighlight.enabled = ( shouldShowDots && currentScreenX == Screen.width * -1f );
		
		if( mainHighlight )
			mainHighlight.enabled = ( shouldShowDots && currentScreenX == 0f );
		
		if( storeHighlight )
			storeHighlight.enabled = ( shouldShowDots && currentScreenX == Screen.width );
	}

	private IEnumerator DoDragNavigation()
	{
		while( wasDragging || CameraAgent.MainCameraObject.transform.localPosition.x != targetScreenX )
		{
			SetCameraX( Mathf.Lerp( CameraAgent.MainCameraObject.transform.localPosition.x, targetScreenX, 0.5f ) );

			yield return null;
		}
	}

	private IEnumerator DoNavigation( float toX )
	{
		if( currentState == State.Printing )
			ChangeState( State.Paused );

		float fromX = CameraAgent.MainCameraObject.transform.localPosition.x;
		float currentTime = 0f;
		float lerp;

		do
		{
			currentTime += Time.deltaTime;
			lerp = Mathf.Clamp01( currentTime / navigationDuration );

			lerp = Mathf.Pow( lerp, 0.5f );

			//lerp = 3f * Mathf.Pow( lerp, 2f ) - 2f * Mathf.Pow( lerp, 3f );

			SetCameraX( Mathf.Lerp( fromX, toX, lerp ) );

			yield return null;

		} while( currentTime < navigationDuration );

		SetCameraX( toX );

		UpdateNavigationHighlight( true );

		if( currentState == State.Ready && currentScreenX == 0f )
		{
			SetUIEnabled( false );
			TipAgent.SetTipEnabled( true );
			UpdateNavigationHighlight( true );
		}

		if( currentScreenX < 0f )
			AnalyticsAgent.LogAnalyticEvent( AnalyticsAgent.AnalyticEvent.SettingsScreen );

		if( currentScreenX > 0f )
			AnalyticsAgent.LogAnalyticEvent( AnalyticsAgent.AnalyticEvent.StoreScreen );
	}

	private IEnumerator DoPrint()
	{
		index = 0;
		float currentDistance = 0f;

		while( index < BoardAgent.BoardSize )
		{
			currentDistance += speed * Time.deltaTime;

			while( currentDistance >= 1f )
			{ 
				SinglePrint();

				index++;
				currentDistance -= 1f;

				if( index == BoardAgent.BoardSize )
					break;
			}

			yield return null;
		}

		FinishPrint();
	}

	private IEnumerator DoShare()
	{
		ScreenshotAgent.Enable();
		
		yield return null;

		IOSSocialManager.instance.ShareMedia( shareCopy, ScreenshotAgent.GetTexture() );
	}

	private void ActivateSprite( Vector2 position )
	{
		if( BoardAgent.GetSpriteEnabled( position ) )
			return;

		BoardAgent.SetSpriteImage( position, SpriteAgent.GetCurrentSprite() );

		Color color = ColorAgent.GetCurrentColorPack().foregroundColor;

		if( color == ColorAgent.RainbowColor )
			BoardAgent.SetSpriteColor( position, Utilities.ColorFromHSV( ( ( position.y / (float)BoardAgent.BoardHeight ) * 360f + colorOffset )%360f, 1f, 1f ) );
		else if( color == ColorAgent.RandomColor )
			BoardAgent.SetSpriteColor( position, Utilities.ColorFromHSV( Random.Range( 0f, 360f ), 1f, 1f ) );
		else
			BoardAgent.SetSpriteColor( position, color );

		BoardAgent.SetSpriteScale( position, new Vector3( BoardAgent.CellSize * ( Random.value < 0.5f ? 1f : -1f ), BoardAgent.CellSize * ( Random.value < 0.5f ? 1f : -1f ), 1f ) );
		BoardAgent.SetSpriteEnabled( position, true );
	}

	private void ShuffleDeck()
	{
		for( int i = 0; i < deck.Length; i++ )
			deck[i] = i;

		int randomValue;
		int temp;

		for( int i = 0; i < deck.Length; i++ )
		{
			randomValue = Random.Range( 0, deck.Length );
			temp = deck[i];
			deck[i] = deck[randomValue];
			deck[randomValue] = temp;
		}
	}

	private void SetCameraX( float x )
	{
		CameraAgent.MainCameraObject.transform.localPosition = Vector3.right * Mathf.Round( x );

		if( scrollPanelRectTransform )
			scrollPanelRectTransform.transform.localPosition = CameraAgent.MainCameraObject.transform.localPosition * widthRatio * -1f;
	}
}
