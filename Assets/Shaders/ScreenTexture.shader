Shader "Custom/ScreenTexture" {
    Properties {
      [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
      _Detail ("Detail", 2D) = "gray" {}
      _Color ("Tint", Color) = (1,1,1,1)
      [MaterialToggle] _UseDetail ("Use Detail", Float) = 0
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      
      Cull Off
		
      CGPROGRAM
      #pragma surface surf Lambert
      struct Input {
          float2 uv_MainTex;
          float4 screenPos;
      };
      sampler2D _MainTex;
      sampler2D _Detail;
      fixed4 _Color;
      fixed _UseDetail;
      void surf (Input IN, inout SurfaceOutput o) {
          o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb * 5;
          o.Albedo *= _Color.rgb;
          
          if( _UseDetail != 0 )
          {
	          float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
	          o.Albedo *= tex2D (_Detail, screenUV).rgb;
          }
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }