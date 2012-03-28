//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerDraw
{
	matrix World;
	float4 Color;
}

cbuffer PerFrame
{
	matrix View;
	matrix Projection;
}

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	float3 Pos : POSITION;
	float3 Normal : NORMAL;
	float3 UVW  : TEXCOORD;
};

//Pixel shader Input
struct PS_IN
{
	float4 Pos : SV_POSITION;
	float3 Normal : NORMAL;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
PS_IN VS( VS_IN input )
{
	PS_IN output;
	
	output.Pos = float4(input.Pos.xyz, 1);
	output.Pos = mul( output.Pos, World );
	output.Pos = mul( output.Pos, View );
	output.Pos = mul( output.Pos, Projection );
	output.Normal = mul( float4(input.Normal,1), World ).xyz; 
	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PSColoredCube( PS_IN input ) : SV_Target
{
	return Color;
}
