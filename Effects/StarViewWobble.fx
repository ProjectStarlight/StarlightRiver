texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

float uTime;
float uIntensity;
// float bobbleAngle;
// float bobbleMag;

float sinsquared(float theta) {
    return pow(sin(theta), 2.0);
}

float4 PixelShaderFunction(float2 uv : TEXCOORD0) : COLOR0
{
    float2 st = uv;
    float theta = atan2(st.y - 0.5, st.x - 0.5);
	// float alpha = (sinsquared(theta * 2. + uTime * 1.2 + 2.0) + sinsquared(theta * 1.5 - uTime  + 2.0) + sin(1.57 - (theta - bobbleAngle)) * bobbleMag) / 10.0;
	float alpha = (sinsquared(theta * 2. + uTime * 1.2 + 2.0) + sinsquared(theta * 1.5 - uTime  + 2.0)) / 10.0;

    float2 center = {0.5, 0.5};
    st = st - (st - center) * alpha * uIntensity;
    
    if (float4(tex2D(samplerTex,st)).x > 0.05)
        return float4(tex2D(samplerTex,st));
    else
        return 0;
}

technique Technique1
{
    pass StarViewWobblePass
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}