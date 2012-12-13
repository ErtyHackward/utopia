//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

#include <SharedFrameCB.hlsl>

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	float4 Position				: POSITION;   //XYZ world location, W = texture array indice
	matrix Tranform				: TRANSFORM;
	float4 Color				: COLOR0;
	float4 AmbiantColor			: COLOR1;     //XY : Size
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
	float4 Color				: COLOR;
	float3 AmbiantColor		: LIGHT0;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
PSInput VS (VSInput input)
{
	PSInput output;
	
	output.Position = mul(input.Position, input.Tranform); //Apply transformation to the cube vertex (scale, rotation, WorldTranslation)
	output.Position = mul( output.Position, ViewProjection ); //World => Camera => Screen space
	output.Color = input.Color;
	output.AmbiantColor = input.AmbiantColor;
	
	return output;
}


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(PSInput IN) : SV_Target
{	
	//Texture Sampling
	float4 color = IN.Color;

	clip(color.a < 0.01f ? -1:1 ); //Remove the pixel if alpha < 0.1

	color *= float4(IN.ColorReceived, 1);

	return color;
}

//--------------------------------------------------------------------------------------
