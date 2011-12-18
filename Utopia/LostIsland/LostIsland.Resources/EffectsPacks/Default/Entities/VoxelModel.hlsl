//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerFrame
{
	matrix World;
	matrix ViewProjection;
	float3 SunColor;			  // Diffuse lighting color
	float fogdist;
	float4 colorMapping[256];
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
static const float normalsZ[6] = {  1, -1,  0,  0,  0,  0};		

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
	float3 EmissiveLight		: Light0;
	float3 normal				: NORMAL0;
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

    float4 worldPosition = mul(newPosition, World);
	output.Position = mul(worldPosition, ViewProjection);

	int facetype = input.faceType.x;

	output.fogPower = 0; //clamp( ((length(worldPosition.xyz) - fogdist) / foglength), 0, 1);

	float3 normal = float3(normalsX[facetype],normalsY[facetype],normalsZ[facetype]);
	
	normal = mul(normal, World);
	//normal = mul(normal, ViewProjection);

	float3 lightDirection = float3(1,1,1);
	
	//emmissiveLight from terran.hlsl : bug, removes the color when a = 1 
	//float3 emmissiveLight = saturate(input.Col.rgb +  SunColor * input.Col.a);
	
	float3 emmissiveLight = float3(0,0,0); //input.Col.rgb;

	float DiffuseIntensity = 0.7;
	float3 DiffuseColor = float3( 1, 1, 1);
	//float3 DiffuseColor = colorMapping[output.colorIndex].rgb;

	float3 diffuse = dot( normal, lightDirection ) * DiffuseIntensity * DiffuseColor;

	output.EmissiveLight=saturate(emmissiveLight+diffuse);
    return output;
}	

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(PS_IN input) : SV_Target
{
	float4 color = float4(input.EmissiveLight, 1); //input.EmissiveLight * colorMapping[input.colorIndex]
	
	
	//maybe i just want to take color if coloroverlay is transparent(alpha=0) instead of lerping
	color = lerp(color, colorMapping[input.colorIndex], 0.25);

	//float4 Finalfogcolor = {SunColor / 1.5, color.a};
	
	// Apply fog on output color
	//color = lerp(color, Finalfogcolor, input.fogPower);
		
    return color;
}

