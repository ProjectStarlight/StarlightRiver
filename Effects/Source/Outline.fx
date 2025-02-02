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
	
    float4 color = float4(0, 0, 0, 0);
    color = tex2D(ToDraw, st);
    
    color.x = round(color.x * 30.0) / 30.0;
    color.y = round(color.y * 30.0) / 30.0;   
    color.z = round(color.z * 30.0) / 30.0;
    
    
    color.xyz += outlineColor * abs(min(0, color.w - 1));
    
    color.w += tex2D(ToDraw, st + float2(2.0, 0.0) / resolution).w;
    color.w += tex2D(ToDraw, st + float2(-2.0, 0.0) / resolution).w;
    color.w += tex2D(ToDraw, st + float2(0.0, 2.0) / resolution).w;
    color.w += tex2D(ToDraw, st + float2(0.0, -2.0) / resolution).w;
    
    color.xyz *= min(color.w, 1);
    
    return color;
}

technique Technique1
{
    pass Shade
    {
        PixelShader = compile ps_2_0 main();
    }
}