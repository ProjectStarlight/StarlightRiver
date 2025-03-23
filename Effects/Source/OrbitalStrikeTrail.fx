

matrix transformMatrix;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; };
float alpha;

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
    float xcoord = input.TexCoords.x;
    float ycoord = input.TexCoords.y;

    float2 st = float2(xcoord, ycoord);
    float3 color = tex2D(samplerTex, st).xyz;
    float transparency = color.x * alpha;
    float4 ret = float4(color * input.Color, transparency);
    return ret * transparency;
}

technique Technique1
{
    pass PrimitivesPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};