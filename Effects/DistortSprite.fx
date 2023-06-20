sampler uImage0 : register(s0);

texture uImage1;
sampler2D uImage1Sampler = sampler_state { texture = <uImage1>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture uImage2;
sampler2D uImage2Sampler = sampler_state { texture = <uImage2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture noiseImage1;
sampler noiseImage1Sampler = sampler_state
{
    Texture = (noiseImage1);
};

float uTime;
float time;
float repeats;
float2 offset;
float2 screenPos;

float4 uColor;

float2 rotation(float2 coords, float delta)
{
    float2 ret;
    ret.x = (coords.x * cos(delta)) - (coords.y * sin(delta));
    ret.y = (coords.x * sin(delta)) + (coords.y * cos(delta));
    return ret;
}

float4 distort(float2 uv : TEXCOORD) : COLOR
{
    
	float2 uv_n = uv;

    float2 uv_N = uv;

	for (int i = 0; i < 10; i++)
	{
		float intensity1 = tex2D(uImage1Sampler, screenPos + (uv_n + float2(time / 6.28f, time / 6.28f) / repeats)).r;
		float intensity2 = tex2D(uImage1Sampler, screenPos + (uv_n + float2(-time / 6.28f, time / 6.28f) / repeats)).r;
		float angle = (sqrt(intensity1 * intensity2) * 6.28f) * tex2D(uImage2Sampler, uv * 0.1f);
		uv_n += rotation(offset, angle);
        uv_N += rotation(offset, angle);
	}

    uv_N.x = (uv_N.x + uTime) % 1;


    float4 color = tex2D(uImage0, uv_n);
    if (tex2D(uImage0, uv_n).r > 0)
	    color *= tex2D(noiseImage1Sampler, uv_N);

    color *= uColor;

    return color;
}

technique Technique1
{
    pass distortPass
    {
        PixelShader = compile ps_3_0 distort();
    }
}