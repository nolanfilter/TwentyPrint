using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TipAgent : MonoBehaviour {

	public Text tipText;

	public string[] tips;

	public int firstTipIndex = 0;
	public int secondTipIndex = 1;

	private List<int> deck;

	private static TipAgent mInstance;
	public static TipAgent instance
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
			Debug.LogError( "Only one instance of TipAgent allowed. Destroying " + gameObject + " and leaving " + mInstance.gameObject );
			Destroy( gameObject );
			return;
		}
		
		mInstance = this;

		deck = new List<int>();
	}
	
	public static void ShowFirstTip()
	{
		if( instance )
			SetTip( instance.firstTipIndex );
	}

	public static void ShowSecondTip()
	{
		if( instance )
			SetTip( instance.secondTipIndex );
	}

	public static void ShowNextTip()
	{
		if( instance )
			instance.internalShowNextTip();
	}

	private void internalShowNextTip()
	{
		int randomIndex = NextRandomIndex();

		if( GameAgent.GetCurrentState() != GameAgent.State.Finished )
		{
			while( randomIndex == firstTipIndex || randomIndex == secondTipIndex )
				randomIndex = NextRandomIndex();
		}
		else
		{
			if( AnalyticsAgent.GetNumPrints() == 1 )
				randomIndex = secondTipIndex;
		}

		SetTip( randomIndex );
	}

	public static void SetTip( int index )
	{
		if( instance )
			instance.internalSetTip( index );
	}

	private void internalSetTip( int index )
	{
		if( tipText && index >= 0 && index < tips.Length )
			tipText.text = tips[ index ];

		SetTipEnabled( true );
	}

	public static void SetTipEnabled( bool enabled )
	{
		if( instance )
			instance.internalSetTipEnabled( enabled );
	}

	private void internalSetTipEnabled( bool enabled )
	{
		if( tipText )
			tipText.enabled = enabled;
	}

	private int NextRandomIndex()
	{
		if( deck.Count == 0 )
		{
			for( int i = 0; i < tips.Length; i++ )
				deck.Add( i );
			
			int randomValue;
			int temp;
			
			for( int i = 0; i < deck.Count; i++ )
			{
				randomValue = Random.Range( 0, deck.Count );
				temp = deck[i];
				deck[i] = deck[randomValue];
				deck[randomValue] = temp;
			}
		}

		int nextRandomIndex = deck[0];

		deck.RemoveAt( 0 );

		return nextRandomIndex;
	}
}
