
texture draw;
sampler2D drawS = sampler_state { texture = <draw>; };

texture distort;
sampler2D distortS = sampler_state { texture = <distort>; AddressU = wrap; AddressV = wrap; };

texture light;
sampler2D lightS = sampler_state { texture = <light>; AddressU = wrap; AddressV = wrap; };

float2 drawSize;
float colorSampleY;
float time;
float screenWidth;
float xOff;
float4x4 zoom;

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

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.coord = input.coord;
	output.Position = mul(input.Position, zoom);
	return output;
}

float4 main(VertexShaderOutput input) : COLOR
{
	float2 st = input.coord;

	float2 move = float2(fmod(time / 10.0, 1.0), 0.0);

	float2 dist = float2(sin(1.57 + time + st.x * 5.0) * 0.01, sin(time * 3.0 + st.x * 23.0) * 0.02);
	dist += tex2D(distortS, st + float2(sin(1.57 + time) * 5., sin(time)) * 0.02).xy * 0.02;

	float off = tex2D(drawS, st + dist).x;
	float off2 = tex2D(distortS, st + dist).x;

	float3 color = tex2D(lightS, float2(xOff + st.x * (drawSize.x / screenWidth), colorSampleY)).xyz;
	float3 colorOut = color * (off * st.y * off2);
	return float4(colorOut.xyz, (colorOut.x + colorOut.y + colorOut.z) / 3.0);
}

technique Technique1
{
	pass Shade
	{
		PixelShader = compile ps_2_0 main();
	}
}