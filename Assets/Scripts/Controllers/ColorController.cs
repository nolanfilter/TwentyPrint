using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorController : MonoBehaviour {

	public ColorAgent.ColorType colorType = ColorAgent.ColorType.Invalid;

	private enum UIType
	{
		UIImage = 0,
		UIText = 1,
		UICamera = 2,
		Invalid = 3,
	}
	private UIType uiType = UIType.Invalid;

	void Awake()
	{
		if( GetComponent<Image>() )
			uiType = UIType.UIImage;

		if( GetComponent<Text>() )
			uiType = UIType.UIText;

		if( GetComponent<Camera>() )
			uiType = UIType.UICamera;

		if( uiType == UIType.Invalid )
			enabled = false;
	}
	
	void OnEnable()
	{
		ColorAgent.RegisterColorController( this );
		SetColor( ColorAgent.GetCurrentColorPack().TypeToColor( colorType ) );
	}
	
	void OnDisable()
	{
		ColorAgent.UnregisterColorController( this );
	}

	public void SetColor( Color color )
	{
		if( colorType == ColorAgent.ColorType.Foreground && ( color == ColorAgent.RainbowColor || color == ColorAgent.RandomColor ) )
			color = ( ColorAgent.GetCurrentColorPack().backgroundColor == Color.white ? Color.black : Color.white );

		switch( uiType )
		{
			case UIType.UIImage: GetComponent<Image>().color = new Color( color.r, color.g, color.b, GetComponent<Image>().color.a ); break;
			case UIType.UIText: GetComponent<Text>().color = new Color( color.r, color.g, color.b, GetComponent<Text>().color.a ); break;
			case UIType.UICamera: GetComponent<Camera>().backgroundColor = new Color( color.r, color.g, color.b, GetComponent<Camera>().backgroundColor.a ); break;
		}
	}
}
