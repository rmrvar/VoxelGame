Shader "Terrain/ChunkCutoutShader"
{
    Properties
    {
        _TextureArray ("Texture Array", 2DArray) = "" {}
	    
    	[Enum(UnityEngine.Rendering.CullMode)]
	    _ShouldCull ("Should Cull", Float) = 2
    	
        _Tint ("Tint", Color) = (0.35, 0.75, 0.20, 1)
    }

    SubShader
    {
    	Tags
		{
		    "Queue" = "AlphaTest"
		    "RenderType" = "TransparentCutout"
		}

        Pass
        {
			Cull [_ShouldCull]
        	
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct FragIn
            {
                float4 positionCS : SV_POSITION;
                float4 uv : TEXCOORD0;
                float lighting : TEXCOORD1;
            };

            TEXTURE2D_ARRAY(_TextureArray);
            SAMPLER(sampler_TextureArray);

            CBUFFER_START(UnityPerMaterial)
                float4 _Tint;
            CBUFFER_END

            FragIn vert(
                float4 positionOS : POSITION,
                float3 normalOS : NORMAL,
                float4 uv : TEXCOORD0
              )
            {
                FragIn o;

                o.positionCS = TransformObjectToHClip(positionOS.xyz);
                o.uv = uv;

                float3 normalWS = TransformObjectToWorldNormal(normalOS);

                Light mainLight = GetMainLight();
                float3 lightVectorWS = mainLight.direction;

                o.lighting = dot(normalWS, lightVectorWS);

                return o;
            }

            half4 frag(
			    FragIn i,
			    bool isFrontFace : SV_IsFrontFace
			  ) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D_ARRAY(
                    _TextureArray,
                    sampler_TextureArray,
                    i.uv.xy,
                    i.uv.z
                  );

                clip(tex.a - 0.1);

                float actualLighting = i.lighting;
			    if (!isFrontFace)
			    {
			        actualLighting = -actualLighting;
			    }
                actualLighting = lerp(0.3, 1, saturate(actualLighting));

                half3 base = tex.rgb;
                bool shouldTint = i.uv.w;
                half3 tint = lerp(1, _Tint, shouldTint);
                half3 rgb = base * tint * actualLighting;

                return half4(rgb, 1);
            }

            ENDHLSL
        }
    }
}
