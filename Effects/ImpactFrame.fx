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

texture vnoiseTexture;
sampler2D vnoise = sampler_state { texture = <vnoiseTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float noiseRepeats = 3.52f;

float2 rotation(float2 coords, float delta)
{
    float2 ret;
    ret.x = (coords.x * cos(delta)) - (coords.y * sin(delta));
    ret.y = (coords.x * sin(delta)) + (coords.y * cos(delta));
    return ret;
}
bool InWave(float2 coords)
{
	float2 pixelSize = 2 / uScreenResolution;
	float progress = sqrt(max(0,uProgress- 0.05f));
	return abs(length(coords - uTargetPosition) - (progress * 3.48f)) < pixelSize.x * lerp(200, 50, sqrt(progress));
}

bool whiteEdge(float2 coords) //CREDIT: https://github.com/James-Jones/HLSLCrossCompiler/blob/master/tests/apps/shaders/generic/postProcessing/sobel.hlsl
{
	float threshhold = 1.0f;
	float2 screenResolution = float2(720, 720);
	float2 pixelSize = 2 / screenResolution;
	const int2 texAddrOffsets[4] = {
            int2( 0, -1),
            int2(-1,  0),
            int2( 1,  0),
            int2( 0,  1),
    };

    float lum[4];
    int i;

    float3 LuminanceConv = { 0.2125f, 0.7154f, 0.0721f };

    for (i=0; i < 4; i++) {
      float3 color = tex2D(uImage0, coords + (texAddrOffsets[i] * pixelSize)).rgb;
      lum[i] = dot(color, LuminanceConv);
    }

    float x = (lum[0] + lum[1] + lum[2] + lum[3]) / 4.0f;
    return x > threshhold;
}

float4 White(float4 unused : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{ 
	float4 whiteInvert = float4(uColor, 1);
	float4 blackInvert = float4(uSecondaryColor, 1);
	
	if (uProgress < 0.1f)
	{
		whiteInvert = float4(uSecondaryColor, 1);
		blackInvert = float4(uColor, 1);
	}
	float4 originalColor = tex2D(uImage0, uv);
	
	if (uProgress < 0.01f || uProgress > 1.2f)
		return originalColor;
		
	float LineLength;
	if (uProgress < 0.1f)
		LineLength = sqrt(min(uProgress * 10, 0.99f));
	else
		LineLength = pow(1.1f - uProgress, 12);
	float noiseThreshhold = lerp(0.8f, 0.9f, uProgress);
	LineLength *= 5;
	
	float2 pixelSize = float2(LineLength, LineLength);
		
	float fade = min(1, pow(uProgress, 0.7f));
	
	float2 direction = normalize(uv - uTargetPosition);
	float4 ret = lerp(blackInvert, originalColor, fade);
	for (int i = 0; i < 10; i++)
	{
		float2 offset = direction * (i * pixelSize);
		offset /= uScreenResolution;
		if (tex2D(vnoise, ((uv - offset) * noiseRepeats) % 1).r > noiseThreshhold && whiteEdge(uv - offset))
			ret = lerp(whiteInvert, originalColor, fade);
	}
	
	for (int j = 0; j < 5; j++)
	{
		float2 offset = direction * (j * (5 / uScreenResolution));
		if (tex2D(vnoise, ((uTargetPosition + (offset * 20)) * noiseRepeats) % 1).r > noiseThreshhold && InWave(uv - offset))
			ret = whiteInvert;
	}
	return ret;
}

technique Technique1
{
    pass MainPS
    {
        PixelShader = compile ps_3_0 White();
    }
}