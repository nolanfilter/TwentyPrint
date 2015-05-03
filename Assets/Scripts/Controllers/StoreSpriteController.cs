using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StoreSpriteController : MonoBehaviour {

	public Image sprite;
	public Image background;
	public Image outline;

	void Start()
	{
		if( sprite == null )
		{
			Debug.LogError( "No sprite on " + name );
			enabled = false;
			return;
		}

		if( background == null )
		{
			Debug.LogError( "No background on " + name );
			enabled = false;
			return;
		}

		if( outline == null )
		{
			Debug.LogError( "No outline on " + name );
			enabled = false;
			return;
		}
	}
}
