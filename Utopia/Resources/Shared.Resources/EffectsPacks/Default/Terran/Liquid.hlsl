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

#define E 2.71828

static const float texmul1[6] = { -1,  1, -1,  1,  0,  0};
static const float texmul2[6] = {  0,  0,  0,  0, -1,  1};
static const float texmul3[6] = { -1, -1,  0,  0, -1, -1};
static const float texmul4[6] = {  0,  0,  1,  1,  0,  0};

//face Types
//Back = 0,
//Front = 1,
//Bottom = 2,
//Top = 3,
//Left = 4,
//Right = 5
static const float3 facenormals[6] = {
												{0,0,1},
												{0,0,1},
												{0,1,0},
												{0,1,0},
												{1,0,0},
												{1,0,0}
												};

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray TerraTexture;
Texture2DArray BiomesColors;
Texture2DArray AnimatedTextures;
Texture2D SkyBackBuffer;

SamplerState SamplerDiffuse;
SamplerState SamplerBackBuffer;
SamplerState SamplerOverlay;
//--------------------------------------------------------------------------------------
//Vertex shader Input

struct VS_LIQUID_IN
{
	uint4 Position		 : POSITION;   // X = XPosi, Y = YPosi, Z = ZPosi,  W =  Y Modified block Height modificator
	float4 Col			 : COLOR;
	uint4 VertexInfo1	 : INFO;       // x = FaceType, (bool)y = is Upper vertex, Z = Biome Texture Id,
	float2 BiomeInfo	 : BIOMEINFO;  // x = Moisture, y = Temperature
	uint2 Animation      : ANIMATION;  // x = animation Speed, y = Animation NbrFrames
	uint ArrayId         : ARRAYID;
	uint Dummy           : DUMMY;
};

struct PS_IN
{
	float4 Position				: SV_POSITION;
	float3 StaticUVW			: TEXCOORD0;
	float fogPower				: VARIOUS0;
	float causticPower			: VARIOUS1;
	float4 EmissiveLight		: Light0;
	float2 BiomeData			: BIOMEDATA0;
	uint2 Various				: BIOMEDATAVARIOUS0;
	float3 AnimationUVW			: TEXCOORD1;
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
PS_IN VS_LIQUID(VS_LIQUID_IN input)
{
    PS_IN output;
	
	float4 newPosition = {input.Position.xyz, 1.0f};
	float YOffset = 0;
	if (input.VertexInfo1.y == 1) YOffset = input.Position.w / 255.0f;
	newPosition.y -= (YOffset + (PopUpValue * 128));

    float4 worldPosition = mul(newPosition, World);
	output.Position = mul(worldPosition, ViewProjection_focused);

	int facetype = input.VertexInfo1.x;
	//Compute the texture mapping
	output.StaticUVW = float3(
						(input.Position.x * texmul1[facetype]) + (input.Position.z * texmul2[facetype]), 
						((input.Position.y * texmul3[facetype]) + YOffset) + (input.Position.z * texmul4[facetype]),
						input.ArrayId);

	//Animate texture !
	if (input.Animation.y > 0)
	{
		int animationFrame = ((TextureFrameAnimation * input.Animation.x) % input.Animation.y);
		output.StaticUVW.z += animationFrame;
	}

	output.AnimationUVW = float3(output.StaticUVW.xy / 8.0f, Various.y * 61);

	output.EmissiveLight.rgb = saturate(input.Col.rgb +  SunColor * input.Col.a);

	float3 facenorm = facenormals[facetype];
	output.EmissiveLight.a = (1 - abs(dot(normalize(worldPosition.xyz), facenorm))) * 1.3 ;
	output.EmissiveLight.a = clamp(output.EmissiveLight.a, 0.6, 1);

	output.fogPower = 1 - (clamp( ((length(worldPosition.xyz) - fogdist) / foglength), 0, 1));
	output.causticPower = clamp( ((length(worldPosition.xyz) - 30) / 20), 0, 1);
	if(facetype != 3) output.causticPower = 1;
	output.BiomeData = (input.BiomeInfo.xy * 0.6f) + 0.2f;
	output.BiomeData.x = saturate(output.BiomeData.x + WeatherGlobalOffset.x);
	output.BiomeData.y = saturate(output.BiomeData.y + WeatherGlobalOffset.y);
	output.Various.x = input.VertexInfo1.z;

    return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
//[PS ENTRY POINT]
PS_OUT PS(PS_IN input)
{
	PS_OUT output;

	float fogvalue = input.fogPower;
	if (FogType != 2.0)
	{
		if (fogvalue <= 0.001)
		{
			clip(-1);
			return output;
		}
	}

	//The alpha value is following the Angle of view to the face
	float4 colorInput = float4(TerraTexture.Sample(SamplerDiffuse, input.StaticUVW).rgb, 1) * input.EmissiveLight;

	//Change the water surface with the biomeColor
	float3 biomeColorSampling = { input.BiomeData.x, input.BiomeData.y, input.Various.x };
	float4 biomeColor =  BiomesColors.Sample(SamplerBackBuffer, biomeColorSampling);
	colorInput.r = colorInput.r * biomeColor.r;
	colorInput.g = colorInput.g * biomeColor.g;
	colorInput.b = colorInput.b * biomeColor.b;
	
	float2 backBufferSampling = {input.Position.x / BackBufferSize.x , input.Position.y / BackBufferSize.y};
	
	float4 color = colorInput;

	//Add overlay only when needed
	if(input.causticPower < 1)
	{
		color.rgb += ((AnimatedTextures.Sample(SamplerOverlay, input.AnimationUVW.xyz).r * (1 - input.causticPower)) * color.rgb) * 2;
	}

	//To execute only when Fog is present !
	if(fogvalue < 1)
	{
		float4 backBufferColor;
		if(FogType == 0.0)
		{
			//Sample BackGround Sky
			backBufferColor = SkyBackBuffer.Sample(SamplerBackBuffer, backBufferSampling);
			color.rgb = (color.rgb * fogvalue) + (backBufferColor.rgb * (1 - fogvalue));
		}else{
			if(FogType == 1.0)
			{
				backBufferColor.xyz = SunColor / 1.5;
				backBufferColor.w = color.a;
				color.rgb = (color.rgb * fogvalue) + (backBufferColor.rgb * (1 - fogvalue));
			}
		}
	}

	output.Color = color;

    return output;
}

