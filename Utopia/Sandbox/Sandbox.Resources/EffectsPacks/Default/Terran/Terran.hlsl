//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	matrix World;
	float PopUpValue;
};

#include <SharedFrameCB.hlsl>

static const float foglength = 20;
static float3 Dayfogcolor = {0.7, 0.7, 0.7 };
static float3 Nightfogcolor = {0, 0, 0 };

//face Types
//Back = 0,
//Front = 1,
//Bottom = 2,
//Top = 3,
//Left = 4,
//Right = 5

static const float texmul1[6] = { -1,  1, -1,  1,  0,  0};
static const float texmul2[6] = {  0,  0,  0,  0, -1,  1};
static const float texmul3[6] = { -1, -1,  0,  0, -1, -1};		
static const float texmul4[6] = {  0,  0,  1,  1,  0,  0};
static const float faceshades[6] = { 0.6, 0.6, 0.8, 1.0, 0.7, 0.8 };

static const float4 faceSpecialOffset[6] = { {0.0f,0.0f,0.0625f,0.0f} , {0.0f,0.0f,-0.0625f,0.0f}, {0.0f,0.0f,0.0f,0.0f}, {0.0f,0.0f,0.0f,0.0f}, {0.0625f,0.0f,0.0f,0.0f}, {-0.0625f,0.0f,0.0f,0.0f} };

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray TerraTexture;
Texture2D SkyBackBuffer;
Texture2DArray BiomesColors;
SamplerState SamplerBackBuffer;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	uint4 Position		 : POSITION;
	float4 Col			 : COLOR;
	uint4 VertexInfo	 : INFO;   // (bool)x = is Upper vertex, y = facetype, z = AOPower factor 255 = Factor of 3, w = Offset
	float2 BiomeData     : BIOMEINFO; //X = Temperature, Y = Moisture
	uint2 Various		 : VARIOUS;   //X = ArrayTextureID for Biome, Y SideOffset multiplier
};

struct PS_IN
{
	float4 Position				: SV_POSITION;
	float3 UVW					: TEXCOORD0;
	float fogPower				: VARIOUS0;
	float3 EmissiveLight		: Light0;
	float2 BiomeData			: BIOMEDATA0;
	uint2 Various				: BIOMEDATAVARIOUS0;  
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

PS_IN VS(VS_IN input)
{
    PS_IN output;
	
	float4 newPosition = {input.Position.xyz, 1.0f};
	if(input.Various.y > 0)
	{
		newPosition += (faceSpecialOffset[input.VertexInfo.y] * input.Various.y);
	}

	float YOffset = 0;
	if(input.VertexInfo.x == 1) YOffset = (input.VertexInfo.w/255.0f);
	newPosition.y -= (YOffset + (PopUpValue * 128));

    float4 worldPosition = mul(newPosition, World);
	output.Position = mul(worldPosition, ViewProjection);

	int facetype = input.VertexInfo.y;
	//Compute the texture mapping
	output.UVW = float3(
						(input.Position.x * texmul1[facetype]) + (input.Position.z * texmul2[facetype]), 
						((input.Position.y * texmul3[facetype]) + YOffset) + (input.Position.z * texmul4[facetype]),
						input.Position.w );

	output.EmissiveLight = saturate(input.Col.rgb +  SunColor * input.Col.a);
	output.EmissiveLight *= faceshades[facetype];

	output.fogPower = 1 - (clamp( ((length(worldPosition.xyz) - fogdist) / foglength), 0, 1));
	output.BiomeData = input.BiomeData;
	output.Various = input.Various;

    return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
PS_OUT PS(PS_IN input)
{
	PS_OUT output;

	float fogvalue = input.fogPower;
	if(FogType != 2.0) clip(fogvalue <= 0.001 ? -1:1);  //Clip if fog is complete

	float4 color = TerraTexture.Sample(SamplerDiffuse, input.UVW);
	
	clip(color.a < 0.1f ? -1:1 );    //Remove the pixel if alpha < 0.1

	//Apply Biome Color if the Alpha is < 1
	if(color.a < 1.0)
	{
		float3 samplingBiomeColor = {input.BiomeData.xy, input.Various.x };
	    float4 biomeColor =  BiomesColors.Sample(SamplerBackBuffer, samplingBiomeColor);
		color.r = color.r * biomeColor.r;
		color.g = color.g * biomeColor.g;
		color.b = color.b * biomeColor.b;
	}

	color = color * float4(input.EmissiveLight, 1);

	float4 finalColor = color;
	//To execute only when Fog is present !
	if(fogvalue < 1){
		float4 backBufferColor = { 0.0f, 0.0f, 0.2f, 1.0f }; //Defaulted Set to underWaterColor
		if(Various.x != 1) //Not Under Water and Fog present
		{
			if(FogType == 0.0)
			{
				//Get sky Color
				float2 backBufferSampling = {input.Position.x / BackBufferSize.x , input.Position.y / BackBufferSize.y};
				backBufferColor = SkyBackBuffer.Sample(SamplerBackBuffer, backBufferSampling);
			}else{
				if(FogType == 1.0)
				{
					backBufferColor.xyz = SunColor / 1.5;
					backBufferColor.w = color.a;
				}
			}
		}

		//Compute Transparency, and blend current color with sky color in a blended way
		if(FogType != 2.0)
		{
			finalColor.rgb = (color.rgb * fogvalue) + (backBufferColor.rgb * (1 - fogvalue));
		}else{
			finalColor = color;
		}

	}

	// Apply fog on output color
	output.Color = finalColor;
    return output;
}

