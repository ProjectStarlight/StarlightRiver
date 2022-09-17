sampler uImage0 : register(s0); // The contents of the screen

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
float4 uShaderSpecificData;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float threshhold = 0.5f;

bool whiteEdge(float2 coords) //CREDIT: https://github.com/James-Jones/HLSLCrossCompiler/blob/master/tests/apps/shaders/generic/postProcessing/sobel.hlsl
{
	float2 pixelSize = 2 / uScreenResolution;
	const int2 texAddrOffsets[8] = {
            int2(-1, -1), 
            int2( 0, -1),
            int2( 1, -1),
            int2(-1,  0),
            int2( 1,  0),
            int2(-1,  1),
            int2( 0,  1),
            int2( 1,  1),
    };

    float lum[8];
    int i;

    float3 LuminanceConv = { 0.2125f, 0.7154f, 0.0721f };

    for (i=0; i < 8; i++) {
      float3 color = tex2D(uImage0, coords + (texAddrOffsets[i] * pixelSize)).rgb;
      lum[i] = dot(color, LuminanceConv);
    }

    float x = lum[0] + 2 * lum[3] + lum[5] - lum[2] - 2 * lum[4] - lum[7];
    float y = lum[0] + 2 * lum[1] + lum[2] - lum[5] - 2 * lum[6] - lum[7];
    float edge = sqrt(x*x + y*y);
    return edge > threshhold;
}

float4 White(float4 unused : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{ 
	float4 originalColor = tex2D(uImage0, uv);
	
	if (uProgress < 0.01f || uProgress > 1.2f)
		return originalColor;
		
	float4 ret = float4(uSecondaryColor, 1);
	if (whiteEdge(uv))
		ret = float4(uColor, 1);
	return ret;
}

technique Technique1
{
    pass ImpactFrame_OutlinePass
    {
        PixelShader = compile ps_3_0 White();
    }
}