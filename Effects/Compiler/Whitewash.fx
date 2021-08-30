float2 resolution;
float3 outlineColor;

texture toDraw;
sampler2D ToDraw = sampler_state
{
    texture = <toDraw>;
};


struct VertexShaderInput
{
    float2 coord : TEXCOORD0;
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float2 coord : TEXCOORD0;
    float4 Position : SV_POSITION;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    return input;
}

float4 main(VertexShaderOutput input) : COLOR
{
    float2 st = input.coord;
	
    float4 color = tex2D(ToDraw, st);
    
    color.x = 1;
    color.y = 1;
    color.z = 1;

    return color;
}

technique Technique1
{
    pass Shade
    {
        PixelShader = compile ps_2_0 main();
    }
}