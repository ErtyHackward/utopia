//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerFrame
{
	matrix ViewProjection;         //Normal ViewProjection
	matrix InvertedOrientation;		//The inverted Matrix view
}

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray DiffuseTexture;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	float4 Position				: POSITION;   //XYZ world location, W = texture array indice
	float4 Color				: COLOR;
	float2 Size					: SIZE;       //XY : Size, Z = Geometry Type (0 = Position Facing Billboard, 1 = View Facing Billboard)
};

//--------------------------------------------------------------------------------------
//Geometry shader Input
struct GSInput {
	float4 Position				: POSITION;   //XYZ world location, W = texture array indice
	float4 Color				: COLOR;
	float2 Size					: SIZE;       //XY : Size, Z = Geometry Type (0 = Position Facing Billboard, 1 = View Facing Billboard)
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
	float4 Color				: COLOR;
	float3 UVW					: TEXCOORD0;
};


//Billboard corners, 0 being no billboards
static const float4 billboardCorners[4] = {
											{0.5, 0.0f, 0.0f, 1.0f},  //Botom right corner
											{-0.5, 0.0f, 0.0f, 1.0f}, //Botom left corner
											{0.5, 1.0f, 0.0f, 1.0f},  //Top right corner
											{-0.5, 1.0f, 0.0f, 1.0f}  //Top left corner
										  };

static const float2 texcoord[4] = { 
									{ 1 , 1 },
									{ 0 , 1 },
									{ 1 , 0 },
									{ 0 , 0 }
								  };

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
//[VS ENTRY POINT]
GSInput VS (VSInput input)
{
	return input;
}

//[GS ENTRY POINT]
[maxvertexcount(4)]
void GS(point GSInput Inputs[1]: POSITION0, inout TriangleStream<PSInput> TriStream)
{
	PSInput Output;
	GSInput Input = Inputs[0];
	
	// *****************************************************
	// generate the 4 vertices to make two triangles
	for( uint i = 0 ; i < 4 ; i++ )
	{
		//Get the billboard template corner
		float4 billboardPosition = billboardCorners[i];

		//Scale to billboard
		billboardPosition.xy *= Input.Size; 

		//Rotating the billboard to make it face the camera
		billboardPosition = mul(billboardPosition, InvertedOrientation);

		//Translation billboard to world position
		billboardPosition.xyz += Input.Position.xyz; 

		//and project it against the Screen
		float4 WorldPosition = mul(billboardPosition, ViewProjection);

		Output.Position = WorldPosition;
		Output.Color = Input.Color;
		Output.UVW = float3( texcoord[i].x, 
							 texcoord[i].y,
							 Input.Position.w);

		//Transform point in screen space
		TriStream.Append( Output );
	}
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
//[PS ENTRY POINT]
float4 PS(PSInput IN) : SV_Target
{	
	//Texture Sampling
	float4 color = DiffuseTexture.Sample(SamplerDiffuse, IN.UVW);

	clip( color.a < 0.01f ? -1:1 ); //Remove the pixel if alpha < 0.1

	return color;
}

//--------------------------------------------------------------------------------------
