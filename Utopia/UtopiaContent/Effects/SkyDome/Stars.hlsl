//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	matrix World;
	matrix ViewProjection;
	float Visibility;
};

// offsets for the 4 vertices
static const float2 p[4] = {
							{-0.5, -0.5},
							{-0.5, 0.5},
							{0.5, -0.5},
							{0.5, 0.5}
						};

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	float3 Position				: POSITION;
	float4 Info					: COLOR; // r = Stars size
};

//--------------------------------------------------------------------------------------
//Geometry shader Input
struct GSInput {
	float3 Position				: POSITION;
	float4 Info					: COLOR; // r = Stars size
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
GSInput VS (VSInput input)
{
	return input;
}

[maxvertexcount(4)]
void GS( point GSInput Input[1]: POSITION0, inout TriangleStream<PSInput> TriStream )
{
	PSInput Output;

	float4 screenPosition;
	
	screenPosition = mul(float4(Input[0].Position.xyz, 1), ViewProjection);
	
	
	// *****************************************************
	// generate the 4 vertices to make two triangles
	for( uint i = 0 ; i < 4 ; i++ )
	{
		Output.Position.zw = screenPosition.zw;
		Output.Position.x = screenPosition.x + (p[i].x * Input[0].Info.r * 7);
		Output.Position.y = screenPosition.y + (p[i].y * Input[0].Info.r * 7);

		TriStream.Append( Output );
	}

	TriStream.RestartStrip();
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(PSInput IN) : SV_Target
{	
	return float4(1,1,1,1 * Visibility);	
}

//--------------------------------------------------------------------------------------
