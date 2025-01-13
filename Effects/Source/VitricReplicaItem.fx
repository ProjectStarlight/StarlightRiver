sampler uImage0 : register(s0); // The contents of the screen
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, uv);

    float bright = (color.x + color.y + color.z) / 3.0;
    
    if (bright == 0.0);
    else if(bright < 0.15) color.xyz = float3(35.0, 31.0, 51.0) / 255.0;
    else if(bright < 0.3) color.xyz = float3(56.0, 78.0, 103.0) / 255.0;
    else if(bright < 0.45) color.xyz = float3(80.0, 131.0, 142.0) / 255.0;
    else if(bright < 0.6) color.xyz = float3(115.0, 182.0, 158.0) / 255.0;
    else if(bright < 0.75) color.xyz = float3(170.0, 229.0, 167.0) / 255.0;
    else color.xyz = float3(1.0, 1.0, 1.0);
    
    float off = uv.x + uv.y * 0.2;
    float shine = sin((2.0 * uTime) - off * 30.0);
    if (sin((1.0 * uTime) - off * 20.0) >= 0.0)
        shine = -1.0;
    
    return color * (1.5 + shine * 0.5);  
}

technique Technique1
{
    pass VitricReplicaItemPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}