using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.iOS;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnionAssets.FLE;

public class GameAgent : MonoBehaviour {
	
	enum State
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

	public Sprite[] sprites;

	public CanvasScaler canvasScaler; 
	public RectTransform scrollPanelRectTransform;

	public TouchDownCallback shareCallback;
	public Text shareText;
	public Image shareImage;

	public Image navigationImage;
	public Image settingsIndent;
	public Image mainIndent;
	public Image storeIndent;
	public Image settingsHighlight;
	public Image mainHighlight;
	public Image storeHighlight;

	private float fillTime = 10f;
	private float speed;

	private int index = 0;
	private int spriteIndex = 0;

	private int[] deck;

	private float colorOffset;
	
	private const string iosGameID = "32974";
	private const string androidGameID = "33171";
	private string gameID;

	private int numTimesPrinted = 0;

	private int mode = 0;

	private int offscreenWidth;

	private float navigationDuration = 0.2f;
	private float currentScreenX = 0f;
	private float widthRatio = 1f;
	private float dragBeginX;
	private float dragDeltaX = 0f;
	private float dragThreshold = 15f;
	private float swipeThreshold = 20f;

	private bool wasFastForwarding = false;
	private bool wasSharing = false;
	private bool wasDragging = false;
	
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
	}

	void Start()
	{
#if UNITY_IOS
		gameID = iosGameID;
#elif UNITY_ANDROID
		gameID = androidGameID;
#endif

		Advertisement.Initialize( gameID, true );
		Advertisement.allowPrecache = true;

		UnityEngine.iOS.NotificationServices.RegisterForNotifications( NotificationType.Alert | NotificationType.Badge | NotificationType.Sound );

		//IOSNotificationController.instance.RequestNotificationPermissions();

		/*
		IOSNotificationController.instance.OnNotificationScheduleResult += OnNotificationScheduleResult;

		IOSNotificationController.instance.ScheduleNotification (5, "Your Notification Text No Sound", false);
		*/

		IOSNotificationController.instance.RegisterForRemoteNotifications( NotificationType.Alert | NotificationType.Badge | NotificationType.Sound );

		IOSNotificationController.instance.addEventListener( IOSNotificationController.DEVICE_TOKEN_RECEIVED, OnTokenReceived );

		if( canvasScaler )
			widthRatio = canvasScaler.referenceResolution.x / Screen.width;

		deck = new int[ BoardAgent.BoardSize ];

		offscreenWidth = Mathf.CeilToInt( ( (float)( BoardAgent.NumScreens - 1 ) * 0.5f ) * BoardAgent.ScreenWidth );

		ChangeState( State.Ready );
	}

	void OnEnable()
	{
		if( shareCallback )
			shareCallback.OnAreaTouch += OnShareAreaTouch;

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

		FingerGestures.OnFingerUp -= OnTouchUp;
		FingerGestures.OnFingerLongPress -= OnLongPress;
		FingerGestures.OnFingerDoubleTap -= OnDoubleTap;
		FingerGestures.OnFingerDragBegin -= OnDragDown;
		FingerGestures.OnFingerDragMove -= OnDragMove;
		FingerGestures.OnFingerDragEnd -= OnDragUp;
	}

	void OnApplicationPause( bool pauseStatus )
	{
		if( pauseStatus && currentState == State.Printing )
		{
			ChangeState( State.Paused );
		}
	}

	/*
	void OnGUI()
	{
		GUI.Label( new Rect( 10f, 10f, 1000f, 1000f ), "" + currentState );
	}
	*/
            
	private void OnNotificationScheduleResult( ISN_Result res ) {
		IOSNotificationController.instance.OnNotificationScheduleResult -= OnNotificationScheduleResult;
		string msg = string.Empty;
		if(res.IsSucceeded) {
			msg += "Notification was successfully scheduled \n allowed notifications types: \n";
			if((IOSNotificationController.AllowedNotificationsType & IOSUIUserNotificationType.Alert) != 0) {
				msg += "Alert ";
			}
			if((IOSNotificationController.AllowedNotificationsType & IOSUIUserNotificationType.Sound) != 0) {
				msg += "Sound ";
			}
			if((IOSNotificationController.AllowedNotificationsType & IOSUIUserNotificationType.Badge) != 0) {
				msg += "Badge ";
			}
		} else {
			msg += "Notification scheduling failed";
		}
		
		IOSMessage.Create("On Notification Schedule Result", msg);
	}

	private void OnTokenReceived( CEvent e )
	{
		IOSNotificationDeviceToken token = e.data as IOSNotificationDeviceToken;
		
		Debug.Log( "OnTokenReceived" );
		
		Debug.Log( token.tokenString );

		IOSNotificationController.instance.removeEventListener( IOSNotificationController.DEVICE_TOKEN_RECEIVED, OnTokenReceived );
	}

	private void OnShareAreaTouch()
	{
		StartCoroutine( "DoShare" );

		wasSharing = true;
	}

	private void OnTouchUp( int fingerIndex, Vector2 fingerPos, float timeHeldDown )
	{
		if( !wasSharing && CameraAgent.MainCameraObject.transform.localPosition.x == 0f )
		{
			switch( currentState )
			{
				case State.Ready:
				{
					ChangeState( State.Printing );
				} break;

				case State.Printing:
				{
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
						ChangeState( State.Advertising );
				} break;

				case State.Advertising:
				{
					if( !Advertisement.isShowing )
						ChangeState( State.Ready );
				} break;
			}
		}

		wasFastForwarding = false;
		wasSharing = false;
	}

	private void OnLongPress( int fingerIndex, Vector2 fingerPos )
	{
		if( currentState == State.Printing )
		{
			ChangeState( State.FastForwarding );
		}
	}

	private void OnDoubleTap( int fingerIndex, Vector2 fingerPos )
	{
		if( ( currentState == State.Printing || currentState == State.Paused ) && CameraAgent.MainCameraObject.transform.localPosition.x == 0f )
		{
			StopCoroutine( "DoPrint" );

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
		if( !wasDragging )
		{
			if( Mathf.Abs( fingerPos.x - dragBeginX ) > dragThreshold )
			{
				if( currentState == State.Printing || currentState == State.FastForwarding )
					ChangeState( State.Paused );
				else
					SetShareEnabled( true );

				StopCoroutine( "DoNavigation" );

				wasDragging = true;
			}

			if( !wasDragging )
				return;
		}

		float absPosition = Mathf.Abs( CameraAgent.MainCameraObject.transform.localPosition.x - delta.x );

		if( absPosition < Screen.width * 1.5f )
		{
			CameraAgent.MainCameraObject.transform.localPosition -= Vector3.right * delta.x;

			if( scrollPanelRectTransform )
				scrollPanelRectTransform.transform.localPosition += Vector3.right * delta.x * widthRatio;
		}

		dragDeltaX = delta.x;
	}
	
	private void OnDragUp( int fingerIndex, Vector2 fingerPos )
	{
		if( Mathf.Abs( dragDeltaX ) > swipeThreshold )
			currentScreenX -= Screen.width * Mathf.Sign( dragDeltaX );
		else
			currentScreenX = Mathf.Round( CameraAgent.MainCameraObject.transform.localPosition.x / Screen.width ) * Screen.width;

		currentScreenX = Mathf.Clamp( currentScreenX, Screen.width * -1f, Screen.width );
		StartCoroutine( "DoNavigation", Vector3.right * currentScreenX );

		wasDragging = false;
	}

	private void ChangeState( State newState )
	{
		if( currentState == newState )
			return;

		currentState = newState;

		switch( currentState )
		{
			case State.Ready:
			{
				SetShareEnabled( false );

				ColorAgent.UpdateColor( ColorAgent.ColorType.Foreground );
				ColorAgent.UpdateColor( ColorAgent.ColorType.Background );
				ColorAgent.UpdateColor( ColorAgent.ColorType.Mid );

				BoardAgent.ResetBoard();
				ShuffleDeck();
				colorOffset = Random.Range( 0f, 360f );

				index = 0;
			} break;
				
			case State.Printing:
			{
				SetShareEnabled( false );
			
				speed = (float)BoardAgent.BoardSize / fillTime;

				if( index == 0 )
					StartCoroutine( "DoPrint" );
			} break;
				
			case State.Paused:
			{				
				SetShareEnabled( true );
				
				speed = 0f;
				
			} break;
				
			case State.FastForwarding:
			{				
				SetShareEnabled( false );
				
				speed = (float)BoardAgent.BoardSize / fillTime * 5f;

				wasFastForwarding = true;
			} break;
				
			case State.Finished:
			{
				SetShareEnabled( true );
			} break;
				
			case State.Advertising:
			{
				if( Advertisement.isReady() )
					Advertisement.Show();

				if( !Application.isEditor )
					ChangeState( State.Ready );

				ColorAgent.UpdateColor( ColorAgent.ColorType.Background );
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

				ActivateSprite( new Vector2( deckIndex % offscreenWidth, BoardAgent.BoardHeight - 1 - deckIndex / BoardAgent.ScreenWidth ) );
				ActivateSprite( new Vector2( deckIndex % BoardAgent.ScreenWidth + offscreenWidth, BoardAgent.BoardHeight - 1 - deckIndex / BoardAgent.ScreenWidth ) );
				ActivateSprite( new Vector2( deckIndex % offscreenWidth + BoardAgent.ScreenWidth + offscreenWidth, BoardAgent.BoardHeight - 1 - deckIndex / BoardAgent.ScreenWidth ) );
			} break;
		}
	}

	private void FinishPrint()
	{
		numTimesPrinted++;
		
		mode = Random.Range( 0, 2 );
		spriteIndex = Random.Range( 0, sprites.Length );
		ColorAgent.AdvanceColorPack();
		
		ChangeState( State.Finished );
	}

	private void SetShareEnabled( bool enabled )
	{
		if( shareText )
			shareText.enabled = enabled;
		
		if( shareImage )
			shareImage.enabled = enabled;
		
		if( shareCallback )
			shareCallback.enabled = enabled;

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
		if( settingsHighlight )
			settingsHighlight.enabled = ( canShow && currentScreenX == Screen.width * -1f );
		
		if( mainHighlight )
			mainHighlight.enabled = ( canShow && currentScreenX == 0f );
		
		if( storeHighlight )
			storeHighlight.enabled = ( canShow && currentScreenX == Screen.width );
	}

	private IEnumerator DoNavigation( Vector3 toPosition )
	{
		if( currentState == State.Printing )
			ChangeState( State.Paused );

		Vector3 fromPosition = CameraAgent.MainCameraObject.transform.localPosition;
		float currentTime = 0f;
		float lerp;
		Vector3 relativeFromPosition = Vector3.zero;
		Vector3 relativeToPosition = Vector3.zero;

		if( scrollPanelRectTransform )
		{
			relativeFromPosition = scrollPanelRectTransform.transform.localPosition;
			relativeToPosition = new Vector3( toPosition.x * widthRatio * -1f, scrollPanelRectTransform.transform.localPosition.y, scrollPanelRectTransform.transform.localPosition.z );
		}

		do
		{
			currentTime += Time.deltaTime;
			lerp = Mathf.Clamp01( currentTime / navigationDuration );

			lerp = 3f * Mathf.Pow( lerp, 2f ) - 2f * Mathf.Pow( lerp, 3f );

			CameraAgent.MainCameraObject.transform.localPosition = Vector3.Lerp( fromPosition, toPosition, lerp );

			if( scrollPanelRectTransform )
				scrollPanelRectTransform.transform.localPosition = Vector3.Lerp( relativeFromPosition, relativeToPosition, lerp );

			yield return null;

		} while( currentTime < navigationDuration );

		CameraAgent.MainCameraObject.transform.localPosition = toPosition;

		UpdateNavigationHighlight( true );

		if( currentState == State.Ready && currentScreenX == 0f )
			SetShareEnabled( false );
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

		IOSSocialManager.instance.ShareMedia( "Check this rad print!", ScreenshotAgent.GetTexture() );
	}

	private void ActivateSprite( Vector2 position )
	{
		if( BoardAgent.GetSpriteEnabled( position ) )
			return;

		BoardAgent.SetSpriteImage( position, sprites[ spriteIndex ] );

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
}
