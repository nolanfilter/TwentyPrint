using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardAgent : MonoBehaviour {

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

	public static int BoardSize
	{
		get
		{
			return mBoardWidth * mBoardHeight;
		}
	}

	public GameObject spritePrefab;
	private SpriteRenderer[ , ] BoardSprites;

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
	}

	void Start()
	{
		mBoardHeight = Mathf.CeilToInt( (float)( BoardWidth / 3 ) / (float)Screen.width * (float)Screen.height );

		CellSize = ( Screen.width ) / ( BoardWidth / 3 );

		MarginWidth = ( Screen.width - CellSize * BoardWidth ) * 0.5f;
		MarginHeight = ( Screen.height - CellSize * BoardHeight ) * 0.5f;

		BoardSprites = new SpriteRenderer[ BoardWidth, BoardHeight ];
		GameObject go;

		for( int i = 0; i < BoardWidth; i++ )
			for( int j = 0; j < BoardHeight; j++ )
				BoardSprites[ i, j ] = MakeSpriteAt( GridToScreenPosition( new Vector2( i, j ) ) );

		ResetBoard();
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

		BoardSprites[ (int)position.x, (int)position.y ].color = new Color( newColor.r, newColor.g, newColor.b, BoardSprites[ (int)position.x, (int)position.y ].color.a );
	}

	private SpriteRenderer MakeSpriteAt( Vector2 position )
	{
		GameObject go = Instantiate( spritePrefab ) as GameObject;
		go.transform.position  = new Vector3( position.x, position.y, 1f );
		go.transform.localScale = new Vector3( CellSize, CellSize, 1f );

		SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();

		float alpha = 1f;
		if( Mathf.Abs( position.x - Screen.width * 0.5f ) > Screen.width * 0.5f )
			alpha = Mathf.Lerp( 1f, 0f, ( Mathf.Abs( position.x - Screen.width * 0.5f ) - Screen.width * 0.5f ) / Screen.width );
					
		spriteRenderer.color = new Color( 1f, 1f, 1f, alpha );

		return spriteRenderer;
	}
}
