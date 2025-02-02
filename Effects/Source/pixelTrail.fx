//i dont know what i am doing
float time;
float repeats;

float pixelation;
float2 resolution;

matrix transformMatrix;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture targetTexture;
sampler2D targetTex = sampler_state { texture = <targetTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror; };

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
	float2 scaledRes = resolution / pixelation;
	float2 modUV = (round(input.TexCoords * scaledRes) / scaledRes);

    float3 color = tex2D(samplerTex, modUV + float2(time, 0)).xyz;
	float3 color2 = tex2D(samplerTex, modUV + float2(-time * 1.5, 0)).xyz * 0.5;

    return float4((color + color2) * input.Color * (1.0 + color.x * 2.0), color.x * input.Color.w);
}

technique Technique1
{
    pass PrimitivesPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};
