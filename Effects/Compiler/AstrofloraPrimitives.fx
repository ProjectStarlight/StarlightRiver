matrix transformMatrix;

Texture2D noise;

sampler2D noiseSampler = sampler_state
{
    Texture = <noise>;
};

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
    //float4 noiseColor = tex2D(noiseSampler, input.TexCoords);
    
    //float brightness = ((noiseColor.r + noiseColor.g + noiseColor.b) / 3) / 2; // Division by 2 decreases intensity a bit.
    
    float darkness = input.TexCoords.x;
    
    return input.Color * darkness;
}

technique Technique1
{
    pass AstrofloraPrimitivesPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};