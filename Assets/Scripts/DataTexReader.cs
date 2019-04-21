using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataTexReader : MonoBehaviour {

    public Texture2D texture;
    public bool read = false;
    public bool write = false;

    void Start()
    {
        if( texture )
        {
            if( read )
            {
                Color[] pixels = texture.GetPixels();

                for( int i = 0; i < pixels.Length; i++ )
                {
                    Debug.Log( "" + i + ": " + pixels[ i ] );
                }
            }

            if( write )
            {
                Texture2D texture2 = new Texture2D( 3, 4 );

                //Left to Right, Bottom to Top
                Color[] pixels = 
                {
                    Color.clear,
                    new Color( 1f, 1f, 1f, 0.5f ),
                    Color.clear,
                    Color.clear,
                    Color.clear,
                    Color.clear,
                    Color.clear,
                    new Color( 1f, 1f, 1f, 0.25f ),
                    new Color( 1f, 0f, 0f, 0.75f ),
                    new Color( 1f, 1f, 1f, 0f ),
                    Color.clear,
                    Color.clear
                };

                texture2.SetPixels( pixels );
                texture2.Apply();

                pixels = texture2.GetPixels();

                for( int i = 0; i < pixels.Length; i++ )
                {
                    Debug.Log( "" + i + ": " + pixels[ i ] );
                }
            }

            //if( read )
            //{
            //    Color[] pixels = texture.GetPixels();

            //    for( int i = 0; i < pixels.Length; i++ )
            //    {
            //        Debug.Log( "" + i + ": " + pixels[ i ] );
            //    }
            //}
        }
    }

    
}
