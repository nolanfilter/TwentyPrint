using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ColorAgent : MonoBehaviour {

	public enum ColorPackType
	{
		Classic = 0,
		BlackOnWhite = 1,
		RainbowOnBlack = 2,
		RandomOnSpacetime = 3,
		Invalid = 4,
	}

	public enum ColorType
	{
		Foreground = 0,
		Background = 1,
		Mid = 2,
		Invalid = 3,
	}

	public struct ColorPack
	{
		public Color foregroundColor { get; private set; }
		public Color backgroundColor { get; private set; }
		public Color midColor { get; private set; }

		public ColorPack( Color newForegroundColor, Color newBackgroundColor )
		{
			foregroundColor = newForegroundColor;
			backgroundColor = newBackgroundColor;

			Color counterColor = Color.white;

			if( new Vector3( backgroundColor.r, backgroundColor.g, backgroundColor.b ).magnitude > halfMagnitude )
				counterColor = Color.black;

			midColor = Color.Lerp( backgroundColor, counterColor, 0.5f );
		}

		public Color TypeToColor( ColorType colorType )
		{
			switch( colorType )
			{
				case ColorType.Foreground: return foregroundColor;
				case ColorType.Background: return backgroundColor;
				case ColorType.Mid: return midColor;
			}

			return Color.clear;
		}
	}

	private ColorPack[] colorPacks;
	private int colorPackIndex = 0;
	private string colorPackIndexString = "ColorPackIndex";

	private Color[] currentColors;
	private List<ColorController>[] colorControllers;

	private float fadeDuration = 0.2f;

	public static float halfMagnitude = Mathf.Sqrt( 3f ) * 0.5f;

	public static Color RandomColor = new Color( -1f, 0f, 0f );
	public static Color RainbowColor = new Color( -1f, -1f, -1f );

	private static ColorAgent mInstance;
	public static ColorAgent instance
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
			Debug.LogError( "Only one instance of ColorAgent allowed. Destroying " + gameObject + " and leaving " + mInstance.gameObject );
			Destroy( gameObject );
			return;
		}

		mInstance = this;

		colorPacks = new ColorPack[ Enum.GetNames( typeof( ColorPackType ) ).Length - 1 ];

		colorPacks[ (int)ColorPackType.Classic ] = new ColorPack( Color.white, new Color( 0f, 0.5f, 0.75f ) );
		colorPacks[ (int)ColorPackType.BlackOnWhite ] = new ColorPack( Color.black, Color.white );
		colorPacks[ (int)ColorPackType.RainbowOnBlack ] = new ColorPack( RainbowColor, Color.black );
		colorPacks[ (int)ColorPackType.RandomOnSpacetime ] = new ColorPack( RandomColor, new Color( 0.06f, 0.1f, 0.14f ) );

		currentColors = new Color[  Enum.GetNames( typeof( ColorType ) ).Length - 1 ];
		colorControllers = new List<ColorController>[ Enum.GetNames( typeof( ColorType ) ).Length - 1 ];

		for( int i = 0; i < colorControllers.Length; i++ )
			colorControllers[i] = new List<ColorController>();

		/* if( PlayerPrefs.HasKey( colorPackIndexString ) )
			colorPackIndex = PlayerPrefs.GetInt( colorPackIndexString );
		else */
			colorPackIndex = 0;
	}

	public static ColorPack GetCurrentColorPack()
	{
		if( instance )
			return instance.internalGetCurrentColorPack();
		
		return new ColorPack( Color.magenta, Color.green );
	}
	
	private ColorPack internalGetCurrentColorPack()
	{
		return colorPacks[ colorPackIndex ];
	}

	public static void SetColorPack( ColorPackType newType )
	{
		if( instance )
			instance.internalSetColorPack( newType );
	}
	
	private void internalSetColorPack( ColorPackType newType )
	{
		colorPackIndex = (int)newType;
		
		//PlayerPrefs.SetInt( colorPackIndexString, colorPackIndex );
	}

	public static void AdvanceColorPack()
	{
		if( instance )
			instance.internalAdvanceColorPack();
	}

	private void internalAdvanceColorPack()
	{
		SetColorPack( (ColorPackType)( ( colorPackIndex + 1 )%colorPacks.Length ) );
	}

	public static void RegisterColorController( ColorController colorController )
	{
		if( instance )
			instance.internalRegisterColorController( colorController );
	}

	private void internalRegisterColorController( ColorController colorController )
	{
		if( !colorControllers[ (int)colorController.colorType ].Contains( colorController ) )
			colorControllers[ (int)colorController.colorType ].Add( colorController );
	}

	public static void UnregisterColorController( ColorController colorController )
	{
		if( instance )
			instance.internalUnregisterColorController( colorController );
	}
	
	private void internalUnregisterColorController( ColorController colorController )
	{
		if( colorControllers[ (int)colorController.colorType ].Contains( colorController ) )
			colorControllers[ (int)colorController.colorType ].Remove( colorController );
	}

	public static void UpdateColor( ColorType colorType )
	{
		if( instance )
			instance.internalUpdateColor( colorType );
	}

	private void internalUpdateColor( ColorType colorType )
	{
		int colorIndex = (int)colorType;
		Color color = GetCurrentColorPack().TypeToColor( colorType );

		if( currentColors[ colorIndex ] == color )
			return;

		if( colorType == ColorType.Foreground && ( color == RainbowColor || color == RandomColor ) )
			color = ( GetCurrentColorPack().backgroundColor == Color.white ? Color.black : Color.white );

		/*
		switch( colorType )
		{
			case ColorType.Foreground:
			{
				if( color == RainbowColor || color == RandomColor )
					color = ( GetCurrentColorPack().backgroundColor == Color.white ? Color.black : Color.white );
					         
				for( int i = 0; i < colorControllers[ colorIndex ].Count; i++ )
					colorControllers[ colorIndex ][i].SetColor( color );
			} break;

			case ColorType.Background:
			{
				StopCoroutine( "DoBackgroundFade" );
				StartCoroutine( "DoBackgroundFade", color );
			} break;

			case ColorType.Mid:
			{
				for( int i = 0; i < colorControllers[ colorIndex ].Count; i++ )
					colorControllers[ colorIndex ][i].SetColor( color );
			} break;
		}
		*/

		for( int i = 0; i < colorControllers[ colorIndex ].Count; i++ )
			colorControllers[ colorIndex ][i].SetColor( color );

		currentColors[ colorIndex ] = color;
	}

	private IEnumerator DoBackgroundFade( Color toColor )
	{	
		int backgroundIndex = (int)ColorType.Background;

		Color fromColor = currentColors[ backgroundIndex ];
		float currentTime = 0f;
		float lerp;
		Color color;
		
		do
		{
			currentTime += Time.deltaTime;
			lerp = Mathf.Clamp01( currentTime / fadeDuration );

			lerp = 3f * Mathf.Pow( lerp, 2f ) - 2f * Mathf.Pow( lerp, 3f );
			
			color = Color.Lerp( fromColor, toColor, lerp );

			for( int i = 0; i < colorControllers[ backgroundIndex ].Count; i++ )
				colorControllers[ backgroundIndex ][i].SetColor( color );
			
			yield return null;
			
		} while( currentTime < fadeDuration );

		for( int i = 0; i < colorControllers[ backgroundIndex ].Count; i++ )
			colorControllers[ backgroundIndex ][i].SetColor( toColor );
	}
}
