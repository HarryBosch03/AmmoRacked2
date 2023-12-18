#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
float _Scale;

float4 _HighColor;
float4 _LowColor;

static const float _Ambient = 0.3f;

struct Attributes
{
    float4 vertex : POSITION;
    float4 normal : NORMAL;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float3 positionWS : POSITION_WS;
    float3 normalWS : NORMAL_WS;
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
    return saturate(dot(light.direction, input.normalWS) * 0.5 + 0.5) * light.distanceAttenuation;
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

float Lighting(Varyings input)
{
    float lighting = 0.0f;

    Light mainLight = GetMainLight();
    lighting += Lighting(input, mainLight);

    uint lightCount = GetAdditionalLightsCount();
    for (uint lightIndex = 0u; lightIndex < lightCount; ++lightIndex)
    {
        Light light = GetAdditionalLight(lightIndex, input.positionWS);
        lighting += Lighting(input, light);
    }

    return lighting;
}

half4 frag(Varyings input) : SV_Target
{
    input.normalWS = normalize(input.normalWS);
    input.normalOS = normalize(input.normalOS);
    float attenuation = Lighting(input);

    half4 col = normalize(lerp(_LowColor, _HighColor, input.uv.y)) * lerp(length(_LowColor), length(_HighColor), input.uv.y);
    col *= input.color;
    col *= Triplanar(_MainTex, sampler_MainTex, input);
    col.rgb *= saturate(attenuation + _Ambient);

    return col;
}
