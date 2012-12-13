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
	float3 AmbiantColor			: LIGHT0;
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
	output.AmbiantColor = saturate(Input.AmbiantColor.rgb +  SunColor * Input.AmbiantColor.a);
	
	return output;
}


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(PSInput IN) : SV_Target
{	
	//Texture Sampling
	float4 color = IN.Color;

	color *= float4(IN.AmbiantColor, 1);

	return color;
}

//--------------------------------------------------------------------------------------
