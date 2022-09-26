
float time;
float stretch;

float dilation = 1.37f;
float falloff = 6;

matrix transformMatrix;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture noiseTexture;
sampler2D noiseSampler = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Color = input.Color;
    output.TexCoords = input.TexCoords;
    output.Position = mul(input.Position, transformMatrix);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TexCoords;
	float2 stretchedcoords = float2(uv.x * stretch, uv.y);

	float offset = tex2D(noiseSampler, float2(stretchedcoords.x, ((time * 0.4f) + uv.y) % 1.0f));
	float2 newcoords = float2 (stretchedcoords.x, lerp(uv.y, 0.5f + (sign(uv.y - 0.5f) * 0.25f), pow(offset / 2, dilation)));
	float3 Color = tex2D(samplerTex, uv).rgb; 
	float3 Color2 = tex2D(samplerTex, float2(0.5f, tex2D(noiseSampler, float2((time - abs(uv.y - 0.5f)) % 1.0f, newcoords.y)).r)).rgb * pow(sin(uv.y * 3.14f), falloff);

    float3 totalColor = Color + Color2;
    return float4(totalColor * input.Color, totalColor.x * input.Color.w);
}

technique Technique1
{
    pass PrimitivesPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};