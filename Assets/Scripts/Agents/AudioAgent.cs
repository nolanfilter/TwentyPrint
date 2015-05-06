using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AudioAgent : MonoBehaviour {

	public enum SoundEffectType
	{
		Print = 0,
		ButtonTap = 1,
		Swipe = 2,
	}

	public TouchDownCallback soundsCallback;
	public Text soundsText;

	public AudioClip[] printAudioClips;
	public AudioClip buttonTapAudioClip;
	public AudioClip swipeAudioClip;

	private Dictionary<SoundEffectType, AudioSource> audioSourcesByType;

	private float fadeOutDuration = 0.3f;

	private bool isAudioOn;
	private string isAudioOnString = "isAudioOn";

	private List<int> deck;

	private static AudioAgent mInstance;
	public static AudioAgent instance
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
			Debug.LogError( "Only one instance of AudioAgent allowed. Destroying " + gameObject + " and leaving " + mInstance.gameObject );
			Destroy( gameObject );
			return;
		}
		
		mInstance = this;

		if( !PlayerPrefs.HasKey( isAudioOnString ) )
			PlayerPrefs.SetInt( isAudioOnString, 1 );

		isAudioOn = ( PlayerPrefs.GetInt( isAudioOnString ) == 1 );

		if( soundsText )
			soundsText.text = ( isAudioOn ? "Sounds on" : "Sounds off" );

		audioSourcesByType = new Dictionary<SoundEffectType, AudioSource>();
		deck = new List<int>();
	}

	void OnEnable()
	{
		if( soundsCallback )
			soundsCallback.OnAreaTouch += OnSoundsAreaTouch;
	}

	void OnDisable()
	{
		if( soundsCallback )
			soundsCallback.OnAreaTouch -= OnSoundsAreaTouch;
	}

	private void OnSoundsAreaTouch()
	{
		SetIsAudioOn( !isAudioOn );
	}

	public static void SetIsAudioOn( bool newIsAudioOn )
	{
		if( instance )
			instance.internalSetIsAudioOn( newIsAudioOn );
	}

	private void internalSetIsAudioOn( bool newIsAudioOn )
	{
		isAudioOn = newIsAudioOn;
		PlayerPrefs.SetInt( isAudioOnString, ( isAudioOn ? 1 : 0 ) );
		
		if( soundsText )
			soundsText.text = ( isAudioOn ? "Sounds on" : "Sounds off" );
		
		foreach( KeyValuePair<SoundEffectType, AudioSource> kvp in audioSourcesByType )
			audioSourcesByType[ kvp.Key ].volume = ( isAudioOn ? 1f : 0f );
	}

	public static void PlaySoundEffect( SoundEffectType soundEffectType, float length = -1f )
	{
		if( instance )
			instance.internalPlaySoundEffect( soundEffectType, length );
	}

	private void internalPlaySoundEffect( SoundEffectType soundEffectType, float length )
	{
		if( audioSourcesByType.ContainsKey( soundEffectType ) && !audioSourcesByType[ soundEffectType ].loop )
			StopSoundEffect( soundEffectType );
		
		if( !audioSourcesByType.ContainsKey( soundEffectType ) )
		{
			AudioClip clip = null;

			switch( soundEffectType )
			{
				case SoundEffectType.Print: clip = printAudioClips[ NextRandomIndex() ]; break;
				case SoundEffectType.ButtonTap: clip = buttonTapAudioClip; break;
				case SoundEffectType.Swipe: clip = swipeAudioClip; break;
			}

			if( clip == null )
				return;

			AudioSource audioSource = CameraAgent.MainCameraObject.AddComponent<AudioSource>();

			if( length == -1f )
				length = clip.length;
			else
				audioSource.loop = true;

			audioSource.clip = clip;
			audioSource.volume = ( isAudioOn ? 1f : 0f );

			audioSourcesByType.Add( soundEffectType, audioSource );
			StartCoroutine( WaitForFinish( soundEffectType, length ) );
		}

		audioSourcesByType[ soundEffectType ].Play();
	}

	public static void PauseSoundEffect( SoundEffectType soundEffectType )
	{
		if( instance )
			instance.internalPauseSoundEffect( soundEffectType );
	}

	private void internalPauseSoundEffect( SoundEffectType soundEffectType )
	{
		if( !audioSourcesByType.ContainsKey( soundEffectType ) )
			return;

		audioSourcesByType[ soundEffectType ].Pause();
	}

	public static void StopSoundEffect( SoundEffectType soundEffectType )
	{
		if( instance )
			instance.internalStopSoundEffect( soundEffectType );
	}

	private void internalStopSoundEffect( SoundEffectType soundEffectType )
	{
		if( !audioSourcesByType.ContainsKey( soundEffectType ) )
			return;
		
		AudioSource audioSource = audioSourcesByType[ soundEffectType ];
		audioSourcesByType.Remove( soundEffectType );
		Destroy( audioSource );
	}

	public static void PitchSoundEffect( SoundEffectType soundEffectType, float pitch )
	{
		if( instance )
			instance.internalPitchSoundEffect( soundEffectType, pitch );
	}

	private void internalPitchSoundEffect( SoundEffectType soundEffectType, float pitch )
	{
		if( !audioSourcesByType.ContainsKey( soundEffectType ) )
			return;

		audioSourcesByType[ soundEffectType ].pitch = pitch;
	}

	public static bool GetIsAudioOn()
	{
		if( instance )
			return instance.isAudioOn;

		return false;
	}

	private IEnumerator WaitForFinish( SoundEffectType soundEffectType, float length )
	{
		if( !audioSourcesByType.ContainsKey( soundEffectType ) )
			yield break;

		AudioSource audioSource = audioSourcesByType[ soundEffectType ];

		float currentTime = 0f;

		do
		{
			if( audioSource == null )
				yield break;

			if( audioSource.isPlaying )
				currentTime += Time.deltaTime * ( audioSource.pitch > 1f ? 5f : 1f );

			yield return null;

		} while( currentTime < length );

		audioSourcesByType.Remove( soundEffectType );

		if( audioSource.loop )
		{
			currentTime = 0f;
			float lerp;

			do
			{
				if( audioSource == null )
					yield break;

				currentTime += Time.deltaTime;
				lerp = currentTime / fadeOutDuration;

				audioSource.volume = Mathf.Lerp( ( isAudioOn ? 1f : 0f ), 0f, lerp );

				yield return null;

			} while( currentTime < fadeOutDuration );
		}

		Destroy( audioSource );
	}

	private int NextRandomIndex()
	{
		if( deck.Count == 0 )
		{
			for( int i = 0; i < printAudioClips.Length; i++ )
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
