
texture draw;
sampler2D InputLayer1 = sampler_state { texture = <draw>; };

texture behind;
sampler2D InputLayerMain = sampler_state { texture = <behind>; AddressU = wrap; AddressV = wrap; };

texture map;
sampler2D InputLayer1Overlay = sampler_state { texture = <map>; };

float4 screen;
float2 drawSize;

float RefractionStrength;

static const float tau = 6.283185307;

float4 main(float2 uv : TEXCOORD) : COLOR 
{ 	
	//samping the overlay
	float2 min = screen / drawSize;

	float xMax = (screen.z) / drawSize.x;
	float yMax = (screen.w) / drawSize.y;

	float xPos = min.x + uv.x * xMax;
	float yPos = min.y + uv.y * yMax;

	float4 OverlayColor = tex2D(InputLayer1Overlay, float2(xPos, yPos));//gets full overlay color
	
	//offset math
	float RefractionRadian = OverlayColor.r * tau;//uses just the r channel * tau to get a radian (color values here are 0 to 1)
	float2 RefractionOffset = (float2(cos(RefractionRadian), sin(RefractionRadian)) * RefractionStrength) * OverlayColor.a;//gets a x/y offset from radian, and multiplies it by overlay alpha channel. so there doesn't need to be branching
	
	//other samples
	float4 MainColor = tex2D(InputLayerMain, uv + RefractionOffset);//main sample, offset by refraction, is not offset if overlay alpha is zero
	float4 Layer1Color = tex2D(InputLayer1, uv);//sampling crystal layer
	
	//combining layers
	//float4 CombinedColor = float4(lerp(MainColor.rgb, Layer1Color.rgb, Layer1Color.a * 0.7), 1);//alt method
	float4 CombinedColor = MainColor + Layer1Color;//simple method

	return CombinedColor; 
}

technique Technique1
{
    pass Shade
    {
        PixelShader = compile ps_2_0 main();
    }
}