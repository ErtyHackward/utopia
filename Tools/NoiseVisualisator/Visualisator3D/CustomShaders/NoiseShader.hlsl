//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

//Constant buffer with values updated once per Frame, it is mostly shared across all various Shader
cbuffer PerFrameShared
{
	matrix ViewProjection;		//View matrix * Projection Matrix
};

//Constant buffer that will have its data refresh before every Draw
cbuffer PerDraw
{
	matrix World;				//World Matrix that define the world position of the passed in object (These object are in "local coordinate", not world)
};

//--------------------------------------------------------------------------------------
// Inputs layout
//--------------------------------------------------------------------------------------
//Vertex shader Input Layout
struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
};

//Pixel shader Input Layout
struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
PS_IN VS( VS_IN input )
{
	PS_IN output;
	
	output.pos = mul(mul(input.pos, World), ViewProjection);
	output.col = input.col;
	
	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS( PS_IN input ) : SV_Target
{
	return input.col;
}