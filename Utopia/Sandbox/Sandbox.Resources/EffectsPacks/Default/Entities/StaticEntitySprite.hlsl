//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerFrameLocal
{
	matrix WorldFocus;
	matrix View;
    float3 WindPower;
	float keyFrameAnimation;
};

#include <SharedFrameCB.hlsl>

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray DiffuseTexture;
SamplerState SamplerDiffuse;
Texture2DArray BiomesColors;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	float4 Position				: POSITION;
	float4 Color				: COLOR;
	float3 Textcoord     		: TEXCOORD;
	float3 MetaData     		: METADATA; //Billboard Size x-y-z
	float2 BiomeData		    : BIOMEINFO; //X = Temperature, Y = Moisture
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
	float3 UVW					: TEXCOORD0;
	float fogPower				: VARIOUS0;
	float3 EmissiveLight		: Light0;
	float2 BiomeData			: BIOMEDATA0;
};

struct PS_OUT
{
	float4 Color				: SV_TARGET0;
};

//--------------------------------------------------------------------------------------

static const float foglength = 45;

//Billboard corners, 0 being no billboards
static const float3 billboardCorners[5] = {
											{0.0f, 0.0f, 0.0f},
											{-0.5, 1.0f, 0.0f},
											{0.5, 1.0f, 0.0f},
											{0.5, 0.0f, 0.0f},
											{-0.5, 0.0f, 0.0f}
										  };

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
PSInput VS (VSInput input)
{
	PSInput output;
	float4 worldPosition;

	[branch] if(input.Position.w > 0) //If i'm a billboard
	{
		//Get the billboard template corner
		float3 billboardVertexPosition = billboardCorners[input.Position.w];
		//Multiply the template by the Billboard size (Scale)
		billboardVertexPosition *= input.MetaData;

		//Rotating it
		billboardVertexPosition = float3(mul(billboardVertexPosition, (float3x3)View)); //Rotate the billboard following Camera position

		//Translating
		worldPosition = float4(billboardVertexPosition.xyz + input.Position.xyz, 1.0f);
	}else{ 
		//I'm not a billboard
		worldPosition = float4(input.Position.xyz, 1.0f);
	}


	worldPosition = mul(worldPosition, WorldFocus); //Translate to vertex to the correct location
	output.Position = mul(worldPosition, ViewProjection);
	output.UVW = input.Textcoord;

	output.BiomeData = input.BiomeData;
		              
	output.fogPower = clamp( ((length(worldPosition.xyz) - fogdist) / foglength), 0, 1);
	output.EmissiveLight = saturate(input.Color.rgb +  SunColor * input.Color.a);

	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
PS_OUT PS(PSInput IN)
{	
   //Don't Display at all entity if in the fog
   clip( IN.fogPower > 0.01f ? -1:1 );

	PS_OUT output;
	//Texture Sampling
	float4 color = DiffuseTexture.Sample(SamplerDiffuse, IN.UVW) * float4(IN.EmissiveLight, 1);
	
	clip( color.a < 0.1f ? -1:1 ); //Remove the pixel if alpha < 0.1

	//Apply Biome Color if the Alpha is < 1
	if(color.a < 1.0)
	{
		float3 samplingBiomeColor = {IN.BiomeData.xy, 0 };
	    float4 biomeColor =  BiomesColors.Sample(SamplerDiffuse, samplingBiomeColor);
		color.r = color.r * biomeColor.r;
		color.g = color.g * biomeColor.g;
		color.b = color.b * biomeColor.b;
	}

	output.Color = color;
    return output;
}

//--------------------------------------------------------------------------------------
