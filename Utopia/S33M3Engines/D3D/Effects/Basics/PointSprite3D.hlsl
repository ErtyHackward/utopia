//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	matrix World;
};

cbuffer PerFrame
{
	matrix ViewProjection;
};

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2D DiffuseTexture;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	uint4 Position				: POSITION; //w = Array texture index
	uint4 Info					: INFO;     //Billboard size
};

//--------------------------------------------------------------------------------------
//Geometry shader Input
struct GSInput {
	uint4 Position				: POSITION;
	uint4 Info					: INFO;
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
	float3 UVW					: TEXCOORD0;
};


// offsets for the 4 vertices
static const float2 p[4] = {
							{-0.5, 0.0f},
							{-0.5, 1.0f},
							{0.5, 0.0f},
							{0.5, 1.0f}
						};

static const float texcoordU[4] = { 0.0f, 0.0f, 1.0f, 1.0f};
static const float texcoordV[4] = { 1.0f, 0.0f, 1.0f, 0.0f};	

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
GSInput VS (VSInput input)
{
	return input;
}

[maxvertexcount(4)]
void GS(point GSInput Input[1]: POSITION0, inout TriangleStream<PSInput> TriStream )
{
	PSInput Output;

	float4 PointSpritePosition = Input[0].Position;
	
	// *****************************************************
	// generate the 4 vertices to make two triangles
	for( uint i = 0 ; i < 4 ; i++ )
	{
		Output.UVW = float3( texcoordU[i], 
							 texcoordV[i],
							 PointSpritePosition.w);

		Output.Position.w = 1.0f;
		Output.Position.z = PointSpritePosition.z;
		Output.Position.x = PointSpritePosition.x + (p[i].x * Input[0].Info.x); //Add Offset to build the Quad X dim
		Output.Position.y = PointSpritePosition.y + (p[i].y * Input[0].Info.x); //Add Offset to build the Quad Y dim

		//Transform into Projection space
		//Add Rotation matrix computation if needed here !

		//Transform point in screen space
		Output.Position = mul(mul(Output.Position, World), ViewProjection);
		TriStream.Append( Output );
	}

	TriStream.RestartStrip();
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(PSInput IN) : SV_Target
{	
	//Texture Sampling
	float4 color = DiffuseTexture.Sample(SamplerDiffuse, IN.UVW);
	return color;	
}

//--------------------------------------------------------------------------------------
