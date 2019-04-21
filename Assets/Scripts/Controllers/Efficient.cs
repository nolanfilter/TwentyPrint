using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Efficient : MonoBehaviour {

    public Texture2D[] textures;

    private Texture2D texture;
    private Texture2DArray textureArray;

    private Material material = null;

    private float boardWidth = 25f;
    private float boardHeight = 34f;

    void Start()
    {
        boardHeight = ( boardWidth / (float)Screen.width * (float)Screen.height );

        if( texture == null )
        {
            texture = new Texture2D( Mathf.CeilToInt( boardWidth ), Mathf.CeilToInt( boardHeight ), TextureFormat.RGBA32, false, false );
            texture.filterMode = FilterMode.Point;
        }

        if( textureArray == null )
        {
            textureArray = new Texture2DArray( 64, 64, textures.Length, TextureFormat.RGBA32, false );
            textureArray.filterMode = FilterMode.Bilinear;
            textureArray.wrapMode = TextureWrapMode.Clamp;

            for( int i = 0; i < textures.Length; i++ )
            {
                textureArray.SetPixels( textures[ i ].GetPixels(), i );
            }

            textureArray.Apply();
        }

        //Left to Right, Bottom to Top
        //Color[] pixels =
        //{
        //    Color.clear,
        //    new Color( 1f, 1f, 1f, 0.5f ),
        //    Color.clear,
        //    Color.clear,
        //    Color.clear,
        //    Color.clear,
        //    Color.clear,
        //    new Color( 1f, 1f, 1f, 0.25f ),
        //    new Color( 1f, 0f, 0f, 0.75f ),
        //    new Color( 1f, 1f, 1f, 0f ),
        //    Color.clear,
        //    Color.clear
        //};

        Reset();

        //pixels = texture.GetPixels();

        //for( int i = 0; i < pixels.Length; i++ )
        //{
        //    Debug.Log( "" + i + ": " + pixels[ i ] );
        //}

        if( material == null )
        {
            material = GetComponent<RawImage>().material;
        }
       
        if( material != null )
        {
            material.SetTexture( "_DataTex", texture );
            material.SetTexture( "_SpriteTexArray", textureArray );

            material.SetFloat( "_BoardWidth", boardWidth );
            material.SetFloat( "_BoardHeight", boardHeight );
        }
    }

    private void Reset()
    {
        Color[] pixels = new Color[ Mathf.CeilToInt( boardWidth ) * Mathf.CeilToInt( boardHeight ) ];

        float colorOffset = Random.Range( 0f, 360f );

        bool shouldDisplay = false;
        int row = 0;

        for( int i = 0; i < pixels.Length; i++ )
        {
            shouldDisplay = ( Random.value < 0.75f );

            if( shouldDisplay )
            {
                row = i / Mathf.CeilToInt( boardWidth );

                pixels[ i ] = Utilities.ColorFromHSV( ( ( row / (float)BoardAgent.BoardHeight ) * 360f + colorOffset ) % 360f, 1f, 1f );

                pixels[ i ] = new Color( pixels[ i ].r, pixels[ i ].g, pixels[ i ].b, Random.Range( 0, 17 ) / 16f );
            }
            else
            {
                pixels[ i ] = Color.black;
            }
        }

        texture.SetPixels( pixels );
        texture.Apply();
    }

    void Update()
    {
        if( Input.GetKeyDown( KeyCode.Space ) )
        {
            Reset();
        }
    }
}
