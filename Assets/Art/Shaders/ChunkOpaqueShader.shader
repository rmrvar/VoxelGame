Shader "Terrain/ChunkOpaqueTexture"
{
    Properties
    {
        _TextureArray ("Texture Array", 2DArray) = "" {}
        _GrassSideTexture ("Grass Side Texture", 2D) = "white" {}
        _GrassSideTextureIndex ("Grass Side Texture Index", Int) = 0
        _GrassTint ("Grass Tint", Color) = (0.35, 0.75, 0.20, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct VertIn
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 uv         : TEXCOORD0;
            };

            struct FragIn
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float lighting      : TEXCOORD1;
                int textureIndex    : TEXCOORD2;
                bool isOverlaySide  : TEXCOORD3;
                half4 baseTint      : TEXCOORD4;
                half4 overTint      : TEXCOORD5;
            };

            TEXTURE2D_ARRAY(_TextureArray);
            SAMPLER(sampler_TextureArray);

            TEXTURE2D(_GrassSideTexture);
            SAMPLER(sampler_GrassSideTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _GrassTint;
                int _GrassSideTextureIndex;
            CBUFFER_END

            FragIn vert(VertIn input)
            {
                FragIn o;

                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                o.textureIndex = (int)(input.uv.z + 0.01);

                bool shouldTint = (int)(input.uv.w + 0.01);
                bool isSide = o.textureIndex % 3 == 0;
                bool isTop = (o.textureIndex - 1) % 3 == 0;
                bool shouldTintOver = shouldTint && isSide;
                bool shouldTintBase = shouldTint && isTop;
                o.baseTint = lerp(1, _GrassTint, shouldTintBase);
                o.overTint = lerp(1, _GrassTint, shouldTintOver);
                o.isOverlaySide = shouldTintOver;

                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                Light mainLight = GetMainLight();
                float3 lightVectorWS = mainLight.direction;
                float lighting = saturate(dot(normalWS, lightVectorWS));
                o.lighting = lerp(0.3, 1.0, lighting);

                return o;
            }


            half4 frag(FragIn i) : SV_Target
            {
                half4 base = SAMPLE_TEXTURE2D_ARRAY(
                    _TextureArray,
                    sampler_TextureArray,
                    i.uv.xy,
                    i.textureIndex
                  );
                half4 over = SAMPLE_TEXTURE2D(
                    _GrassSideTexture,
                    sampler_GrassSideTexture,
                    i.uv.xy
                  );
                half3 combined = lerp(
                    base * i.baseTint, 
                    over * i.overTint, 
                    over.a * i.isOverlaySide
                  );

                half3 rgb = combined * i.lighting;

                return half4(rgb, 1);
            }

            ENDHLSL
        }
    }
}
