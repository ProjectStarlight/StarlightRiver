float time;
float2 screenSize;
float2 offset;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture sampleTexture2;
sampler2D samplerTex2 = sampler_state { texture = <sampleTexture2>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float4 PixelShaderFunction(float4 screenSpace : TEXCOORD0) : COLOR0
{
    float2 st = screenSpace.xy;

    float power = tex2D(samplerTex2, st).a;
    float4 color = tex2D(samplerTex, st);

    float factor = (0.8 + sin(power * 30.0 + time) * 0.1) * 3.0;
    float bright = ((color.r + color.b + color.g) / 3.0);
    color.g += bright * power * factor;
    color.r += bright * power * factor * 0.5;
    color.a -= power;

    return color;
}

technique Technique1
{
    pass PrimitivesPass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
};