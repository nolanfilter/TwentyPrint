using UnityEngine;
using System.Collections;

public class ColorAgent : MonoBehaviour {

	public enum ColorPackType
	{
		Classic = 0,
		BlackOnWhite = 1,
		RainbowOnBlack = 2,
		Invalid = 3,
	}

	public struct ColorPack
	{
		public Color foregroundColor { get; private set; }
		public Color backgroundColor { get; private set; }

		public ColorPack( Color newForegroundColor, Color newBackgroundColor )
		{
			foregroundColor = newForegroundColor;
			backgroundColor = newBackgroundColor;
		}
	}

	private ColorPack[] colorPacks;
	private int colorPackIndex = 0;
	private string colorPackIndexString = "ColorPackIndex";

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

		colorPacks = new ColorPack[ 3 ];

		colorPacks[ (int)ColorPackType.Classic ] = new ColorPack( Color.white, new Color( 0f, 0.5f, 0.75f ) );
		colorPacks[ (int)ColorPackType.BlackOnWhite ] = new ColorPack( Color.black, Color.white );
		colorPacks[ (int)ColorPackType.RainbowOnBlack ] = new ColorPack( RainbowColor, Color.black );

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
}
