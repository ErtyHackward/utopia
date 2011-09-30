//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerFrame
{
	matrix WorldFocus;
	matrix ViewProjection;
    float WindPower;
	float3 SunColor;			  // Diffuse lighting color
	float fogdist;
};

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray DiffuseTexture;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	uint4 Info					: INFO;     //Billboard size
	float3 Position				: POSITION; //w = Array texture index
};

//--------------------------------------------------------------------------------------
//Geometry shader Input
struct GSInput {
	uint4 Info					: INFO;
	float3 Position				: POSITION;
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
	float3 UVW					: TEXCOORD0;
	float fogPower				: VARIOUS0;
	float3 EmissiveLight		: Light0;
};


static const float foglength = 45;
static float3 Dayfogcolor = {0.7, 0.7, 0.7 };
static float3 Nightfogcolor = {0, 0, 0 };

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

		Output.fogPower = clamp(((length(Output.Position.xyz) - fogdist) / foglength), 0, 1);
		//Transform point in screen space
		Output.Position = mul(mul(Output.Position, WorldFocus), ViewProjection);
		Output.EmissiveLight = saturate(SunColor); //I should multiple the sun color by the qt of light received by the entity (depend on the block where its located) TODO
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
	float4 color = DiffuseTexture.Sample(SamplerDiffuse, IN.UVW) * float4(IN.EmissiveLight, 1);;
	
	clip( color.a < 0.1f ? -1:1 ); //Remove the pixel if alpha < 0.1

	float4 Finalfogcolor = {SunColor / 1.5, color.a};
	color = lerp(color, Finalfogcolor, IN.fogPower);

	return color;	
}

//--------------------------------------------------------------------------------------
