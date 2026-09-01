Shader "Terrain/ChunkCutoutGrassShader"
{
    Properties
    {
        _TextureArray ("Texture Array", 2DArray) = "" {}
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
		    Cull Off

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
                float3 normalWS : TEXCOORD1;
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
                o.normalWS = TransformObjectToWorldNormal(normalOS);

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

			    clip(tex.a - 1.0);

			    float3 normalWS = normalize(i.normalWS);

			    if (!isFrontFace)
			    {
			        normalWS = -normalWS;
			    }

			    Light mainLight = GetMainLight();
			    float3 lightVectorWS = mainLight.direction;

			    float lighting = saturate(dot(normalWS, lightVectorWS));

			    lighting = lerp(0.3, 1, lighting);

			    return half4(tex.rgb * lighting, 1);
			}

            ENDHLSL
        }
    }
}
