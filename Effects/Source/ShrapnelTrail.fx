

matrix transformMatrix;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; };

float progress;

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
    float xcoord = input.TexCoords.x * (1.0f / (progress + 0.1f));
    float ycoord = 0.5f + ((input.TexCoords.y - 0.5f) * progress);
    if (xcoord > 1)
        return float4(0,0,0,0);
	float2 st = float2(xcoord, ycoord);

	float3 color = tex2D(samplerTex, st).xyz;
    float3 white = float3(1, 1, 1);
    float3 originalColor = lerp(input.Color, white, pow(progress, 2));
    float3 trueColor = lerp(white, originalColor, pow(abs(input.TexCoords.y - 0.5f) * 2, 0.2f));
    return float4((color * 2) * trueColor, color.x);
}

technique Technique1
{
    pass PrimitivesPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};