//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

#include <SharedFrameCB.hlsl>

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	float4 Position				: POSITION;   //XYZ world location, W = texture array indice
	float4 Color				: COLOR0;
	float4 ColorReceived		: COLOR1;
	float2 Size					: SIZE;       //XY : Size
};

//--------------------------------------------------------------------------------------
//Geometry shader Input
struct GSInput {
	float4 Position				: POSITION;   //XYZ world location, W = texture array indice
	float4 Color				: COLOR0;
	float4 ColorReceived		: COLOR1;
	float2 Size					: SIZE;       //XY : Size
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
	float4 Color				: COLOR;
	float3 ColorReceived		: LIGHT0;
};


//Billboard corners, 0 being no billboards
static const float4 billboardCorners[4] = {
											{0.5, 0.0f, 0.0f, 1.0f},  //Botom right corner
											{-0.5, 0.0f, 0.0f, 1.0f}, //Botom left corner
											{0.5, 1.0f, 0.0f, 1.0f},  //Top right corner
											{-0.5, 1.0f, 0.0f, 1.0f}  //Top left corner
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
		Output.ColorReceived = saturate(Input.ColorReceived.rgb +  SunColor * Input.ColorReceived.a);

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
	float4 color = IN.Color;

	clip(color.a < 0.01f ? -1:1 ); //Remove the pixel if alpha < 0.1

	color *= float4(IN.ColorReceived, 1);

	return color;
}

//--------------------------------------------------------------------------------------
