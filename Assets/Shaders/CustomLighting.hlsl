#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

void MainLight_float(float3 WorldPos, out float3 Direction, out float3 Color, out float DistanceAtten, out float ShadowAtten)
{
#if SHADERGRAPH_PREVIEW
    Direction = float3(0.5, 0.5, 0);
    Color = 1;
    DistanceAtten = 1;
    ShadowAtten = 1;
#else
    #if SHADOWS_SCREEN
        float4 clipPos = TransformWorldToHClip(WorldPos);
        float4 shadowCoord = ComputeScreenPos(clipPos);
    #else
        float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    #endif
    Light mainLight = GetMainLight(shadowCoord);
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = mainLight.shadowAttenuation;
#endif
}

void MainLight_half(float3 WorldPos, out half3 Direction, out half3 Color, out half DistanceAtten, out half ShadowAtten)
{
#if SHADERGRAPH_PREVIEW
    Direction = half3(0.5, 0.5, 0);
    Color = 1;
    DistanceAtten = 1;
    ShadowAtten = 1;
#else
#if SHADOWS_SCREEN
    half4 clipPos = TransformWorldToHClip(WorldPos);
    half4 shadowCoord = ComputeScreenPos(clipPos);
#else
    half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
#endif
    Light mainLight = GetMainLight(shadowCoord);
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = mainLight.shadowAttenuation;
#endif
}

void DirectSpecular_float(float3 Specular, float Smoothness, float3 Direction, float3 Color, float3 WorldNormal, float3 WorldView, out float3 Out)
{
#if SHADERGRAPH_PREVIEW
    Out = 0;
#else
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    Out = LightingSpecular(Color, Direction, WorldNormal, WorldView, float4(Specular, 0), Smoothness);
#endif
}

void DirectSpecular_half(half3 Specular, half Smoothness, half3 Direction, half3 Color, half3 WorldNormal, half3 WorldView, out half3 Out)
{
#if SHADERGRAPH_PREVIEW
    Out = 0;
#else
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    Out = LightingSpecular(Color, Direction, WorldNormal, WorldView,half4(Specular, 0), Smoothness);
#endif
}

void AdditionalLights_float(float3 SpecColor, float Smoothness, float3 WorldPosition, float3 WorldNormal, float3 WorldView, out float3 Diffuse, out float3 Specular)
{
    float3 diffuseColor = 0;
    float3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPosition);
        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
        specularColor += LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, float4(SpecColor, 0), Smoothness);
    }
#endif

    Diffuse = diffuseColor;
    Specular = specularColor;
}

void AdditionalLights_half(half3 SpecColor, half Smoothness, half3 WorldPosition, half3 WorldNormal, half3 WorldView, out half3 Diffuse, out half3 Specular)
{
    half3 diffuseColor = 0;
    half3 specularColor = 0;

#ifndef SHADERGRAPH_PREVIEW
    Smoothness = exp2(10 * Smoothness + 1);
    WorldNormal = normalize(WorldNormal);
    WorldView = SafeNormalize(WorldView);
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPosition);
        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);
        diffuseColor += LightingLambert(attenuatedLightColor, light.direction, WorldNormal);
        specularColor += LightingSpecular(attenuatedLightColor, light.direction, WorldNormal, WorldView, half4(SpecColor, 0), Smoothness);
    }
#endif

    Diffuse = diffuseColor;
    Specular = specularColor;
}

// Halftone dot pattern - a uniform, fixed-size repeating grid of dots (like
// manga/comic screentone ink), triplanar-projected off OBJECT space so it's
// stuck to the character (moves/rotates with it, unlike screen-space) with
// no UV-seam breaks (unlike sampling straight off UV0) and no pole-stretch
// (unlike a single flat XY/XZ/YZ projection) - the three axis projections
// are blended by how much the surface normal faces each axis, so every
// point on the surface mostly uses whichever projection it's most flatly
// facing. DotSize is a constant 0-0.5 radius (0.5 = dots touch their
// neighbors and read as solid); it does NOT vary with how dark the shadow
// is - "how dark" is a separate call's job (mask this node's Out against
// your shadow mask downstream, e.g. Multiply by One-Minus(toon shadow), so
// dots only appear inside the shadow region). Out is 1 where a dot should
// draw (ink), 0 where it shouldn't - blended values between are possible
// right at the seam between two projections.
float HalftoneDot(float2 uv, float DotSize, float CellSize, float Rotation)
{
    float s = sin(Rotation);
    float c = cos(Rotation);
    float2 rotated = float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
    float2 cell = rotated / max(CellSize, 0.0001);
    float2 cellUV = frac(cell) - 0.5;
    float dist = length(cellUV);
    return step(dist, saturate(DotSize));
}

half HalftoneDot(half2 uv, half DotSize, half CellSize, half Rotation)
{
    half s = sin(Rotation);
    half c = cos(Rotation);
    half2 rotated = half2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
    half2 cell = rotated / max(CellSize, 0.0001h);
    half2 cellUV = frac(cell) - 0.5;
    half dist = length(cellUV);
    return step(dist, saturate(DotSize));
}

// Hard axis pick instead of a blend - a weighted 3-way blend of three
// already-binary (0/1) dot grids always has a wide band where none of the
// three weights dominates, which reads as a smeared gray blob rather than
// dots (a sphere's normal sweeps through every direction, so that band is
// never just a thin seam). Snapping to whichever single axis the normal
// faces most keeps every dot crisp everywhere except a hard cut line at the
// boundary, where dots can look slightly stretched but never blurred.
void Halftone_float(float3 ObjectPosition, float3 ObjectNormal, float DotSize, float CellSize, float Rotation, out float Out)
{
    float3 n = abs(normalize(ObjectNormal));

    if (n.x >= n.y && n.x >= n.z)
        Out = HalftoneDot(ObjectPosition.yz, DotSize, CellSize, Rotation);
    else if (n.y >= n.z)
        Out = HalftoneDot(ObjectPosition.xz, DotSize, CellSize, Rotation);
    else
        Out = HalftoneDot(ObjectPosition.xy, DotSize, CellSize, Rotation);
}

void Halftone_half(half3 ObjectPosition, half3 ObjectNormal, half DotSize, half CellSize, half Rotation, out half Out)
{
    half3 n = abs(normalize(ObjectNormal));

    if (n.x >= n.y && n.x >= n.z)
        Out = HalftoneDot(ObjectPosition.yz, DotSize, CellSize, Rotation);
    else if (n.y >= n.z)
        Out = HalftoneDot(ObjectPosition.xz, DotSize, CellSize, Rotation);
    else
        Out = HalftoneDot(ObjectPosition.xy, DotSize, CellSize, Rotation);
}

#endif