Shader "Custom/Efficient"
{
    Properties
    {
        _SpriteTexArray ("Sprite Texture", 2DArray) = "" {}
        _DataTex ("Data Texture", 2D) = "white" {}
        _BoardWidth ("Board Width", Float) = 1
        _BoardHeight ("Board Height", Float) = 1
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                half2 texcoord  : TEXCOORD0;
            };
            
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                                
                return OUT;
            }
            
            UNITY_DECLARE_TEX2DARRAY(_SpriteTexArray);
            sampler2D _DataTex;
            fixed _BoardWidth;
            fixed _BoardHeight;

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 spriteCount = float2( _BoardWidth, _BoardHeight );
                float2 spriteUV = IN.texcoord * spriteCount;
                float2 sampleCoord = floor(spriteUV) / spriteCount;
            
                fixed4 data = tex2D(_DataTex, sampleCoord);
                                                                
                float info = floor(data.a * 16.0);
                                
                float reflection = floor(info * 0.25);
                float hReflection = saturate(clamp(reflection, 0, 1) + clamp(reflection, 2, 3) - 2);
                float vReflection = saturate(reflection - 1);
                
                spriteUV = fmod( spriteUV, float2(1,1) );
                spriteUV = float2((1-hReflection) * spriteUV.x + hReflection * (1-spriteUV.x),
                                  (1-vReflection) * spriteUV.y + vReflection * (1-spriteUV.y)
                );
                                
                float spriteIndex = fmod( info, 4.0 );
                                
                //fixed4 c = tex2D( _MainTex, spriteUV );
                fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_SpriteTexArray, float3(spriteUV, spriteIndex));
                c.rgb *= data.rgb;
                c.rgb *= c.a;
                
                return c;
            }
        ENDCG
        }
    }    
}