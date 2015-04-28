using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.iOS;
using UnityEngine.UI;
using System.Collections;
using UnionAssets.FLE;

public class GameController : MonoBehaviour {
	
	enum State
	{
		Ready = 0,
		Printing = 1,
		Paused = 2,
		FastForwarding = 3,
		Complete = 4,
		Advertising = 5,
		Invalid = 6,
	}
	private State currentState = State.Invalid;

	public CanvasScaler canvasScaler; 
	public RectTransform scrollPanelRectTransform;

	private float speed;
	
	private int index = 0;

	private int[] deck;

	private float colorOffset;
	
	private const string iosGameID = "32974";
	private const string androidGameID = "33171";
	private string gameID;

	private int numTimesPrinted = 0;

	private int mode = 0;

	private float navigationDuration = 0.2f;
	private float currentScreenX = 0f;
	private float widthRatio = 1f;
	private float dragBeginTime;
	private Vector2 dragBeginPosition;
	private bool wasFastForwarding = false;

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

		currentState = State.Ready;
	}

	void OnEnable()
	{
		FingerGestures.OnFingerUp += OnTouchUp;
		FingerGestures.OnFingerLongPress += OnLongPress;
		FingerGestures.OnFingerDoubleTap += OnDoubleTap;
		FingerGestures.OnFingerDragBegin += OnDragDown;
		FingerGestures.OnFingerDragMove += OnDragMove;
		FingerGestures.OnFingerDragEnd += OnDragUp;
	}

	void OnDisable()
	{
		FingerGestures.OnFingerUp -= OnTouchUp;
		FingerGestures.OnFingerLongPress -= OnLongPress;
		FingerGestures.OnFingerDoubleTap -= OnDoubleTap;
		FingerGestures.OnFingerDragBegin -= OnDragDown;
		FingerGestures.OnFingerDragMove -= OnDragMove;
		FingerGestures.OnFingerDragEnd -= OnDragUp;
	}

	/*
	bool tokenSent = false;
	void  FixedUpdate () {
		
		if (!tokenSent) { // tokenSent needs to be defined somewhere (bool tokenSent = false)

			byte[] token   =  UnityEngine.iOS.NotificationServices.deviceToken;
			
			if(token != null) {
				
				string tokenString =  System.BitConverter.ToString(token).Replace("-", "").ToLower();
				
				Debug.Log ("OnTokenReceived");
				
				Debug.Log (tokenString);
				
			}
			
		}
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

	private void OnTouchUp( int fingerIndex, Vector2 fingerPos, float timeHeldDown )
	{
		if( !wasFastForwarding && currentScreenX == 0f && Mathf.Abs( CameraAgent.MainCameraObject.transform.localPosition.x ) < Screen.width * 0.125f )
		{
			switch( currentState )
			{
				case State.Ready:
				{
					currentState = State.Printing;

					speed = 100f;

					BoardAgent.ResetBoard();
					ShuffleDeck();
					colorOffset = Random.Range( 0f, 360f );
					
					StartCoroutine( "DoPrint" );
				} break;

				case State.Printing:
				{
					currentState = State.Paused;

					speed = 0f;
				} break;

				case State.Paused:
				{
					currentState = State.Printing;

					speed = 100f;

				} break;

				case State.FastForwarding:
				{
					currentState = State.Printing;
					
					speed = 100f;
					
				} break;

				case State.Complete:
				{
					if( Application.isEditor )
						currentState = State.Advertising;
					else
						currentState = State.Ready;

					if( Advertisement.isReady() )
						Advertisement.Show();

					BoardAgent.ResetBoard();
				} break;

				case State.Advertising:
				{
					if( !Advertisement.isShowing )
						currentState = State.Ready;
				} break;
			}
		}

		wasFastForwarding = false;
	}

	private void OnLongPress( int fingerIndex, Vector2 fingerPos )
	{
		if( currentState == State.Printing )
		{
			currentState = State.FastForwarding;

			speed = 500f;

			wasFastForwarding = true;
		}
	}

	private void OnDoubleTap( int fingerIndex, Vector2 fingerPos )
	{
		if( currentState == State.Printing )
		{
			StopCoroutine( "DoPrint" );
			
			int deckIndex;
			
			while( index < BoardAgent.BoardSize )
			{
				deckIndex = deck[index];
				
				switch( mode )
				{
					case 0: ActivateSprite( new Vector2( index % BoardAgent.BoardWidth, BoardAgent.BoardHeight - 1 - index / BoardAgent.BoardWidth ) ); break;
					case 1: ActivateSprite( new Vector2( deckIndex % BoardAgent.BoardWidth, BoardAgent.BoardHeight - 1 - deckIndex / BoardAgent.BoardWidth ) ); break;
				}
				
				index++;
			}
			
			StartCoroutine( "FinishPrint" );
		}
	}

	private void OnDragDown( int fingerIndex, Vector2 fingerPos, Vector2 startPos )
	{
		if( currentState == State.Printing || currentState == State.FastForwarding )
		{
			currentState = State.Paused;
			speed = 0;
		}

		dragBeginTime = Time.time;
		dragBeginPosition = startPos;

		StopCoroutine( "DoNavigation" );
	}
	
	private void OnDragMove( int fingerIndex, Vector2 fingerPos, Vector2 delta )
	{
		float absPosition = Mathf.Abs( CameraAgent.MainCameraObject.transform.localPosition.x - delta.x );

		if( absPosition < Screen.width * 1.5f )
		{
			CameraAgent.MainCameraObject.transform.localPosition -= Vector3.right * delta.x;

			if( scrollPanelRectTransform )
				scrollPanelRectTransform.transform.localPosition += Vector3.right * delta.x * widthRatio;
		}
	}
	
	private void OnDragUp( int fingerIndex, Vector2 fingerPos )
	{
		if( Time.time - dragBeginTime < 0.25f )
		{
			float deltaFingerPosX = Mathf.Abs( fingerPos.x - dragBeginPosition.x );

			if( deltaFingerPosX > 25f && deltaFingerPosX > Mathf.Abs( fingerPos.y - dragBeginPosition.y ) )
				currentScreenX -= Screen.width * Mathf.Sign( fingerPos.x - dragBeginPosition.x );
		}
		else
		{
			currentScreenX = Mathf.Round( CameraAgent.MainCameraObject.transform.localPosition.x / Screen.width ) * Screen.width;
		}

		currentScreenX = Mathf.Clamp( currentScreenX, Screen.width * -1f, Screen.width );
		StartCoroutine( "DoNavigation", Vector3.right * currentScreenX );
	}

	private IEnumerator DoNavigation( Vector3 toPosition )
	{
		float beginTime = Time.time;
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
	}

	private IEnumerator DoPrint()
	{
		while( Advertisement.isShowing )
		{
			yield return null;
		}

		index = 0;
		int deckIndex;
		float currentDistance = 0f;

		while( index < BoardAgent.BoardSize )
		{
			currentDistance += speed * Time.deltaTime;

			while( currentDistance >= 1f )
			{
				deckIndex = deck[index];

				switch( mode )
				{
					case 0: ActivateSprite( new Vector2( index % BoardAgent.BoardWidth, BoardAgent.BoardHeight - 1 - index / BoardAgent.BoardWidth ) ); break;
					case 1: ActivateSprite( new Vector2( deckIndex % BoardAgent.BoardWidth, BoardAgent.BoardHeight - 1 - deckIndex / BoardAgent.BoardWidth ) ); break;
				}

				index++;
				currentDistance -= 1f;

				if( index == BoardAgent.BoardSize )
					break;
			}

			yield return null;
		}

		StartCoroutine( "FinishPrint" );
	}

	private IEnumerator FinishPrint()
	{
		numTimesPrinted++;

		currentState = State.Complete;

		ScreenshotAgent.Enable();

		yield return null;

		//IOSSocialManager.instance.ShareMedia( "Check this rad print!", ScreenshotAgent.GetTexture() );
	}

	private void ActivateSprite( Vector2 position )
	{
		BoardAgent.SetSpriteScale( position, new Vector3( BoardAgent.CellSize * ( Random.value < 0.5f ? 1f : -1f ), BoardAgent.CellSize * ( Random.value < 0.5f ? 1f : -1f ), 1f ) );
		//BoardAgent.SetSpriteColor( position, Utilities.ColorFromHSV( Random.Range( 0f, 360f ), 1f, 1f ) );
		BoardAgent.SetSpriteColor( position, Utilities.ColorFromHSV( ( ( position.y / (float)BoardAgent.BoardHeight ) * 360f + colorOffset )%360f, 1f, 1f ) );
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
