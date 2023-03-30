texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 color;
float intensity;

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    return tex2D(samplerTex, uv) * color * 3.0 * intensity;
}

technique Technique1
{
    pass CrescentOrbPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}