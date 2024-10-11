texture drawTexture;
sampler2D drawSampler = sampler_state
{
    texture = <drawTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

texture noiseTexture;
sampler2D noiseSampler = sampler_state
{
    texture = <noiseTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float time;
float4 incolor;
float width;
float height;

float GoodCos(float input)
{
    return sin(input + 3.14);
}

float4 MainPS(float2 uv : TEXCOORD) : COLOR
{
    float2 res = float2(width, height);
    
    float2 st = uv;

    float2 off = float2(sin(time * 2.0), GoodCos(time * 2.0)) * 0.35f;
    float2 off2 = float2(sin(-time * 1.2), GoodCos(-time * 1.2)) * 0.15f;
    
    off += time * 0.02;
    off2 += time * -0.02;
    
    float2 one = float2(st.x + off.x, off.y + st.y * 0.05);
    float2 two = float2(st.x + off2.x, off.y + st.y * 0.05);

    float3 noise =
    tex2D(noiseSampler, one).rgb *
    tex2D(noiseSampler, two).rgb;
    
    float4 color = tex2D(drawSampler, st);
    
    color.a = color.r;
    
    color.rgb *= noise; 
    color.rgb *= incolor * 3.0;
    
    return color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_3_0 MainPS();
    }
};