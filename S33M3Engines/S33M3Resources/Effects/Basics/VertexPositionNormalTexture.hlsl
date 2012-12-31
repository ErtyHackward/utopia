//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerDraw
{
	matrix World;
}

cbuffer PerFrame
{
	matrix View;
	matrix Projection;
	float Alpha;
}

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2D DiffuseTexture;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	float3 Pos : POSITION;
	float3 Norm : NORMAL;
	float3 UV  : TEXCOORD;
};

//Pixel shader Input
struct PS_IN
{
	float4 Pos : SV_POSITION;
	float2 UV  : TEXCOORD;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
//[VS ENTRY POINT]
PS_IN VS( VS_IN input )
{
	PS_IN output;
	
	output.Pos = float4(input.Pos.xyz, 1);
	output.Pos = mul( output.Pos, World );
	output.Pos = mul( output.Pos, View );
	output.Pos = mul( output.Pos, Projection );
	output.UV = input.UV;
	
	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
//[PS ENTRY POINT]
float4 PS( PS_IN input ) : SV_Target
{
	float4 color = DiffuseTexture.Sample(SamplerDiffuse, input.UV);
	color.a *= Alpha;

	return color;
}
