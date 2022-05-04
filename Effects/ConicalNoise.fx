sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

texture vnoise;
sampler vnoiseSampler = sampler_state
{
    Texture = (vnoise);
};

texture pallette;
sampler palletteSampler = sampler_state
{
    Texture = (pallette);
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

	float column;
	if (conecoords.y > 1 || conecoords.y < 0)
		return float4(0, 0, 0, 0);
	column = tex2D(uImage0, conecoords).r;
	column *= (0.2f + pow(uv.x, 2)) - tex2D(vnoiseSampler, noiseCoords).r;

	float4 Color = tex2D(palletteSampler, float2(clamp(column, 0, 1), 0.5f)) * column;

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