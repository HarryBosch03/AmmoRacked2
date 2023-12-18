Shader "Unlit/FX"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_Brightness("Brightness", float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

			struct Attributes
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct Varyings
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			Varyings vert (Attributes input)
			{
				Varyings output;
				output.vertex = TransformObjectToHClip(input.vertex.xyz);
				output.color = input.color;
				return output;
			}

			float4 _Color;
			float _Brightness;
			
			half4 frag (Varyings input) : SV_Target
			{
				half4 color = _Color * input.color;
				color.rgb *= _Brightness;
				return color;
			}
			ENDHLSL
		}
	}
}