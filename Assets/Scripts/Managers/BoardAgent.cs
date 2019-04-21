using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BoardAgent : MonoBehaviour {

	//has to be odd
	private static int mBoardWidth = 75;
	public static int BoardWidth
	{
		get
		{
			return mBoardWidth;
		}
	}

	private static int mBoardHeight = 40;
	public static int BoardHeight
	{
		get
		{
			return mBoardHeight;
		}
	}

	public static int NumScreens = 3;

	public static int BoardSize
	{
		get
		{
			return ScreenWidth * mBoardHeight;
		}
	}

	public static int ScreenWidth
	{
		get
		{
			return ( mBoardWidth / NumScreens );
		}
	}
	
	public GameObject spritePrefab;
	private SpriteRenderer[ , ] BoardSprites;

	public Text detailText;
	public TouchDownCallback detailCallback;

	private bool useDetail;
	private string useDetailString = "useDetail";

	private static BoardAgent mInstance = null;
	public static BoardAgent instance
	{
		get
		{
			return mInstance;
		}
	}

	public static float CellSize;
	private static float MarginWidth;
	private static float MarginHeight;

	void Awake()
	{
		if( mInstance != null )
		{
			Debug.LogError( string.Format( "Only one instance of BoardAgent allowed! Destroying:" + gameObject.name +", Other:" + mInstance.gameObject.name ) );
			Destroy( gameObject );
			return;
		}
		
		mInstance = this;

		if( !PlayerPrefs.HasKey( useDetailString ) )
			PlayerPrefs.SetInt( useDetailString, 1 );
		
		useDetail = ( PlayerPrefs.GetInt( useDetailString ) == 1 );
		
		if( detailText )
			detailText.text = ( useDetail ? "Oleg on" : "Oleg off" );
	}

	void OnEnable()
	{
		if( detailCallback )
			detailCallback.OnAreaTouch += OnDetailAreaTouch;
	}

	void OnDisable()
	{
		if( detailCallback )
			detailCallback.OnAreaTouch -= OnDetailAreaTouch;
	}

	void Start()
	{
		mBoardHeight = Mathf.CeilToInt( (float)( BoardWidth / NumScreens ) / (float)Screen.width * (float)Screen.height );

		CellSize = (float)Screen.width / (float)( BoardWidth / NumScreens );

		MarginWidth = ( Screen.width - CellSize * BoardWidth ) * 0.5f;
		MarginHeight = ( Screen.height - CellSize * BoardHeight ) * 0.5f;

		BoardSprites = new SpriteRenderer[ BoardWidth, BoardHeight ];
		GameObject go;

		for( int i = 0; i < BoardWidth; i++ )
			for( int j = 0; j < BoardHeight; j++ )
				BoardSprites[ i, j ] = MakeSpriteAt( GridToScreenPosition( new Vector2( i, j ) ) );

		ResetBoard();
	}

    private void OnDetailAreaTouch()
	{
		SetUseDetail( !useDetail );
	}

	public static void SetUseDetail( bool newUseDetail )
	{
		if( instance )
			instance.internalSetUseDetail( newUseDetail );
	}
	
	private void internalSetUseDetail( bool newUseDetail )
	{
		useDetail = newUseDetail;
		PlayerPrefs.SetInt( useDetailString, ( useDetail ? 1 : 0 ) );
		
		if( detailText )
			detailText.text = ( useDetail ? "Oleg on" : "Oleg off" );
	}

	public static Vector2 GridToScreenPosition( Vector2 gridPosition )
	{
		if( instance )
			return instance.internalGridToScreenPosition( gridPosition );

		return Vector2.one * -1f;
	}

	private Vector2 internalGridToScreenPosition( Vector2 gridPosition )
	{
		return new Vector2( CellSize * ( gridPosition.x + 0.5f ) + MarginWidth, CellSize * ( gridPosition.y + 0.5f ) + MarginHeight );
	}

	public static void ResetBoard()
	{
		if( instance )
			instance.internalResetBoard();
	}
	
	private void internalResetBoard()
	{
		for( int i = 0; i < BoardWidth; i++ )
			for( int j = 0; j < BoardHeight; j++ )
				SetSpriteEnabled( new Vector2( i, j ), false );
    }

	public static void SetSpriteEnabled( Vector2 position, bool newEnabled )
	{
		if( instance )
			instance.internalSetSpriteEnabled( position, newEnabled );
	}

	private void internalSetSpriteEnabled( Vector2 position, bool newEnabled )
	{
		if( position.x < 0 || position.x > BoardWidth || position.y < 0 || position.y > BoardHeight )
			return;

		BoardSprites[ (int)position.x, (int)position.y ].enabled = newEnabled;
	}

	public static bool GetSpriteEnabled( Vector2 position )
	{
		if( instance )
			return instance.internalGetSpriteEnabled( position );

		return true;
	}

	private bool internalGetSpriteEnabled( Vector2 position )
	{
		if( position.x < 0 || position.x > BoardWidth || position.y < 0 || position.y > BoardHeight )
			return true;

		return BoardSprites[ (int)position.x, (int)position.y ].enabled;
	}

	public static void SetSpriteImage( Vector2 position, Sprite sprite )
	{
		if( instance )
			instance.internalSetSpriteImage( position, sprite );
	}
	
	private void internalSetSpriteImage( Vector2 position, Sprite sprite )
	{
		if( position.x < 0 || position.x > BoardWidth || position.y < 0 || position.y > BoardHeight )
			return;
		
		BoardSprites[ (int)position.x, (int)position.y ].sprite = sprite;
	}

	public static void SetSpriteScale( Vector2 position, Vector3 newScale )
	{
		if( instance )
			instance.internalSetSpriteScale( position, newScale );
	}

	private void internalSetSpriteScale( Vector2 position, Vector3 newScale )
	{
		if( position.x < 0 || position.x > BoardWidth || position.y < 0 || position.y > BoardHeight )
			return;

		BoardSprites[ (int)position.x, (int)position.y ].transform.localScale = newScale;
	}

    public static void SetSpriteColor( Vector2 position, Color newColor )
	{
		if( instance )
			instance.internalSetSpriteColor( position, newColor );
	}

	private void internalSetSpriteColor( Vector2 position, Color newColor )
	{
		if( position.x < 0 || position.x >= BoardWidth || position.y < 0 || position.y >= BoardHeight )
			return;

		Color color = new Color( newColor.r, newColor.g, newColor.b, BoardSprites[ (int)position.x, (int)position.y ].color.a );

		BoardSprites[ (int)position.x, (int)position.y ].color = color;
		BoardSprites[ (int)position.x, (int)position.y ].material.color = color;
		BoardSprites[ (int)position.x, (int)position.y ].material.SetFloat( "_UseDetail", ( useDetail ? 1f : 0f ) );
	}

	private SpriteRenderer MakeSpriteAt( Vector2 position )
	{
		GameObject go = Instantiate( spritePrefab ) as GameObject;
		go.transform.position  = new Vector3( position.x, position.y, 1f );
		go.transform.localScale = new Vector3( CellSize, CellSize, 1f );

		SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();

		float alpha = 1f;

		/*
		float relativeX = Mathf.Abs( position.x - Screen.width * 0.5f ) - Screen.width * 0.5f;
		if( relativeX > 0f )
		{
			alpha = Mathf.Lerp( 0.75f, 0f, relativeX / ( (float)( NumScreens - 1 ) * 0.5f * Screen.width ) );
		}
		*/
					
		spriteRenderer.color = new Color( 1f, 1f, 1f, alpha );

		return spriteRenderer;
	}
}
