sampler uImage0 : register(s0); // The contents of the screen
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor; //set flat RGB value here
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity; //set flat alpha value here (reflection strength)
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;


//used to turn complex images (like vanila glass walls) that are using their RGBA channels and convert it into a generic reflection map with constant angle and reflection power where the image has alpha greater than 0
//generally should just create sprite custom reflection maps for anything more complex than this since they look much better

float4 P1(float2 coords : TEXCOORD0) : COLOR0
{

    float4 texColor = tex2D(uImage0, coords);
    

    return float4(uColor, uIntensity) * step(0.001, texColor.a);
}

technique Technique1
{
    pass ReflectionMapperPass
    {
        PixelShader = compile ps_2_0 P1();
    }
}