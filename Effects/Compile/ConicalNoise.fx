sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

texture vnoise;
sampler vnoiseSampler = sampler_state
{
    Texture = (vnoise);
};
float rotation;

float transparency;

float4 color;

float val1 = 3.35f;

float val2 = 0.2f;
float4 White(float2 uv : TEXCOORD0) : COLOR0
{
	float2 conecoords = float2(uv.x, uv.y);

	float angle = atan2(conecoords.x - 1.25f, conecoords.y - 0.5) + rotation;
	float2 noiseCoords = float2(0.5f, ((angle + 3.14f) / 6.28f) % 1);

	float4 Color;
	if (conecoords.y > 1 || conecoords.y < 0)
		return float4(0, 0, 0, 0);
	Color = tex2D(uImage0, conecoords);
	Color *= pow(1.25f - tex2D(vnoiseSampler, noiseCoords).r, 4);

	Color *= pow(uv.x, 0.3f);

	Color *= transparency;

	return Color * color;
}

technique BasicColorDrawing
{
    pass WhiteSprite
    {
        PixelShader = compile ps_2_0 White();
    }
};