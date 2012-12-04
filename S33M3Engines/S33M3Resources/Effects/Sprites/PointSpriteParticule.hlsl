//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

#include <SharedFrameCB.hlsl>

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray DiffuseTexture;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	float3 Position				: POSITION;   //XYZ world location
	uint4  Info					: INFO;		  //X = Particule texture index, Y = Particule size
};

//--------------------------------------------------------------------------------------
//Geometry shader Input
struct GSInput {
	float3 Position				: POSITION;   //XYZ world location
	uint4  Info					: INFO;		  //X = Particule texture index, Y = Particule size
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
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
GSInput VS (VSInput input)
{
	return input;
}

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

		billboardPosition = mul(billboardPosition, InvertedOrientation);

		//Scale to billboard local size
		billboardPosition.xy *= Input.Info.y; 
		billboardPosition.xyz += Input.Position.xyz;  //Already in world position

		//Rotating the billboard to make it face the camera, and project it against the Screen
		float4 WorldPosition = mul(billboardPosition, ViewProjection);

		Output.Position = WorldPosition;
		Output.UVW = float3( texcoord[i].x, 
							 texcoord[i].y,
							 Input.Info.x);

		//Transform point in screen space
		TriStream.Append( Output );
	}
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(PSInput IN) : SV_Target
{	
	//Texture Sampling
	float4 color = DiffuseTexture.Sample(SamplerDiffuse, IN.UVW);

	clip( color.a < 0.01f ? -1:1 ); //Remove the pixel if alpha < 0.1

	return color;
}

//--------------------------------------------------------------------------------------
