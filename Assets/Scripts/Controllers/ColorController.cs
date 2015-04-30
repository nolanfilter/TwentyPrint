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
	}
	
	void OnDisable()
	{
		ColorAgent.UnregisterColorController( this );
	}

	public void SetColor( Color color )
	{
		switch( uiType )
		{
			case UIType.UIImage: GetComponent<Image>().color = color; break;
			case UIType.UIText: GetComponent<Text>().color = color; break;
			case UIType.UICamera: GetComponent<Camera>().backgroundColor = color; break;
		}
	}
}
