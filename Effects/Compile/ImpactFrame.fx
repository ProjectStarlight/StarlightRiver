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
float noiseThreshhold = 0.8f;
float threshhold = 0.5f;
float waveThreshhold;

float2 rotation(float2 coords, float delta)
{
    float2 ret;
    ret.x = (coords.x * cos(delta)) - (coords.y * sin(delta));
    ret.y = (coords.x * sin(delta)) + (coords.y * cos(delta));
    return ret;
}
bool InWave(float2 coords)
{
	float2 pixelSize = 5 / uScreenResolution;
	float progress = sqrt(max(0,uProgress- 0.05f));
	return abs(length(coords - (uTargetPosition/ uScreenResolution)) - (progress * 3.48f)) < pixelSize.x * lerp(100, 20, sqrt(progress));
}

bool whiteEdge(float2 coords) //CREDIT: https://github.com/James-Jones/HLSLCrossCompiler/blob/master/tests/apps/shaders/generic/postProcessing/sobel.hlsl
{
	float2 pixelSize = 5 / uScreenResolution;
    int i;

	float3 originalColor = tex2D(uImage0, coords).rgb;
    float3 color = abs(tex2D(uImage0, coords + pixelSize).rgb - originalColor);
	return color.r + color.g + color.b > threshhold;
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
	LineLength *= 5;
	
	float2 pixelSize = float2(LineLength, LineLength);
		
	float fade = min(1, pow(uProgress, 0.7f));
	
	float2 direction = normalize(uv - (uTargetPosition/ uScreenResolution));
	float4 ret = lerp(blackInvert, originalColor, fade * fade);
	for (int i = 0; i < 20; i++)
	{
		float2 offset = direction * (i * pixelSize);
		offset /= uScreenResolution;
		if (tex2D(vnoise, ((uv - offset) * noiseRepeats) % 1).r > noiseThreshhold && whiteEdge(uv - offset))
			ret = lerp(whiteInvert, originalColor, fade);
	}
	
	for (int j = 0; j < 10; j++)
	{
		float2 offset = direction * (j * (5.0f / uScreenResolution));
		if (tex2D(vnoise, (((uTargetPosition/ uScreenResolution) + (offset * 20)) * noiseRepeats) % 1).r > waveThreshhold && InWave(uv - offset))
			ret = whiteInvert;
	}
	return ret;
}

technique Technique1
{
    pass ImpactFramePass
    {
        PixelShader = compile ps_3_0 White();
    }
}