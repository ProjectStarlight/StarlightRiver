float2 resolution;
float2 center;
float4x4 zoom;

texture sprite;
sampler2D InputLayer3 = sampler_state { texture = <sprite>; };

texture behind;
sampler2D InputLayer0 = sampler_state { texture = <behind>; };

texture volumeMap;
sampler2D InputLayer1 = sampler_state { texture = <volumeMap>; };

texture refractMap;
sampler2D InputLayer2 = sampler_state { texture = <refractMap>; };


struct VertexShaderInput
{
	float2 coord : TEXCOORD0;
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float2 coord : TEXCOORD0;
	float4 Position : SV_POSITION;
};

float offset(float a, float n)
{
	float d = abs(a - n) % 6.28;
	return d > 3.14 ? 6.28 - d : d;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput) 0;
	output.coord = input.coord;
	output.Position = mul(input.Position, zoom);
	return output;
}

float4 main(VertexShaderOutput input) : COLOR
{
	float2 st = input.coord;
	
	float2 vec = center - input.coord;
	float angle = atan2(vec.y, vec.x);
	
	float2 refractColor = tex2D(InputLayer2, st).xw;
	float2 off = float2(cos(angle), sin(angle)) * ((refractColor.x - 0.5) / 50.0) * refractColor.y;

    float4 color = tex2D(InputLayer0, st + off)
	* 1.0 - length(vec)
	* tex2D(InputLayer3, st);
	
	float3 map = tex2D(InputLayer1, st).xyz;
	
	color.xyz += map * (
	abs(offset(angle, 0.75)),
	abs(offset(angle, -1.57)), 
	abs(offset(angle, 2.32)) 
	/ 3.14);

    return float4(color);
}

technique Technique1
{
	pass Shade { PixelShader = compile ps_2_0 main(); }
}