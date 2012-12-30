//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer VoxelModelPerFrame
{
	matrix ViewProjection;
	float3 LightDirection;		//diffuse light direction
	float fogdist;
};

cbuffer VoxelModel
{
	float4 colorMapping[64];
	matrix World;
	float3 LightColor;			// Diffuse lighting color
	float Alpha;				// model transparency
}

cbuffer VoxelModelPerPart
{
	matrix Transform;
}

static const float foglength = 45;
static float3 Dayfogcolor = {0.7, 0.7, 0.7 };
static float3 Nightfogcolor = {0, 0, 0 };

//	cube face						ba	F	Bo	T	L   R
static const float normalsX[6] = {  0,  0,  0,  0, -1,  1};
static const float normalsY[6] = {  0,  0, -1,  1,  0,  0};
static const float normalsZ[6] = { -1,  1,  0,  0,  0,  0};		

static const float faceshades[6] = { 0.6, 0.6, 0.8, 1.0, 0.7, 0.8 };

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	uint4 Position		 : POSITION;
	uint4 faceType    	 : INFO;
};

struct PS_IN
{
	float4 Position				: SV_POSITION;
	float fogPower				: VARIOUS0;
	int colorIndex              : VARIOUS1;
	float EmissiveLight         : Light0;
	float Light					: Light1;
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
	
	output.colorIndex = input.Position.w - 1;

	float4 newPosition = {input.Position.xyz, 1.0f};

	newPosition = mul(newPosition, Transform);

    float4 worldPosition = mul(newPosition, World);
	output.Position = mul(worldPosition, ViewProjection);

	int facetype = input.faceType.x;

	output.fogPower = 0; //clamp( ((length(worldPosition.xyz) - fogdist) / foglength), 0, 1);

	// ambient occlusion value	
	output.Light = input.faceType.y;
	

	// fake shadow
	output.EmissiveLight = faceshades[facetype];

	// diffuse shadow
	//float3 normal = float3(normalsX[facetype], normalsY[facetype], normalsZ[facetype]);
	//normal = normalize(mul(normal, World));
	//float diffuse = dot(normal, LightDirection);
	//float lowerBound = 0.7;
	//output.EmissiveLight = diffuse * (1.0f-lowerBound) + lowerBound;

    return output;
}	

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
PS_OUT PS(PS_IN input)
{
	PS_OUT output;

	float intensity = input.Light / 255;

	float3 color = colorMapping[input.colorIndex].rgb * input.EmissiveLight * LightColor * intensity;
	
	output.Color = float4(color, Alpha);

    return output;
}

