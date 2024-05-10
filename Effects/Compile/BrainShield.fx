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

texture pulseTexture;
sampler2D pulseSampler = sampler_state
{
    texture = <noiseTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

texture edgeTexture;
sampler2D edgeSampler = sampler_state
{
    texture = <noiseTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float time;
float2 size;
float opacity;
float pixelRes;

float4 MainPS(float2 st : TEXCOORD) : COLOR
{
    st = st - fmod(st, pixelRes / size.x);
    
    float dist = distance(st, float2(0.5, 0.5));
    float2 middif = (st - float2(0.5, 0.5));
    
    float rot = atan(middif.y / middif.x);
    rot = st.x > 0.5 ? -rot : rot;
    st -= (-0.05 + sin(rot * 8.0 + time * 10.0) * 0.08) * middif * max(0.0, dist - 0.1) * sin(st.y * 3.14);
    st -= (-0.05 + sin(rot * 12.0 + time * 8.0) * 0.04) * middif * max(0.0, dist - 0.1) * sin(st.y * 3.14);
    
    float2 oldst = st;

    st += dist * dist * dist * dist * dist * dist * dist * middif * 300.0;
    
    float3 noise = float3(0.0, 0.0, 0.0);
    noise.r = 0.5 * tex2D(noiseSampler, st + float2(time * 0.1, time * 0.1)).x;
    noise.g = 0.5 * tex2D(noiseSampler, st + 0.2 + float2(0, time * -0.17)).x;
    noise.b = 0.5 * tex2D(noiseSampler, st + 0.4 + float2(time * -0.13, 0)).x;
    noise.rgb += (noise.r + noise.g + noise.b) / 3.0 * 1.5;

    float3 map = tex2D(drawSampler, oldst).xyz;
    
    float3 shined = float3(0.0, 0.0, 0.0);
    float3 under = tex2D(pulseSampler, st).xyz;
    float saved = under.r * 1.5;
    saved += sin(st.x * 3.14);
    shined += 0.5 + 0.5 * sin(st.y * 8.0 + saved - time * 3.0);
    shined += max(0.0, -20.0 + 20.8 * sin(st.y * 12.0 + saved - time * 9.0));
      
    float3 bloom = tex2D(edgeSampler, oldst).xyz * (1.0 - shined) * 0.15;
    
    float3 final = (bloom * map * opacity) + noise * shined * map * opacity;
    
    dist = distance(st, float2(0.5, 0.5));
    final *= 1.0 + max(0.0, (dist - 0.45) * 10.0);
    //final = final - mod(final, 0.2);
    
    return float4(final, 1.0);
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_3_0 MainPS();
    }
};