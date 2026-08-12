float3 u_color;
float3 u_fade;
float2 u_resolution;
float u_time;
float u_strength;

texture mainbody_t;
sampler2D mainbody = sampler_state { texture = <mainbody_t>; AddressU = wrap; AddressV = wrap; };

texture noisemap_t;
sampler2D noisemap = sampler_state { texture = <noisemap_t>; AddressU = wrap; AddressV = wrap; };


float noiseAtAngle(float angle)
{
    return tex2D(noisemap, float2(angle, 0)).x;
}

float3 rainbow(float2 st, float time)
{
    float3 col = float3(u_color.r + u_fade.r * abs(sin(st.x + time)),
                        u_color.g + u_fade.g * abs(sin(st.y + time + 2.0)),
                        u_color.b + u_fade.b * abs(sin(st.x + st.y + time + 4.0)));
    return col;
}

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 st = uv;

    float2 auv = st - 0.5;

    float2 sample = float2(0.0, 0.0);
    float angle = atan2(auv.x, auv.y);
    float angle2 = (3.14 + atan2(auv.x, auv.y)) / 6.28;
    sample.y = angle2;
    sample.x = 0.5 + u_time * 0.1;

    float2x2 rotate = float2x2(sin(angle), cos(angle), cos(angle), -sin(angle));
    st += mul(float2(tex2D(noisemap, sample).x * -u_strength, 0), rotate);

    float3 color = tex2D(mainbody, st + float2(6, 0) / u_resolution).xyz;
    color += tex2D(mainbody, st + float2(0, 6) / u_resolution).xyz;
    color += tex2D(mainbody, st + float2(-6, 0) / u_resolution).xyz;
    color += tex2D(mainbody, st + float2(0, -6) / u_resolution).xyz;

    color -= tex2D(mainbody, st).xyz * 4.0;

    return float4(color * rainbow(st, u_time), 0.0);
}

technique Technique1
{
    pass VitricReplicaItemPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}