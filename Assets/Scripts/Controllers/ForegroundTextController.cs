using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ForegroundTextController : MonoBehaviour {

	private Text foregroundText;
	
	void Awake()
	{
		foregroundText = GetComponent<Text>();
		
		if( foregroundText == null )
			enabled = false;
	}
	
	void OnEnable()
	{
		GameAgent.RegisterForegroundText( foregroundText );
	}
	
	void OnDisable()
	{
		GameAgent.UnregisterRegisterForegroundText( foregroundText );
	}
}
