

matrix transformMatrix;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture noiseTexture;
sampler2D noiseTex = sampler_state { texture = <noiseTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float repeats;
float progress;

float3 midColor;
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

float3 colorLerp(float3 colorOne, float3 colorTwo, float val)
{
    float3 ret = colorOne;
    float3 difference = colorTwo - colorOne;
    ret += difference * val;
    return ret;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float xcoord = input.TexCoords.x * repeats;
    float ycoord = input.TexCoords.y;
	float2 st = float2(xcoord, ycoord);

	float3 color = tex2D(samplerTex, st).xyz;

    st.y /= 2;
    float3 trueColor = tex2D(noiseTex, st).xyz;

    float3 white = float3(1, 1, 1);

    float lerper = pow(trueColor.r, (2 - (progress * 2))) * 2;

    if (lerper > 1)
        trueColor = colorLerp(midColor, white, lerper - 1);
    else
        trueColor = colorLerp(input.Color, midColor, lerper);
    
    return float4(color * trueColor * progress, color.r * input.Color.a * progress);
}

technique Technique1
{
    pass PrimitivesPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};