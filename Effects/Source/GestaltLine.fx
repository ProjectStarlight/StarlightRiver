float u_time;
float u_speed;

texture maintex_t;
sampler2D maintex = sampler_state { texture = <maintex_t>; AddressU = wrap; AddressV = wrap; };

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 sam1 = uv + float2(0.0, sin(u_time) * 0.1 * sin( uv.x * 10.0 + u_time * u_speed));
    float2 sam2 = uv + float2(0.0, sin(u_time + 2.0) * 0.1 * sin( uv.x * 7.0 + u_time * u_speed * 0.8));
    float2 sam3 = uv + float2(0.0, sin(u_time + 4.0) * 0.1 * sin( uv.x * 12.0 + u_time * u_speed * 1.1));
   
    float3 color = tex2D( maintex, sam1).xyz * float3(1.0, 0.2, 0.2);
    color += tex2D( maintex, sam2).xyz * float3(0.2, 1.0, 0.2);
    color += tex2D( maintex, sam3).xyz * float3(0.2, 0.2, 1.0);
    
    return float4(color, (color.r + color.g + color.b) / 3.0);
}

technique Technique1
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
