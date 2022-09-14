sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

texture noiseTexture1;
sampler2D noiseSampler1 = sampler_state { texture = <noiseTexture1>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture noiseTexture2;
sampler2D noiseSampler2 = sampler_state { texture = <noiseTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float intensity;

float time;

float repeats;

float2 screenPosition;

float3 distortionColor1;
float3 distortionColor2;

float colorIntensity;

float2 rotation(float2 coords, float delta)
{
    float2 ret;
    ret.x = (coords.x * cos(delta)) - (coords.y * sin(delta));
    ret.y = (coords.x * sin(delta)) + (coords.y * cos(delta));
    return ret;
}

float4 MainPS(float2 uv : TEXCOORD) : COLOR 
{ 
	float2 offset = float2(intensity, intensity);
	float2 uv_n = uv;
	
	float length = 0;
	for (int i = 0; i < 10; i++)
	{
		float intensity1 = tex2D(noiseSampler1, screenPosition + (uv_n + float2(time / 6.28f,time / 6.28f) / repeats)).r;
		float intensity2 = tex2D(noiseSampler1, screenPosition + (uv_n + float2(-time / 6.28f,time / 6.28f) / repeats)).r;
		float angle = (time + (sqrt(intensity1 * intensity2) * 6.28f)) * tex2D(noiseSampler2, uv * 0.1f);
		uv_n += rotation(offset, angle);
		length += abs(angle);
	}
	
	float4 ret = tex2D(uImage0, uv_n);
	float lerper = min(length * colorIntensity, 1);
	float3 distortionColor = lerp(distortionColor1, distortionColor2, lerper);
	ret.rgb = lerp(ret.rgb, distortionColor, lerper);
	return ret;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_3_0 MainPS();
    }
};