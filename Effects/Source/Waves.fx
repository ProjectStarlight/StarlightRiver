sampler uImage0 : register(s0);
float uTime;
float2 uImageSize0;
float2 uImageSize1;
float4x4 transform;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture lightTexture;
sampler2D lightTex = sampler_state { texture = <lightTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = wrap; AddressV = wrap; };

texture gameTexture;
sampler2D gameTex = sampler_state { texture = <gameTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = clamp; AddressV = clamp; };

float2 offset;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{  
    float2 originalCoords = coords;
    
    float2 pixel = coords * uImageSize1 * 2.0;
    coords = mul(float4(pixel, 0.0, 1.0), transform).xy / (uImageSize1 * 2.0);
    
    float2 pixCoord = coords - coords % (1.0 / uImageSize1) + float2(1.0 / uImageSize1);
    
    float4 mapSample = tex2D(samplerTex, coords * 2.0);
    float swirl = sin((mapSample.g + uTime * 0.3) * 6.28);
    
    float4 light = tex2D(lightTex, coords * 2.0);
	
    float2 underCoord = coords * 2.0 + (swirl) * 0.005 * tex2D(uImage0, coords).a;
    float2 pixUnderCoord = underCoord - underCoord % (2.0 / uImageSize1);
    
    float4 distortLight = tex2D(lightTex, pixUnderCoord);
    float shapeMask = tex2D(uImage0, coords).a;
    
    float lum = ((light.r + light.g + light.b) / 3.0);
    
    float caustics = tex2D(samplerTex, pixUnderCoord).r;
    caustics = max(0.0, caustics - 0.1);
    float speculars = tex2D(uImage0, pixCoord).g * 0.4 * (caustics + pow(caustics, 3.0) * 1.2 * lum);
    speculars += tex2D(uImage0, coords).r * (0.5 + light * 0.5);
	
    float bright = pow(speculars, 6.0) * 200.0 * pow(lum, 2.0);
    float4 color = distortLight * (pow(speculars, 2.0) * 3.0 + bright);
    color.a = shapeMask;
    
    float2 originalUnderCoord = originalCoords * 2.0 + (swirl) * 0.005 * tex2D(uImage0, coords).a;
    float4 underColor = tex2D(gameTex, originalUnderCoord);
    underColor += shapeMask * distortLight * distortLight * (1.0 - abs(swirl));

	return underColor + color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
	}
}