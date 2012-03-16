//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerFrame
{
	matrix World;
	matrix ViewProjection;
	float3 SunColor;			  // Diffuse lighting color
	float fogdist;
	float4 colorMapping[64];
	float3 LightDirection;
};

cbuffer PerPart
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
	float3 normal				: NORMAL0;
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

	float3 normal = float3(normalsX[facetype],normalsY[facetype],normalsZ[facetype]);
	
	Matrix wvp = World;// * ViewProjection;

	// transform normal
	normal.x = normal.x * wvp._11 + normal.y * wvp._21 + normal.z * wvp._31;
    normal.y = normal.x * wvp._12 + normal.y * wvp._22 + normal.z * wvp._32;
    normal.z = normal.x * wvp._13 + normal.y * wvp._23 + normal.z * wvp._33;
	
	
	output.Light = input.faceType.y;

	//float3 lightDirection = float3(0,0,-5);
	float3 ambient = float3(0.4, 0.4, 0.4);	

	float diffuse = dot( normal, LightDirection );

	output.EmissiveLight=saturate(diffuse);
    return output;
}	

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
PS_OUT PS(PS_IN input)
{
	PS_OUT output;

	float intensity = input.Light / 255;
	
	output.Color = float4(lerp(colorMapping[input.colorIndex].rgb * intensity,input.EmissiveLight, 0.4 ),1);
    return output;
}

