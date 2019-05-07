using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class Efficient : MonoBehaviour {

    public Texture2D[] textures;

    private Texture2D dataTexture;
    private Texture2DArray textureArray;

    private Material material = null;

    private int boardWidth = 25;
    private int boardHeight = 34;
    private int boardSize = 850;

    private float colorOffset;
    
    private float speed;
    private float fillDuration = 10f;

    void Start()
    {
        boardHeight = Mathf.RoundToInt( boardWidth / (float)Screen.width * (float)Screen.height );

        boardSize = boardWidth * boardHeight;
        
        if( dataTexture == null )
        {
            dataTexture = new Texture2D( Mathf.CeilToInt( boardWidth ), Mathf.CeilToInt( boardHeight ), TextureFormat.RGBA32, false, false );
            dataTexture.filterMode = FilterMode.Point;
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
            material.SetTexture( "_DataTex", dataTexture );
            material.SetTexture( "_SpriteTexArray", textureArray );

            material.SetFloat( "_BoardWidth", boardWidth );
            material.SetFloat( "_BoardHeight", boardHeight );
        }
    }

    private void Reset()
    {
        Color[] pixels = new Color[ Mathf.CeilToInt( boardWidth ) * Mathf.CeilToInt( boardHeight ) ];

        colorOffset = Random.Range( 0f, 360f );

        bool shouldDisplay = false;
        int row = 0;

        for( int i = 0; i < pixels.Length; i++ )
        {
            pixels[ i ] = Color.black;
            
//            shouldDisplay = ( Random.value < 0.75f );
//
//            if( shouldDisplay )
//            {
//                row = i / Mathf.CeilToInt( boardWidth );
//
//                pixels[ i ] = Utilities.ColorFromHSV( ( ( row / (float)BoardAgent.BoardHeight ) * 360f + colorOffset ) % 360f, 1f, 1f );
//
//                pixels[ i ] = new Color( pixels[ i ].r, pixels[ i ].g, pixels[ i ].b, Random.Range( 0, 17 ) / 16f );
//            }
//            else
//            {
//                pixels[ i ] = Color.black;
//            }
        }

        dataTexture.SetPixels( pixels );
        dataTexture.Apply();
    }

    void Update()
    {
        if( Input.GetKeyDown( KeyCode.Space ) )
        {
            Reset();
            StartCoroutine( DoFill() );
        }
    }

    private IEnumerator DoFill()
    {
        int index = 0;
        float currentDistance = 0f;

        speed = boardSize / fillDuration;
        
        while( index < boardSize )
        {
            currentDistance += speed * Time.deltaTime;

            while( currentDistance >= 1f )
            { 
                SinglePrint( index );

                index++;
                currentDistance -= 1f;

                if( index == boardSize )
                    break;
            }

            yield return null;
        }
    }

    private void SinglePrint( int index )
    {
        if( dataTexture == null )
            return;

        int row = index % boardWidth;
        int col = index / boardWidth;

        dataTexture.SetPixel( row, col, GetDataColor( Utilities.ColorFromHSV( ( ( row / (float)boardHeight ) * 360f + colorOffset ) % 360f, 1f, 1f ), Random.Range( 0, 5 ), Random.Range( 0, 5 ) ) );
        dataTexture.Apply();
    }

    private Color GetDataColor( Color color, int spriteIndex, int reflection )
    {
        return new Color( color.r, color.g, color.b, ( spriteIndex * reflection ) / 16f );
    }
}
