sampler uImage0 : register(s0); // The contents of the screen

float fade;

texture originalTex;
sampler2D originalSample = sampler_state { texture = <originalTex>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 White(float4 unused : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{ 
	float4 newColor = tex2D(uImage0, uv);
	float4 originalColor = tex2D(originalSample, uv);
	
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