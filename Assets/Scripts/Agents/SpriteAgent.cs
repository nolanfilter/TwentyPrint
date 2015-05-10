using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SpriteAgent : MonoBehaviour {

	public Sprite[] sprites;
	public string[] freeSprites;
	public StoreSpriteController[] storeSpriteControllers;
	public TouchDownCallback[] callbacks;
	public Text instructionText;

	private int spriteIndex = 0;
	private string spriteIndexString = "SpriteIndex";

	private Dictionary<string, bool> spritesUnlocked;
	private List<int> unlockedSpriteIndices;

	private List<string> spritesUsed;

	private int randomSpriteIndex = -1;

	private float outlineWidth = 2.5f;
	private float highlightWidth = 10f;

	private bool hasWatchedAd = false;

	private static SpriteAgent mInstance;
	public static SpriteAgent instance
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
			Debug.LogError( "Only one instance of SpriteAgent allowed. Destroying " + gameObject + " and leaving " + mInstance.gameObject );
			Destroy( gameObject );
			return;
		}
		
		mInstance = this;

		spritesUnlocked = new Dictionary<string, bool>();
		unlockedSpriteIndices = new List<int>();
		spritesUsed = new List<string>();

		for( int i = 0; i < sprites.Length; i++ )
		{
			string spriteName = sprites[i].name;

			if( !PlayerPrefs.HasKey( spriteName ) )
				PlayerPrefs.SetInt( spriteName, ( System.Array.IndexOf( freeSprites, sprites[i].name ) != -1 ? 1 : 0 ) );

			bool spriteUnlocked = ( PlayerPrefs.GetInt( spriteName ) == 1 );

			spritesUnlocked.Add( spriteName, spriteUnlocked );

			if( i < storeSpriteControllers.Length )
			{
				storeSpriteControllers[i].sprite.sprite = sprites[i];

				storeSpriteControllers[i].outline.rectTransform.offsetMin = Vector2.one * outlineWidth * -1f;
				storeSpriteControllers[i].outline.rectTransform.offsetMax = Vector2.one * outlineWidth;

				if( !spritesUnlocked[ spriteName ] )
				{
					storeSpriteControllers[i].sprite.color = new Color( storeSpriteControllers[i].sprite.color.r, storeSpriteControllers[i].sprite.color.g, storeSpriteControllers[i].sprite.color.b, 0.5f );
					storeSpriteControllers[i].background.color = new Color( storeSpriteControllers[i].background.color.r, storeSpriteControllers[i].background.color.g, storeSpriteControllers[i].background.color.b, 0.5f );
					storeSpriteControllers[i].outline.color = new Color( storeSpriteControllers[i].outline.color.r, storeSpriteControllers[i].outline.color.g, storeSpriteControllers[i].outline.color.b, 0.5f );
				}
				else
				{
					unlockedSpriteIndices.Add( i );
				}
			}
		}

		CheckAllSpritesUnlocked();

		if( !PlayerPrefs.HasKey( spriteIndexString ) )
			PlayerPrefs.SetInt( spriteIndexString, 23 );

		SetSpriteIndex( PlayerPrefs.GetInt( spriteIndexString ) );
	}

	void OnEnable()
	{
		for( int i = 0; i < callbacks.Length; i++ )
			callbacks[i].OnAreaTouchWithCallback += OnSpriteAreaTouch;
	}

	void OnDisable()
	{
		for( int i = 0; i < callbacks.Length; i++ )
			callbacks[i].OnAreaTouchWithCallback -= OnSpriteAreaTouch;
	}

	private void OnSpriteAreaTouch( TouchDownCallback callback )
	{
		int newSpriteIndex = System.Array.IndexOf( callbacks, callback );

		if( newSpriteIndex >= 0 && newSpriteIndex < sprites.Length )
		{
			string spriteName = sprites[ newSpriteIndex ].name;

			if( spritesUnlocked.ContainsKey( spriteName ) )
			{
				if( spritesUnlocked[ spriteName ] )
				{
					SetSpriteIndex( newSpriteIndex );
				}
				else
				{
					StopCoroutine( "WaitForAdSuccess" );
					StartCoroutine( "WaitForAdSuccess", newSpriteIndex );

					AdAgent.ShowIncentivizedVideo();
				}
			}
		}
		else if( newSpriteIndex == callbacks.Length - 1 )
		{
			SetSpriteIndex( newSpriteIndex );
		}
	}

	public static Sprite GetCurrentSprite()
	{
		if( instance )
			return instance.internalGetCurrentSprite();

		return null;
	}

	private Sprite internalGetCurrentSprite()
	{
		if( spriteIndex >= 0 )
		{
			if( spriteIndex < callbacks.Length - 1 )
				return sprites[ spriteIndex ];

			if( spriteIndex == callbacks.Length - 1 )
				return sprites[ randomSpriteIndex ];
		}

		return null;
	}

	public static void Randomize()
	{
		if( instance )
			instance.internalRandomize();
	}

	private void internalRandomize()
	{
		randomSpriteIndex = unlockedSpriteIndices[ Random.Range( 0, unlockedSpriteIndices.Count ) ];
	}

	public static void UnlockSpriteByName( string name )
	{
		if( instance )
			instance.internalUnlockSpriteByName( name );
	}

	private void internalUnlockSpriteByName( string name )
	{
		for( int i = 0; i < sprites.Length; i++ )
		{
			if( sprites[i].name == name )
			{
				UnlockSprite( i );
				return;
			}
		}
	}

	public static void UnlockAllSprites()
	{
		if( instance )
			instance.internalUnlockAllSprites();
	}

	private void internalUnlockAllSprites()
	{
		for( int i = 0; i < sprites.Length; i++ )
			UnlockSprite( i );

		PreferencesAgent.UpdateICloud();

		if( instructionText )
			instructionText.enabled = false;
	}

	public static void WatchedAd()
	{
		if( instance )
			instance.hasWatchedAd = true;
	}

	public static void DidNotWatchAd()
	{
		if( instance )
			instance.StopCoroutine( "WaitForAdSuccess" );
	}

	public static Dictionary<string, bool> GetSpritesUnlockedDictionary()
	{
		if( instance )
			return instance.spritesUnlocked;

		return null;
	}

	public static void LogSpriteName()
	{
		if( instance )
			instance.internalLogSpriteName();
	}

	private void internalLogSpriteName()
	{
		string name = sprites[ spriteIndex ].name;

		if( !spritesUsed.Contains( name ) )
			spritesUsed.Add( name );
	}

	public static List<string> GetSpritesUsed()
	{
		if( instance )
			return instance.spritesUsed;

		return null;
	}

	public static void ClearSpriteNames()
	{
		if( instance )
			instance.spritesUsed.Clear();
	}

	private void SetSpriteIndex( int newSpriteIndex )
	{
		storeSpriteControllers[ spriteIndex ].outline.rectTransform.offsetMin = Vector2.one * outlineWidth * -1f;
		storeSpriteControllers[ spriteIndex ].outline.rectTransform.offsetMax = Vector2.one * outlineWidth;

		spriteIndex = newSpriteIndex;
		PlayerPrefs.SetInt( spriteIndexString, spriteIndex );

		storeSpriteControllers[ spriteIndex ].outline.rectTransform.offsetMin = Vector2.one * highlightWidth * -1f;
		storeSpriteControllers[ spriteIndex ].outline.rectTransform.offsetMax = Vector2.one * highlightWidth;
	}

	private IEnumerator WaitForAdSuccess( int spriteIndexToUnlock )
	{
		hasWatchedAd = false;

		while( !hasWatchedAd )
			yield return null;

		UnlockSprite( spriteIndexToUnlock );

		SetSpriteIndex( spriteIndexToUnlock );

		AnalyticsAgent.LogAnalyticEvent( AnalyticsAgent.AnalyticEvent.UnlockSprite );
		PreferencesAgent.UpdateICloud();

		CheckAllSpritesUnlocked();
	}

	private void UnlockSprite( int spriteIndexToUnlock )
	{
		string spriteName = sprites[ spriteIndexToUnlock ].name;

		if( spritesUnlocked[ spriteName ] )
			return;

		spritesUnlocked[ spriteName ] = true;
		PlayerPrefs.SetInt( spriteName, 1 );
		
		unlockedSpriteIndices.Add( spriteIndexToUnlock );
		
		storeSpriteControllers[ spriteIndexToUnlock ].sprite.color = new Color( storeSpriteControllers[ spriteIndexToUnlock ].sprite.color.r, storeSpriteControllers[ spriteIndexToUnlock ].sprite.color.g, storeSpriteControllers[ spriteIndexToUnlock ].sprite.color.b, 1f );
		storeSpriteControllers[ spriteIndexToUnlock ].background.color = new Color( storeSpriteControllers[ spriteIndexToUnlock ].background.color.r, storeSpriteControllers[ spriteIndexToUnlock ].background.color.g, storeSpriteControllers[ spriteIndexToUnlock ].background.color.b, 1f );
		storeSpriteControllers[ spriteIndexToUnlock ].outline.color = new Color( storeSpriteControllers[ spriteIndexToUnlock ].outline.color.r, storeSpriteControllers[ spriteIndexToUnlock ].outline.color.g, storeSpriteControllers[ spriteIndexToUnlock ].outline.color.b, 1f );
	}

	private void CheckAllSpritesUnlocked()
	{
		for( int i = 0; i < sprites.Length; i++ )
		{
			string name = sprites[i].name;

			if( spritesUnlocked.ContainsKey( name ) && !spritesUnlocked[ name ] )
				return;
		}

		if( instructionText )
			instructionText.enabled = false;
	}
}