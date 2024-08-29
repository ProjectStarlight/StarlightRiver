
float time;
float repeats;
float2 resolution;
float2 texSize;

matrix transformMatrix;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float4 PosiitonPass : TEXCOORD1;
    float2 TexCoords : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Color = input.Color;
    output.TexCoords = input.TexCoords;
    output.Position = mul(input.Position, transformMatrix);
    output.PosiitonPass = output.Position;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 screen = input.PosiitonPass.xy;
    screen -= fmod(screen, 2.0 / resolution);
    
    float2 ratio = resolution /texSize;
	float2 st = float2(input.TexCoords.x * repeats, input.TexCoords.y);
    st -= fmod(input.PosiitonPass.xy, 2.0 / resolution * ratio);
    
	float4 color = tex2D(samplerTex, st);

    return float4(screen.x + 1.0 / 2.0, screen.y + 1.0 / 2.0, 0.0, 1.0);
    return color * float4(input.Color.xyz, color.w);
}

technique Technique1
{
    pass PrimitivesPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
};