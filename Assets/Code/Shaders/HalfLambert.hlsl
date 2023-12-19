#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"


CBUFFER_START(UnityPerMaterial)

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);

TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);
float _NormalStrength;

float _Scale;
float4 _HighColor;
float4 _LowColor;

CBUFFER_END

static const float _Ambient = 0.5f;

struct Attributes
{
    float4 vertex : POSITION;
    float4 normal : NORMAL;
    float4 tangent : TANGENT;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float3 positionWS : POSITION_WS;
    float3 normalWS : NORMAL_WS;
    float3 tangentWS : TANGENT_WS;
    float3 normalOS : NORMAL_OS;
    float3 triplanar : TRIPLANAR;
    float4 color : COLOR;
};

Varyings vert(Attributes input)
{
    Varyings output;
    output.vertex = TransformObjectToHClip(input.vertex.xyz);
    output.uv = input.uv;
    output.positionWS = TransformObjectToWorld(input.vertex.xyz);
    output.normalWS = TransformObjectToWorldNormal(input.normal.xyz);
    output.tangentWS = TransformObjectToWorldNormal(input.tangent.xyz);
    output.normalOS = input.normal;

    float3 worldScale = float3
    (
        length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)),
        length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y)),
        length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z))
    );
    output.triplanar = input.vertex * worldScale * _Scale;

    output.color = input.color;
    return output;
}

float Lighting(Varyings input, Light light)
{
    return saturate(dot(light.direction, input.normalWS) * light.distanceAttenuation * light.shadowAttenuation);
}

float4 Triplanar(TEXTURE2D_PARAM(_tex, sampler_tex), Varyings input)
{
    float4 x = SAMPLE_TEXTURE2D(_tex, sampler_tex, input.triplanar.zy);
    float4 y = SAMPLE_TEXTURE2D(_tex, sampler_tex, input.triplanar.xz);
    float4 z = SAMPLE_TEXTURE2D(_tex, sampler_tex, input.triplanar.xy);

    float3 weights = pow(abs(input.normalOS.xyz), 2.0);

    return
        x * weights.x +
        y * weights.y +
        z * weights.z;
}

float3 TriplanarNormal(TEXTURE2D_PARAM(_tex, sampler_tex), Varyings input)
{
    float3 x = UnpackNormalScale(SAMPLE_TEXTURE2D(_tex, sampler_tex, input.triplanar.zy), _NormalStrength);
    float3 y = UnpackNormalScale(SAMPLE_TEXTURE2D(_tex, sampler_tex, input.triplanar.xz), _NormalStrength);
    float3 z = UnpackNormalScale(SAMPLE_TEXTURE2D(_tex, sampler_tex, input.triplanar.xy), _NormalStrength);

    x = float3(x.xy + input.normalWS.zy, abs(x.z) * input.normalWS.x);
    y = float3(y.xy + input.normalWS.xz, abs(y.z) * input.normalWS.y);
    z = float3(z.xy + input.normalWS.xy, abs(z.z) * input.normalWS.z);

    float3 weights = pow(abs(input.normalOS.xyz), 2.0);

    return normalize
    (
        x.zyx * weights.x +
        y.xzy * weights.y +
        z.xyz * weights.z
    );
}

float Lighting(Varyings input)
{
    float lighting = 0.0f;

    float4 shadowCoords = TransformWorldToShadowCoord(input.positionWS);

    Light mainLight = GetMainLight(shadowCoords);
    lighting += Lighting(input, mainLight);

    uint lightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < lightCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, input.positionWS, shadowCoords);
        lighting += Lighting(input, light);
    }

    return lighting;
}

half4 frag(Varyings input) : SV_Target
{
    input.normalWS = normalize(input.normalWS);
    input.tangentWS = normalize(input.tangentWS);
    input.normalOS = normalize(input.normalOS);

    input.normalWS = TriplanarNormal(_NormalMap, sampler_NormalMap, input);
    float attenuation = Lighting(input);

    half4 col = normalize(lerp(_LowColor, _HighColor, input.uv.y)) * lerp(length(_LowColor), length(_HighColor), input.uv.y);
    col *= input.color;
    col *= Triplanar(_MainTex, sampler_MainTex, input);
    col.rgb *= lerp(0.5, 1.0, attenuation);

    return col;
}
