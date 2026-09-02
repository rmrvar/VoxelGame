Shader "Terrain/ChunkCutoutShader"
{
    Properties
    {
        _TextureArray ("Texture Array", 2DArray) = "" {}
	    
    	[Enum(UnityEngine.Rendering.CullMode)]
	    _ShouldCull ("Should Cull", Float) = 2
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

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 uv : TEXCOORD0;
                float lighting : TEXCOORD1;
            };

            TEXTURE2D_ARRAY(_TextureArray);
            SAMPLER(sampler_TextureArray);

            v2f vert(
                float4 positionOS : POSITION,
                float3 normalOS : NORMAL,
                float3 uv : TEXCOORD0
              )
            {
                v2f o;

                o.positionCS = TransformObjectToHClip(positionOS.xyz);
                o.uv = uv;

                float3 normalWS = TransformObjectToWorldNormal(normalOS);

                Light mainLight = GetMainLight();
                float3 lightVectorWS = mainLight.direction;

                o.lighting = dot(normalWS, lightVectorWS);

                return o;
            }

            half4 frag(
			    v2f i,
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

                return half4(tex.rgb * actualLighting, 1);
            }

            ENDHLSL
        }
    }
}
