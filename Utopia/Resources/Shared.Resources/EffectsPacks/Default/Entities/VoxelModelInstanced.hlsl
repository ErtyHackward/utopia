//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer VoxelModelPerFrame
{
	matrix ViewProjection;
	matrix LightViewProjection;
	float fogdist;
	float3 SunVector;
	float3 ShadowMapVars;
	bool UseShadowMap;
	matrix Focus;
};

cbuffer VoxelModel
{
	float4 colorMapping[64];
}

static const float SHADOW_EPSILON = 0.0002f;
static const float SMAP_SIZE = 4096.0f;
static const float SMAP_DX = 1.0f / SMAP_SIZE;

//	cube face						ba	F	Bo	T	L   R
static const float normalsX[6] = {  0,  0,  0,  0, -1,  1};
static const float normalsY[6] = {  0,  0, -1,  1,  0,  0};
static const float normalsZ[6] = { -1,  1,  0,  0,  0,  0};		

static const float faceshades[6] = { 0.6, 0.6, 0.8, 1.0, 0.7, 0.8 };

#define FACE_BACK 0
#define FACE_FRONT 1
#define FACE_BOTTOM 2
#define FACE_TOP 3
#define FACE_LEFT 4
#define FACE_RIGHT 5

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	uint4 Position		: POSITION;
	uint4 faceType    	: INFO;
	matrix Transform	: TRANSFORM;
	float4 LightColor	: COLOR; // Diffuse lighting color
};

struct PS_IN
{
	float4 Position				: SV_POSITION;
	float Light					: Light1;
	float3 LightColor			: Light2;
	float EmissiveLight         : Light0;
	int colorIndex              : VARIOUS0;
	float4 projTexC			    : TEXCOORD1;
	float Bias					: VARIOUS1;
	float SunLightLevel			: COLOR1;
};

struct PS_OUT
{
	float4 Color				: SV_TARGET0;
};

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2D ShadowMap;
SamplerState SamplerBackBuffer;

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
	output.Position = mul(worldPosition, ViewProjection);

	int facetype = input.faceType.x;

	// ambient occlusion value	
	output.Light = input.faceType.y;
	output.LightColor = input.LightColor.rgb;
	output.SunLightLevel = input.LightColor.a;

	// fake shadow
	output.EmissiveLight = faceshades[facetype];

	if (SunVector.y < 0 && UseShadowMap)
	{
		// Generate projective tex-coords to project shadow map onto scene.
		output.projTexC = mul(mul(worldPosition, Focus) , LightViewProjection);

		// compute variable bias for the shadow map
		float3 norm = float3(normalsX[facetype], normalsY[facetype], normalsZ[facetype]);
			
		// adjust bias according to the angle between face and Sun
		float cosTheta = abs(dot(norm, SunVector));
		float bias = tan(acos(cosTheta)) * ShadowMapVars.x;
		output.Bias = clamp(abs(bias), ShadowMapVars.y, ShadowMapVars.z);
	}

    return output;
}	

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

	// Is the pixel depth <= shadow map value?
	float result0 = depth <= s0 + shadowBias;

	return result0;
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

	if (UseShadowMap)
	{
		float shadowFactor = CalcShadowFactor(input.projTexC, input.Position.xy / input.Position.w, input.Bias);
		color.rbg *= 1 - (input.SunLightLevel * (1 - clamp(shadowFactor, 0.5, 1)));
	}

	output.Color = float4(color,1);

    return output;
}

