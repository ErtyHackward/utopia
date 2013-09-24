//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	matrix World;
	matrix LightViewProjection;
	float PopUpValue;
	float3 SunVector;
};

#include <SharedFrameCB.hlsl>

static const float SHADOW_EPSILON = 0.0002f;
static const float SMAP_SIZE = 4096.0f;
static const float SMAP_DX = 1.0f / SMAP_SIZE;

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

//	cube face						ba	F	Bo	T	L   R
static const float normalsX[6] = {  0,  0,  0,  0, -1,  1};
static const float normalsY[6] = {  0,  0, -1,  1,  0,  0};
static const float normalsZ[6] = { -1,  1,  0,  0,  0,  0};	


//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray TerraTexture;
Texture2D SkyBackBuffer;
Texture2DArray BiomesColors;
Texture2D ShadowMap;
SamplerState SamplerBackBuffer;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	uint4 Position		 : POSITION;
	float4 Col			 : COLOR;
	uint4 VertexInfo	 : INFO;   // (bool)x = is Upper vertex, y = facetype, z = AOPower factor 255 = Factor of 3, w = Offset
	float2 BiomeData     : BIOMEINFO; //X = Moisture, Y = Temperature
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
	float4 projTexC			    : TEXCOORD1;
	float Bias					: VARIOUS1;
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
	
	float4 newPosition = {input.Position.xyz, 1.0f};
	if(input.Various.y > 0)
	{
		newPosition += (faceSpecialOffset[input.VertexInfo.y] * input.Various.y);
	}

	float YOffset = 0;
	if(input.VertexInfo.x == 1) YOffset = (input.VertexInfo.w/255.0f);
	newPosition.y -= (YOffset + (PopUpValue * 128));

    float4 worldPosition = mul(newPosition, World);
	output.Position = mul(worldPosition, ViewProjection_focused);

	// Generate projective tex-coords to project shadow map onto scene.
	output.projTexC = mul(worldPosition, LightViewProjection);

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

	if (SunVector.y < 0)
	{
		// commented for debug reason
		//if (facetype == 0 || facetype == 1)
		//	facetype = 4;

		// compute variable bias for the shadow map
		float3 norm = float3(normalsX[facetype], normalsY[facetype], normalsZ[facetype]);
	
		float cosTheta = dot(norm, SunVector);
		float bias = tan(acos(cosTheta)) * 0.00024;
		output.Bias = clamp( abs(bias), 0.0002, 0.006);
	}
	
    return output;
}

// ============================================================================
// Shadow Map Creation ==> not used ATM moment, stability problems, and too much impact on the GPU ! (Need to render the scene twice !)
// ============================================================================
float CalcShadowFactor(float4 projTexC, float2 worldPos, float shadowBias)
{
	// if the sun is under the horisont => dark
	if (SunVector.y > 0)
	{
		return 0.0f;
	}

 	// Complete projection by doing division by w.
	projTexC.xyz /= projTexC.w;

	// Points outside the light volume are lit.
	if( projTexC.x < -1.0f || projTexC.x > 1.0f || projTexC.y < -1.0f || projTexC.y > 1.0f || projTexC.z < 0.0f) return 1.0f;
 	
 	// Transform from NDC space to texture space.
 	projTexC.x = +0.5f*projTexC.x + 0.5f;
 	projTexC.y = -0.5f*projTexC.y + 0.5f;
 	
 	// Depth in NDC space.
 	float depth = projTexC.z;
	
 	// Sample shadow map to get nearest depth to light.
 	float s0 = ShadowMap.Sample(SamplerBackBuffer, projTexC.xy).r;
	float s1 = ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + float2(SMAP_DX, 0) ).r;
	float s2 = ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + float2(0, SMAP_DX)).r;
	float s3 = ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + float2(SMAP_DX, SMAP_DX)).r;
	
	// Is the pixel depth <= shadow map value?
	float result0 = depth <= s0 + shadowBias;
	float result1 = depth <= s1 + shadowBias;
	float result2 = depth <= s2 + shadowBias;
	float result3 = depth <= s3 + shadowBias;
		
	// Transform to texel space
	float2 texelPos = SMAP_SIZE * projTexC.xy;
 
	// Determine the interpolation amounts
	float2 t = frac(texelPos);

 	// Interpolate results
	return lerp(lerp(result0, result1, t.x), lerp(result2, result3, t.x), t.y);
}


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
//[PS ENTRY POINT]
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

    float shadowFactor = CalcShadowFactor(input.projTexC, input.Position.xy / input.Position.w, input.Bias);
    finalColor.rbg *= clamp(shadowFactor, 0.5, 1);


	// Apply fog on output color
	output.Color = finalColor;
    return output;
}

