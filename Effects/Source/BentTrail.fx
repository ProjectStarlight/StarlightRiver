texture toDraw;
sampler2D InputLayer0 = sampler_state
{
    texture = <toDraw>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

float time;
float speed;

float squeeze1;
float squeeze2;
float distance;
float bendFac;

float4 color1;
float4 color2;

float4 main(float2 uv : TEXCOORD0) : COLOR0
{
    float2 duv = uv;
    //float2 res = float2(256.0, 128.0);
    //duv = floor(duv * res) / res;
    
    float xFac = duv.x / distance;
    
    float squeeze = lerp(squeeze1, squeeze2, xFac);
    duv.x += time * speed;
    duv.y = (duv.y - 0.5) / squeeze + 0.5;   
    float arc = -pow(2.0 * xFac - 1.0, 2.0) + 1.0;
    duv.y += arc * bendFac;
    
    float4 color = float4(0.0, 0.0, 0.0, 0.0);
    if ((duv.y >= 0.0) && (duv.y <= 1.0))
    {   
        color = tex2D(InputLayer0, duv);
        
        float4 mixedColor = lerp(color1, color2, xFac.xxxx);
        color *= mixedColor;
    }
    
    return color;
}

technique Technique1
{
    pass BentTrailPass
    {
        PixelShader = compile ps_2_0 main();
    }
}