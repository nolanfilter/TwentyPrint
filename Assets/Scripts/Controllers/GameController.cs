using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.iOS;
using UnityEngine.UI;
using System.Collections;
using UnionAssets.FLE;

public class GameController : MonoBehaviour {

	public CanvasScaler canvasScaler; 
	public RectTransform scrollPanelRectTransform;

	public float speed;

	private bool isRunning = false;

	private int index = 0;

	private int[] deck;

	private float colorOffset;
	
	private const string iosGameID = "32974";
	private const string androidGameID = "33171";
	private string gameID;

	private int numTimesPrinted = 0;

	private int mode = 1;

	private float navigationDuration = 0.2f;
	private float currentScreenX = 0f;
	private float width;
	private float dragBeginTime;
	private Vector2 dragBeginPosition;

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
			width = canvasScaler.referenceResolution.x;

		deck = new int[ BoardAgent.BoardSize ];
	}

	void OnEnable()
	{
		FingerGestures.OnFingerDown += OnTouchDown;
		FingerGestures.OnFingerDragBegin += OnDragDown;
		FingerGestures.OnFingerDragMove += OnDragMove;
		FingerGestures.OnFingerDragEnd += OnDragUp;
	}

	void OnDisable()
	{
		FingerGestures.OnFingerDown -= OnTouchDown;
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

	private void OnTouchDown( int fingerIndex, Vector2 fingerPos )
	{
		if( isRunning )
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
		else
		{
			if( numTimesPrinted > 0 && Advertisement.isReady() )
			{
				Advertisement.Show();
			}

			BoardAgent.ResetBoard();
			ShuffleDeck();
			colorOffset = Random.Range( 0f, 360f );

			StartCoroutine( "DoPrint" );
		}
	}

	void OnDragDown( int fingerIndex, Vector2 fingerPos, Vector2 startPos )
	{
		dragBeginTime = Time.time;
		dragBeginPosition = startPos;

		StopCoroutine( "DoNavigation" );
	}
	
	void OnDragMove( int fingerIndex, Vector2 fingerPos, Vector2 delta )
	{
		if( scrollPanelRectTransform == null )
			return;

		float absPosition = Mathf.Abs( scrollPanelRectTransform.localPosition.x + delta.x );

		if( absPosition < width * 1.5f )
			scrollPanelRectTransform.localPosition += Vector3.right * delta.x;
	}
	
	void OnDragUp( int fingerIndex, Vector2 fingerPos )
	{
		if( scrollPanelRectTransform == null )
			return;

		if( Time.time - dragBeginTime < 0.25f )
		{
			if( Mathf.Abs( fingerPos.x - dragBeginPosition.x ) > Mathf.Abs( fingerPos.y - dragBeginPosition.y ) )
				currentScreenX += width * Mathf.Sign( fingerPos.x - dragBeginPosition.x );
		}
		else
		{
			currentScreenX = Mathf.Round( scrollPanelRectTransform.localPosition.x / width ) * width;
		}

		currentScreenX = Mathf.Clamp( currentScreenX, width * -1f, width );
		StartCoroutine( "DoNavigation", Vector3.right * currentScreenX );
	}

	private IEnumerator DoNavigation( Vector3 toPosition )
	{
		if( scrollPanelRectTransform == null )
			yield break;

		float beginTime = Time.time;
		Vector3 fromPosition = scrollPanelRectTransform.localPosition;
		float currentTime = 0f;
		float lerp;

		do
		{
			currentTime += Time.deltaTime;
			lerp = Mathf.Clamp01( currentTime / navigationDuration );

			lerp = 3f * Mathf.Pow( lerp, 2f ) - 2f * Mathf.Pow( lerp, 3f );

			scrollPanelRectTransform.localPosition = Vector3.Lerp( fromPosition, toPosition, lerp );

			yield return null;

		} while( currentTime < navigationDuration );

		scrollPanelRectTransform.localPosition = toPosition;
	}

	private IEnumerator DoPrint()
	{
		while( Advertisement.isShowing )
		{
			yield return null;
		}

		isRunning = true;

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
		
		isRunning = false;
		
		ScreenshotAgent.Enable();

		yield return null;

		IOSSocialManager.instance.ShareMedia( "Check this rad print!", ScreenshotAgent.GetTexture() );
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
