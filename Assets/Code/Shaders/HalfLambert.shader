Shader "Unlit/HalfLambert"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        [Normal] _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", float) = 1.0
        _Scale("Texture Scale", float) = 1
        _HighColor("High Color", Color) = (1, 1, 1, 1)
        _LowColor("Low Color", Color) = (0.8, 0.8, 0.8, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        Pass
        {
            Tags{"LightMode" = "UniversalForward"}
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #define _MAIN_LIGHT_SHADOWS 
            #define _MAIN_LIGHT_SHADOWS_CASCADE 
            #define _ADDITIONAL_LIGHTS 
            #define _ADDITIONAL_LIGHT_SHADOWS 
            #define _SHADOWS_SOFT 
            
            #include "HalfLambert.hlsl"
            ENDHLSL
        }
        
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormalsOnly"
            Tags{"LightMode" = "DepthNormalsOnly"}

            ZWrite On

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5

            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT // forward-only variant

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitDepthNormalsPass.hlsl"
            ENDHLSL
        }
    }
}