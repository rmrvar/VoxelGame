// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Example/Sample2DArrayTexture"
{
    Properties
    {
        _TextureArray ("Texture Array", 2DArray) = "" {}
    }
    SubShader
    {
		Pass
		{
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

		    v2f vert(float4 positionOS : POSITION, float3 normalOS : NORMAL, float3 uv : TEXCOORD0)
		    {
		        v2f o;

		        o.positionCS = TransformObjectToHClip(positionOS.xyz);
		        o.uv = uv;

				float3 normalWS = TransformObjectToWorldNormal(normalOS);

				Light mainLight = GetMainLight();
				float3 lightVectorWS = mainLight.direction;

		        float lighting = saturate(dot(normalWS, lightVectorWS));

				o.lighting = float2(lerp(0.3, 1, lighting), 0);

		        return o;
		    }

		    half4 frag(v2f i) : SV_Target
		    {
		        float3 color =
		            SAMPLE_TEXTURE2D_ARRAY(
		                _TextureArray,
		                sampler_TextureArray,
		                i.uv.xy,
		                i.uv.z
		            ).rgb;

		        return half4(color * i.lighting, 1);
		    }

		    ENDHLSL
		}
    }
}