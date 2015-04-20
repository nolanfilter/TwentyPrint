using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;

public class GameController : MonoBehaviour {

	public float speed;

	private bool isRunning = false;

	private int index = 0;

	private int[] deck;

	private float colorOffset;

	private const string gameID = "32974";

	private int numTimesPrinted = 0;

	void Start()
	{
		Advertisement.Initialize( gameID, true );

		deck = new int[ BoardAgent.BoardSize ];
	}

	void OnEnable()
	{
		FingerGestures.OnFingerDown += OnPress;
	}

	void OnDisable()
	{
		FingerGestures.OnFingerDown -= OnPress;
	}

	private void OnPress( int fingerIndex, Vector2 fingerPos )
	{
		if( isRunning )
		{
			return;

			StopCoroutine( "DoPrint" );

			int deckIndex;

			while( index < BoardAgent.BoardSize )
			{
				deckIndex = deck[index];
				ActivateSprite( new Vector2( deckIndex % BoardAgent.BoardWidth, BoardAgent.BoardHeight - 1 - deckIndex / BoardAgent.BoardWidth ) );
				//ActivateSprite( new Vector2( i % BoardAgent.BoardWidth, BoardAgent.BoardHeight - 1 - i / BoardAgent.BoardWidth ) );
			
				index++;
			}

			isRunning = false;
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

	private IEnumerator DoPrint()
	{
		isRunning = true;

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
				//ActivateSprite( new Vector2( deckIndex % BoardAgent.BoardWidth, BoardAgent.BoardHeight - 1 - deckIndex / BoardAgent.BoardWidth ) );
				ActivateSprite( new Vector2( index % BoardAgent.BoardWidth, BoardAgent.BoardHeight - 1 - index / BoardAgent.BoardWidth ) );

				index++;
				currentDistance -= 1f;

				if( index == BoardAgent.BoardSize )
					break;
			}

			yield return null;
		}

		numTimesPrinted++;

		isRunning = false;
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
