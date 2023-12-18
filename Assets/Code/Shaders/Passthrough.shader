Shader "Unlit/Passthrough"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Greater
			ZWrite Off
			
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

			struct Attributes
			{
				float4 vertex : POSITION;
			};

			struct Varyings
			{
				float4 vertex : SV_POSITION;
			};

			Varyings vert (Attributes input)
			{
				Varyings output;
				output.vertex = TransformObjectToHClip(input.vertex.xyz);
				return output;
			}

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			
			half4 frag (Varyings input) : SV_Target
			{
				float2 uv = ComputeScreenPos(input.vertex);
				half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
				return col;
			}
			ENDHLSL
		}
	}
}