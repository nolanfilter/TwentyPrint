using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class TestPrint : MonoBehaviour {

	public Text text;

	void Start()
	{
		if( text )
			StartCoroutine( "DoPrint" );
	}

	private IEnumerator DoPrint()
	{
		int counter = 0;
		int lines = 0;

		while( lines < 30 )
		{
			text.text += ( Random.value < 0.5f ? "╲" : "╱" );
			//text.text += ( Random.value < 0.5f ? "◤" : "◥" );

			/*
			switch( Random.Range( 0, 4 ) )
			{
				case 0: text.text += "╮"; break;
				case 1: text.text += "╰"; break;
				case 2: text.text += "╯"; break;
				case 3: text.text += "╭"; break;
			}
			*/

			/*
			switch( Random.Range( 0, 4 ) )
			{
				case 0: text.text += "├"; break;
				case 1: text.text += "┴"; break;
				case 2: text.text += "┬"; break;
				case 3: text.text += "┤"; break;
			}
			*/

			/*
			switch( Random.Range( 0, 4 ) )
			{
				case 0: text.text += "└"; break;
				case 1: text.text += "┐"; break;
				case 2: text.text += "┌"; break;
				case 3: text.text += "┘"; break;
			}
			*/

			/*
			switch( Random.Range( 0, 3 ) )
			{
				case 0: text.text += "╲"; break;
				case 1: text.text += "╱"; break;
				case 2: text.text += "╳"; break;
			}
			*/

			counter++;

			if( counter == 22 )
			{
				text.text += "\n";
				counter = 0;
				lines++;
			}

			yield return null;
		}
	}
}
