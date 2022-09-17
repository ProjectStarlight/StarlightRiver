sampler uImage0 : register(s0); // The contents of the screen

float fade;
float time;

float4 uSecondaryColor;
float4 uColor;

texture originalTex;
sampler2D originalSample = sampler_state { texture = <originalTex>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 White(float4 unused : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{ 
	float4 newColor = tex2D(uImage0, uv);
	float4 originalColor = tex2D(originalSample, uv);

	if (time < 0.1f)
	{
		if (abs(newColor.r - uSecondaryColor.r) > 0.1f)
			newColor = uSecondaryColor;
		else
			newColor = uColor;
	}
	float4 ret = lerp(newColor, originalColor, fade);
	return ret;
}

technique Technique1
{
    pass ImpactFrame_FadePass
    {
        PixelShader = compile ps_3_0 White();
    }
}