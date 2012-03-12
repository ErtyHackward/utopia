//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerFrame
{
	matrix WorldFocus;
	matrix ViewProjection;
    float WindPower;
};

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray DiffuseTexture;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	float3 Position				: POSITION; //w = Array texture index
	uint4 Info					: INFO;     //Billboard size
};

//--------------------------------------------------------------------------------------
//Geometry shader Input
struct GSInput {
	float3 Position				: POSITION;
	uint4 Info					: INFO;
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
	float3 UVW					: TEXCOORD0;
};


// offsets for the 4 vertices
static const float3 p[4] = {
							{-0.5, 0.0f, 0.0f},
							{-0.5, 1.0f, 0.0f},
							{0.5, 0.0f, 0.0f},
							{0.5, 1.0f, 0.0f}
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

	float4 PointSpritePosition = float4(Input[0].Position, 1);
	
	// *****************************************************
	// generate the 4 vertices to make two triangles
	for( uint i = 0 ; i < 4 ; i++ )
	{
		Output.UVW = float3( texcoordU[i], 
							 texcoordV[i],
							 Input[0].Info.x);

		Output.Position.x = PointSpritePosition.x + (p[i].x * Input[0].Info.y) ;// + (p[i].z * 1); //Add Offset to build the Quad X dim
		Output.Position.y = PointSpritePosition.y + (p[i].y * Input[0].Info.y) ; //Add Offset to build the Quad Y dim
		Output.Position.z = PointSpritePosition.z + (p[i].z * 1);
		Output.Position.w = 1.0f;

		//Transform into Projection space
		//Add Rotation matrix computation if needed here !

		//Transform point in screen space
		Output.Position = mul(mul(Output.Position, WorldFocus), ViewProjection);
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
	
	clip( color.a < 0.1f ? -1:1 ); //Remove the pixel if alpha < 0.1

	return color;	
}

//--------------------------------------------------------------------------------------
