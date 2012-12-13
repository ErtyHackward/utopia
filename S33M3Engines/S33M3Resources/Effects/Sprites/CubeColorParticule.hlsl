//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

#include <SharedFrameCB.hlsl>

static const float faceshades[6] = { 0.6, 0.6, 0.8, 1.0, 0.7, 0.8 };

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	float4 Position				: POSITION;   //XYZ world location, W = texture array indice
	matrix Tranform				: TRANSFORM;
	float4 Color				: COLOR0;
	float4 AmbiantColor			: COLOR1;     //XY : Size
};

struct GSInput {
	float4 Position	 			: POSITION;
	float4 Color				: COLOR;
	float3 AmbiantColor			: LIGHT0;
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
GSInput VS (VSInput input)
{
	GSInput output;
	
	float4 newPosition = {input.Position.xyz, 1.0f};
	output.Position = mul(newPosition, input.Tranform); //Apply transformation to the cube vertex (scale, rotation, WorldTranslation)
	output.Position = mul( output.Position, ViewProjection ); //World => Camera => Screen space
	output.Color = input.Color;
	output.AmbiantColor = saturate(input.AmbiantColor.rgb +  SunColor * input.AmbiantColor.a);
	
	return output;
}

[maxvertexcount(3)]
void GS(triangle GSInput Inputs[3], uint primID : SV_PrimitiveID, inout TriangleStream<PSInput> TriStream)
{
	Inputs[0].AmbiantColor *= faceshades[(primID % 12) / 2];
	Inputs[1].AmbiantColor = Inputs[0].AmbiantColor;
	Inputs[2].AmbiantColor = Inputs[0].AmbiantColor;

	TriStream.Append(Inputs[0]);
	TriStream.Append(Inputs[1]);
	TriStream.Append(Inputs[2]);
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
