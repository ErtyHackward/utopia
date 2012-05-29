//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	matrix World;
	matrix ViewProj;
	float3 CameraWorldPosition;
	float time;
	float3 LightDirection;
	float headUnderWater;
}

static const float3 LightColor = {1.0f, 1.0f, 1.0f};
static const float3 LightColorAmbient = {0.0f, 0.0f, 0.0f};
static const float SunLightness = 0.5; 
static const float sunRadiusAttenuation = 128;
static const float largeSunLightness = 0.5;
static const float largeSunRadiusAttenuation = 3;

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2D DiffuseTexture;
SamplerState SkySampler;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct appdata {
	float3 Position				: POSITION;
};

//Pixel shader Input
struct vertexOutput {
	float4 HPosition	 		: SV_POSITION;
	float2 texcoord				: TEXCOORD0;
	float3 WorldLightVec		: TEXCOORD1;
	float3 WorldEyeDirection	: TEXCOORD2;
};

struct PS_OUT
{
	float4 Color				: SV_TARGET0;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
vertexOutput mainVS (appdata IN)
{
	vertexOutput OUT;

	//Center the sky on the Camera
	OUT.HPosition = mul(float4(IN.Position.xyz, 1), World);
	OUT.WorldEyeDirection = float3(-OUT.HPosition.xyz); //CameraWorldPosition.xyz - OUT.HPosition;
	OUT.HPosition = mul(OUT.HPosition, ViewProj);
	OUT.WorldLightVec = -LightDirection; 
	OUT.texcoord.y = 1-((IN.Position.y + 347.0f) / 2000.0f);
	OUT.texcoord.x = time;

	return OUT;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
PS_OUT mainPS(vertexOutput IN)
{	
	PS_OUT output;
	float4 colorOutput = float4(0,0,0,1);		
	float3 lightVec = normalize(IN.WorldLightVec);
	float3 eyeVec = normalize(IN.WorldEyeDirection);

	// Calculate sun highlight...	
	// Peut être interpreté comme : SI je regarde en face du soleil ==> Maximum sunHighLigh !! (dot = 1)
	float sunHighlight = pow(max(0.000001f, dot(lightVec, -eyeVec)), sunRadiusAttenuation) * SunLightness;	
	// Calculate a wider sun highlight 
	float largeSunHighlight = pow(max(0.000001f, dot(lightVec, -eyeVec)), largeSunRadiusAttenuation) * largeSunLightness;

	if(headUnderWater.x == 1)
	{
		colorOutput = float4(0,0,0.2,1);
	}else{
		colorOutput = DiffuseTexture.Sample(SkySampler, IN.texcoord);
	}

	//colorOutput = DiffuseTexture.Sample(SkySampler, IN.texcoord);

	colorOutput += sunHighlight + largeSunHighlight;

	colorOutput.a *= 1 - (min(max(CameraWorldPosition.y - 127, 0), 173) / 173);

	output.Color = colorOutput;
    return output;
}
