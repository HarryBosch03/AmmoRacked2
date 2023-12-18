Shader "Unlit/Outline"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		Cull Back

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

			float4 vert (float4 vertex : POSITION) : SV_POSITION
			{
				return TransformObjectToHClip(vertex.xyz);
			}

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);
			
			half4 frag (float4 position : SV_POSITION) : SV_Target
			{
				return 0.0;
			}
			ENDHLSL
		}
	}
}