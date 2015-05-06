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

	private bool isAudioOn;
	private string isAudioOnString = "isAudioOn";

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
			PlayerPrefs.SetInt( isAudioOnString, 0 );

		isAudioOn = ( PlayerPrefs.GetInt( isAudioOnString ) == 1 );

		audioSourcesByType = new Dictionary<SoundEffectType, AudioSource>();
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
		if( soundsText )
		{
			if( soundsText.text == "Sounds off" )
				soundsText.text = "Sounds on";
			else if( soundsText.text == "Sounds on" )
				soundsText.text = "Sounds off";
		}
	}

	public static void PlaySoundEffect( SoundEffectType soundEffectType, float length = -1f )
	{
		if( instance )
			instance.internalPlaySoundEffect( soundEffectType, length );
	}

	private void internalPlaySoundEffect( SoundEffectType soundEffectType, float length )
	{
		if( !isAudioOn )
			return;
	}

	private void PauseSoundEffect( SoundEffectType soundEffectType )
	{
		if( instance )
			instance.internalPauseSoundEffect( soundEffectType );
	}

	private void internalPauseSoundEffect( SoundEffectType soundEffectType )
	{

	}

	private IEnumerator WaitForFinish( float length )
	{

	}
}
