Texture2D SpriteTexture;

float time;

float upperRadiusLimit;
float lowerRadiusLimit;

float4 color;

const float pi = 3.14159265359f;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float circleAngle(float2 testPoint, float2 center)
{
    float x = testPoint.x;
    float y = testPoint.y;
    float cx = center.x;
    float cy = center.y;
    
    double beta, alpha;

    double distanceX = abs(abs(x) - abs(cx));
    double distanceY = abs(abs(y) - abs(cy));

    if (x >= cx && y <= cy)
    {
        beta = atan(distanceY / distanceX);
        alpha = (pi / 2) - beta;
        
        return alpha;
    }
    else if (x >= cx && y >= cy)
    {
        beta = atan(distanceY / distanceX);
        alpha = (pi / 2) + beta;
        
        return alpha;
    }
    else if (x <= cx && y >= cy)
    {
        beta = atan(distanceY / distanceX);
        alpha = ((3 * pi) / 2) - beta;
        
        return alpha;
    }
    else if (x <= cx && y <= cy)
    {
        beta = atan(distanceY / distanceX);
        alpha = ((3 * pi) / 2) + beta;

        return alpha;
    }
    
    return 0;
}

float4 Shade(VertexShaderOutput input) : COLOR
{
    float2 uv = input.TextureCoordinates;
    
    float distanceToCenter = distance(uv, float2(0.5f, 0.5f));
    
    float twoPi = 2 * pi;
    
    float angle = twoPi - circleAngle(uv, float2(0.5f, 0.5f));
        
    float angleRound = frac(time) * twoPi;
    
    if (distanceToCenter <= upperRadiusLimit && distanceToCenter >= lowerRadiusLimit && angle <= angleRound)
    {
        return color;
    }
    
    return float4(0, 0, 0, 0);  
}

technique Technique1
{
    pass DonutIconPass
    {
        PixelShader = compile ps_2_0 Shade();
    }
};