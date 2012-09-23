//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerDraw
{
	matrix Screen;
	matrix Projection;
	float3 LightColor;
}

static const float4 DiffuseColor = {1.0f, 1.0f, 1.0f, 1.0f };
static const float4 AmbientColor = {0.2f, 0.2f, 0.2f, 1.0f };

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray DiffuseTexture;
SamplerState SamplerDiffuse;

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
	float3 UVW  : TEXCOORD;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
PS_IN VS( VS_IN input )
{
	PS_IN output;
	
	output.Pos = float4(input.Pos.xyz, 1);
	output.Pos = mul( output.Pos, Screen );
	output.Pos = mul( output.Pos, Projection );
	output.UVW = input.UVW;
	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS( PS_IN input ) : SV_Target
{
	// Sample our texture at the specified texture coordinates to get the texture color
	float4 texColor = DiffuseTexture.Sample(SamplerDiffuse, input.UVW);

	float4 color = float4(texColor.rgb * LightColor,1);

	return color;
}