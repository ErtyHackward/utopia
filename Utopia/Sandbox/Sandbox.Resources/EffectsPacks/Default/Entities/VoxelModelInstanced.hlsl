//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer VoxelModelPerFrame
{
	matrix ViewProjectionOLD;
	float3 LightDirection;		//diffuse light direction
};

cbuffer VoxelModel
{
	float4 colorMapping[64];
}

#include <SharedFrameCB.hlsl>

//	cube face						ba	F	Bo	T	L   R
static const float normalsX[6] = {  0,  0,  0,  0, -1,  1};
static const float normalsY[6] = {  0,  0, -1,  1,  0,  0};
static const float normalsZ[6] = { -1,  1,  0,  0,  0,  0};		

static const float faceshades[6] = { 0.6, 0.6, 0.8, 1.0, 0.7, 0.8 };

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	uint4 Position		: POSITION;
	uint4 faceType    	: INFO;
	matrix Transform	: TRANSFORM;
	float3 LightColor	: COLOR; // Diffuse lighting color
};

struct PS_IN
{
	float4 Position				: SV_POSITION;
	float Light					: Light1;
	float3 LightColor			: Light2;
	float EmissiveLight         : Light0;
	int colorIndex              : VARIOUS1;
};

struct PS_OUT
{
	float4 Color				: SV_TARGET0;
};

//--------------------------------------------------------------------------------------
// Fonctions
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Vertex Shaders
//--------------------------------------------------------------------------------------
//[VS ENTRY POINT]
PS_IN VS(VS_IN input)
{
    PS_IN output;
	
	output.colorIndex = input.Position.w - 1;

	float4 newPosition = {input.Position.xyz, 1.0f};
    float4 worldPosition = mul(newPosition, input.Transform);

	worldPosition.x += (newPosition.y / 16 * Various2.x);
	worldPosition.z += (newPosition.y / 16 * Various2.z);

	output.Position = mul(worldPosition, ViewProjectionOLD);


	int facetype = input.faceType.x;

	// ambient occlusion value	
	output.Light = input.faceType.y;
	output.LightColor = input.LightColor;

	// fake shadow
	output.EmissiveLight = faceshades[facetype];
    return output;
}	

//--------------------------------------------------------------------------------------
// Pixel Shader
//-------------------------------------------------------------------------------------
//[PS ENTRY POINT]
PS_OUT PS(PS_IN input)
{
	PS_OUT output;

	float intensity = input.Light / 255;
	float3 color = colorMapping[input.colorIndex].rgb * input.EmissiveLight * input.LightColor * intensity;
	output.Color = float4(color,1);

    return output;
}

