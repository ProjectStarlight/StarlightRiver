float minAngle;
float maxAngle;
float4 u_color;

texture sampleTexture;
sampler2D  u_tex0 = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 PixelShaderFunction(float2 uv: TEXCOORD) : COLOR0
{
    float angle = atan2(uv.x - 0.5, uv.y - 0.5) + 3.14;
    float4 color = tex2D(u_tex0, uv);
    
    color *= u_color;
    
    if (angle < minAngle || angle > maxAngle)
        color *= 0.0;
    
    return color;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}