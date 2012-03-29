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
	float3 DiffuseLightDirection;
}

static const float4 DiffuseColor = {1.0f, 1.0f, 1.0f, 1.0f };
static const float4 AmbientColor = {0.1f, 0.1f, 0.1f, 1.0f };

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

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PSColoredCubeWithLight( PS_IN input ) : SV_Target
{
	// Sample our texture at the specified texture coordinates to get the texture color
	float4 texColor = Color;

	float3 lightdir = normalize( DiffuseLightDirection );
	float3 norm = normalize( input.Normal );
	float4 diffuse = dot( lightdir, norm ) * DiffuseColor;

	float4 color = texColor * ( diffuse + AmbientColor );
	color.a = texColor.a;

	return color;
}
