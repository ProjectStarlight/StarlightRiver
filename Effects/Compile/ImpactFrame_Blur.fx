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

texture originalTex;
sampler2D originalSample = sampler_state { texture = <originalTex>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float noiseRepeats = 3.52f;
float noiseThreshhold = 0.8f;
float fadeRoot = 0.5f;

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

float4 White(float4 unused : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{ 
	float4 originalColor = tex2D(originalSample, uv);
	
	if (uProgress < 0.01f || uProgress > 1.2f)
		return originalColor;
		
	float LineLength;
	if (uProgress < 0.1f)
		LineLength = sqrt(min(uProgress * 10, 0.99f));
	else
		LineLength = pow(1.1f - uProgress, 12);
	LineLength *= uIntensity;
	
	float2 pixelSize = float2(LineLength, LineLength);
		
	float fade = min(1, pow(uProgress, 0.7f));
	
	float2 directionTrue = (uv * uScreenResolution) - uTargetPosition;
	float2 direction = normalize(directionTrue);
	float4 ret = float4(uSecondaryColor, 1);
	for (int i = 0; i < 40; i++)
	{
		float2 offset = direction * (i * pixelSize);
		if (length(offset) > length(directionTrue))
			offset = directionTrue;
		offset /= uScreenResolution;
		float4 color = tex2D(uImage0, (uv - offset));

		if (tex2D(vnoise, ((uv - offset) * noiseRepeats) % 1).r > noiseThreshhold && abs(color.r - uSecondaryColor.r) > 0.5f)
			ret = float4(uColor, 1);
	}
	return ret;
}

technique Technique1
{
    pass ImpactFrame_BlurPass
    {
        PixelShader = compile ps_3_0 White();
    }
}