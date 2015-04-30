using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundImageController : MonoBehaviour {

	private Image backgroundImage;

	void Awake()
	{
		backgroundImage = GetComponent<Image>();

		if( backgroundImage == null )
			enabled = false;
	}

	void OnEnable()
	{
		CameraAgent.RegisterBackgroundImage( backgroundImage );
	}

	void OnDisable()
	{
		CameraAgent.UnregisterBackgroundImage( backgroundImage );
	}
}
