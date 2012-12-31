//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	matrix World;
	matrix ViewProjection;
	float4 Color;
};

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	float3 Position		 : POSITION;
};

struct PS_IN
{
	float4 Position				: SV_POSITION;
};

//[VS ENTRY POINT]
//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
PS_IN VS( VS_IN input )
{
	PS_IN output;
	
	output.Position = float4(input.Position.xyz, 1);
	output.Position = mul( output.Position, World );
	output.Position = mul( output.Position, ViewProjection );
	
	return output;
}

//[PS ENTRY POINT]
//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS( PS_IN input ) : SV_Target
{
	return Color;
}
