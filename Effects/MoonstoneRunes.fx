sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

texture noiseTexture1;
sampler2D noiseSampler1 = sampler_state { texture = <noiseTexture1>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture noiseTexture2;
sampler2D noiseSampler2 = sampler_state { texture = <noiseTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float intensity;

float time;

float4 color1;

float4 color2;

float screenWidth;
float screenHeight;

bool drawOriginal = true;

float opacity = 1.0f;

float4 MainPS(float2 uv : TEXCOORD) : COLOR 
{ 
	float noiseR1 = tex2D(noiseSampler1, uv.xy).r;
	float noiseR2 = tex2D(noiseSampler2, uv.xy).r;
	
	float4 whiteColor = tex2D(uImage0, uv.xy);
	
	if (whiteColor.a > 0 && drawOriginal)
		return whiteColor;
		
	float2 noiseCoords1 = uv;
	noiseCoords1.x += (intensity * (sin((noiseR1 * 6.28f) + 4 + (0.1f * time)) * noiseR2)) / screenWidth;
	noiseCoords1.y += (intensity * (cos((noiseR2 * 6.28f * 1.1f) + 3 + (0.2f * time)) * noiseR1)) / screenHeight;
	
	float2 noiseCoords2 = uv;
	noiseCoords2.x += (intensity * (sin((noiseR2 * 6.28f * 0.9f) + 1 + (0.17f * time)) * noiseR1)) / screenWidth;
	noiseCoords2.y += (intensity * (cos((noiseR1 * 6.28f * 1.05f) + 2 + (0.12f * time)) * noiseR2)) / screenHeight;
	
	float4 ret = float4(0,0,0,0);

	ret += tex2D(uImage0, noiseCoords1) * color1;
	ret += tex2D(uImage0, noiseCoords2) * color2;

	return ret; 
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_3_0 MainPS();
    }
};