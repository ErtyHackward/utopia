//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	matrix World;
	matrix LightViewProjection;
	float PopUpValue;
	float3 SunVector;
	float3 ShadowMapVars;
	bool UseShadowMap;
};

cbuffer PerDrawShadow
{
	matrix LightWVP;
};

#include <SharedFrameCB.hlsl>

static const float SHADOW_EPSILON = 0.0002f;
static const float SMAP_SIZE = 4096.0f;
static const float SMAP_DX = 1.0f / SMAP_SIZE;

static const float foglength = 20;
static float3 Dayfogcolor = {0.7, 0.7, 0.7 };
static float3 Nightfogcolor = {0, 0, 0 };

//face Types
#define FACE_BACK 0
#define FACE_FRONT 1
#define FACE_BOTTOM 2
#define FACE_TOP 3
#define FACE_LEFT 4
#define FACE_RIGHT 5

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
	uint4 Position		 : POSITION;  // X = XPosi, Y = YPosi, Z = ZPosi, W = not used
	float4 Col			 : COLOR;     // Light color, A = sun light
	uint4 VertexInfo	 : INFO;	  // (bool)x = is Upper vertex, y = facetype, z = AOPower factor 255 = Factor of 3, w = Offset
	float2 BiomeData     : BIOMEINFO; // X = Moisture, Y = Temperature
	uint2 Various		 : VARIOUS;   // X = ArrayTextureID for Biome, Y SideOffset multiplier
	uint4 Animation      : ANIMATION; // X = Speed, Y = NbrFrames
	uint ArrayId         : ARRAYID;
	uint Dummy           : DUMMY;
};

struct PS_IN
{
	float4 Position				: SV_POSITION;
	float3 UVW					: TEXCOORD0;
	float fogPower				: VARIOUS0;
	float4 EmissiveLight		: Light0;
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

// ============================================================================
// Shadow Map Creation
// ============================================================================
float CalcShadowFactor(float4 projTexC, float2 worldPos, float shadowBias)
{
	// if the sun is under the horisont => dark
	if (SunVector.y > 0 || shadowBias == 0)
	{
		return 0.0f;
	}

	// Complete projection by doing division by w.
	projTexC.xyz /= projTexC.w;

	// Points outside the light volume are lit.
	if (projTexC.x < -1.0f || projTexC.x > 1.0f || projTexC.y < -1.0f || projTexC.y > 1.0f || projTexC.z < 0.0f) return 1.0f;

	// Transform from NDC space to texture space.
	projTexC.x = +0.5f*projTexC.x + 0.5f;
	projTexC.y = -0.5f*projTexC.y + 0.5f;

	// Depth in NDC space.
	float depth = projTexC.z;

	// Sample shadow map to get nearest depth to light.
	float s0 = ShadowMap.Sample(SamplerBackBuffer, projTexC.xy).r;
	//float s1 = ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + float2(SMAP_DX, 0) ).r;
	//float s2 = ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + float2(0, SMAP_DX)).r;
	//float s3 = ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + float2(SMAP_DX, SMAP_DX)).r;

	// Is the pixel depth <= shadow map value?
	float result0 = depth <= s0 + shadowBias;
	//float result1 = depth <= s1 + shadowBias;
	//float result2 = depth <= s2 + shadowBias;
	//float result3 = depth <= s3 + shadowBias;

	// Transform to texel space
	//float2 texelPos = SMAP_SIZE * projTexC.xy;

	// Determine the interpolation amounts
	//float2 t = frac(texelPos);

	// Uncomment to interpolate results
	return result0; // lerp(lerp(result0, result1, t.x), lerp(result2, result3, t.x), t.y);
}

//--------------------------------------------------------------------------------------
// Vertex Shaders
//--------------------------------------------------------------------------------------
//[VS ENTRY POINT]
PS_IN VS(VS_IN input)
{
    PS_IN output;
	
	float4 newPosition = {input.Position.xyz, 1.0f};

	//For block that are shorter than full block size on the X/Z axis
	if(input.Various.y > 0)
	{
		newPosition += (faceSpecialOffset[input.VertexInfo.y] * input.Various.y);
	}

	float YOffset = 0;
	if (input.VertexInfo.x == 1) YOffset = (input.VertexInfo.w/255.0f);
	newPosition.y -= (YOffset + (PopUpValue * 128));

    float4 worldPosition = mul(newPosition, World);
	output.Position = mul(worldPosition, ViewProjection_focused);
	
	int facetype = input.VertexInfo.y;
	//Compute the texture mapping
	output.UVW = float3(
						(input.Position.x * texmul1[facetype]) + (input.Position.z * texmul2[facetype]), 
						((input.Position.y * texmul3[facetype]) + YOffset) + (input.Position.z * texmul4[facetype]),
						input.ArrayId);

	//Animate texture !
	if (input.Animation.y > 0)
	{
		int animationFrame = ((TextureFrameAnimation * input.Animation.x) % input.Animation.y);
		output.UVW.z += animationFrame;
	}

	float3 light = saturate(input.Col.rgb + SunColor * input.Col.a) * faceshades[facetype];

	output.EmissiveLight = float4(light, input.Col.a);


	output.fogPower = 1 - (clamp( ((length(worldPosition.xyz) - fogdist) / foglength), 0, 1));
	output.BiomeData = (input.BiomeData * 0.6f) + 0.2f;
	output.BiomeData.x = saturate(output.BiomeData.x + WeatherGlobalOffset.x); //Temp
	output.BiomeData.y = saturate(output.BiomeData.y + WeatherGlobalOffset.y); //Moisture
	output.Various = input.Various;

	if (SunVector.y < 0 && UseShadowMap)
	{
		// Generate projective tex-coords to project shadow map onto scene.
		output.projTexC = mul(worldPosition, LightViewProjection);

		// compute variable bias for the shadow map
		float3 norm = float3(normalsX[facetype], normalsY[facetype], normalsZ[facetype]);
	
		// bottom face is always dark
		if (facetype == FACE_BOTTOM)
		{
			output.Bias = 0; // 0 is a special case
			return output;
		}

	    // left and right faces will be always dark in case when the light vector is facing other side
		if ((facetype == FACE_LEFT || facetype == FACE_RIGHT) && dot(norm, SunVector) >= 0)
		{
			output.Bias = 0;
			return output;
		}
		
		// back and front faces will always be perpendicular to sunVector so we will keep the bias close to zero
		if (facetype == FACE_BACK || facetype == FACE_FRONT)
		{
			output.Bias = 0.00001;
			return output;
		}

		// on early morning or late evening draw top face always dark to avoid huge moire effect
		if (facetype == FACE_TOP && (SunVector.x > 0.99 || SunVector.x < -0.99))
		{
			output.Bias = 0;
			return output;
		}

		// adjust bias according to the angle between face and Sun
		float cosTheta = abs(dot(norm, SunVector));
		float bias = tan(acos(cosTheta)) * ShadowMapVars.x;
		output.Bias = clamp(abs(bias), ShadowMapVars.y, ShadowMapVars.z);				
	}
	
    return output;
}

//[VS ENTRY POINT]
PS_IN VSShadow(VS_IN input)
{
	PS_IN output;

	float4 newPosition = { input.Position.xyz, 1.0f };

	if (input.Various.y > 0)
	{
		newPosition += (faceSpecialOffset[input.VertexInfo.y] * input.Various.y);
	}

	float YOffset = 0;
	if (input.VertexInfo.x == 1) YOffset = (input.VertexInfo.w / 255.0f);
	newPosition.y -= YOffset;

	output.Position = mul(newPosition, LightWVP);

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
	//float4 color = TerraTexture.SampleLevel(SamplerDiffuse, input.UVW, 0);
	float4 color = TerraTexture.Sample(SamplerDiffuse, input.UVW);
	
	clip(color.a < 0.1f ? -1:1 );    //Remove the pixel if alpha < 0.1

	//Apply Biome Color if the Alpha is < 0.5
	if(color.a < 0.5)
	{
		float3 samplingBiomeColor = {input.BiomeData.xy, input.Various.x };
	    float4 biomeColor =  BiomesColors.Sample(SamplerBackBuffer, samplingBiomeColor);
		color.r = color.r * biomeColor.r;
		color.g = color.g * biomeColor.g;
		color.b = color.b * biomeColor.b;
	}

	color.a = 1.0f;
	color = color * float4(input.EmissiveLight.rgb, 1);

	float4 finalColor = color;

	if (UseShadowMap)
	{
		float shadowFactor = CalcShadowFactor(input.projTexC, input.Position.xy / input.Position.w, input.Bias);
		finalColor.rbg *= 1 - (input.EmissiveLight.a * (1 - clamp(shadowFactor, 0.5, 1)));
	}
		
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

