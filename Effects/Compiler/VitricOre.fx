float time;
float4x4 scale;

texture toDraw;
sampler2D ToDraw = sampler_state {texture = <toDraw>;};

texture outline;
sampler2D Outline = sampler_state {texture = <toDraw>;};

texture refract;
sampler2D Refract = sampler_state {texture = <toDraw>;};

texture volume;
sampler2D Volume = sampler_state {texture = <toDraw>;};




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
    color.a = 0.65 + tex2D(Outline, st).a;
    
    float speedMul = 0.5;
    
    float4 map = tex2D(Refract, st);
    
    float off = map.x * tex2D(Volume, st).x;
    float target = 2.0-(st.x + st.y * off * 2.5) + off * 2.0;
    float woff =  0.07;
    
    if(fmod(time * 2.0 * speedMul, 6.0) + woff > target && fmod(time * 2.0 * speedMul, 6.0) - woff < target)
    	color += tex2D(ToDraw, st) * 1.5 * map.x;
    
    color.xyz += (-0.1 + sin( time * 5.0 * speedMul + map.x * 7.0) * 0.05) * map.a;
    
    return color;
}

technique Technique1
{
    pass Shade
    {
        PixelShader = compile ps_2_0 main();
    }
}