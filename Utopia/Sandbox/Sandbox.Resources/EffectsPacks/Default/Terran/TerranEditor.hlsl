//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	matrix World;
	float popUpYOffset;
};

#include <SharedFrameCB.hlsl>

static const float foglength = 45;
static float3 Dayfogcolor = {0.7, 0.7, 0.7 };
static float3 Nightfogcolor = {0, 0, 0 };

static const float texmul1[6] = { -1,  1, -1,  1,  0,  0};
static const float texmul2[6] = {  0,  0,  0,  0, -1,  1};
static const float texmul3[6] = { -1, -1,  0,  0, -1, -1};		
static const float texmul4[6] = {  0,  0,  1,  1,  0,  0};


//	cube face						ba	F	Bo	T	L   R
static const float normalsX[6] = {  0,  0,  0,  0, -1,  1};
static const float normalsY[6] = {  0,  0, -1,  1,  0,  0};
static const float normalsZ[6] = {  1, -1,  0,  0,  0,  0};		


//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray TerraTexture;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	uint4 Position		 : POSITION;
	float4 Col			 : COLOR;
	uint4 VertexInfo	 : INFO;
};

struct PS_IN
{
	float4 Position				: SV_POSITION;
	float3 UVW					: TEXCOORD0;
	float fogPower				: VARIOUS0;
	float3 EmissiveLight		: Light0;
	float3 UVWOverlay			: TEXCOORD1;
	float3 normal				: NORMAL0;
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
	newPosition.y += input.VertexInfo.x + popUpYOffset; //Offseting the Y

    float4 worldPosition = mul(newPosition, World);
	output.Position = mul(worldPosition, ViewProjection);

	int facetype = input.VertexInfo.y;
	//Compute the texture mapping
	output.UVW = float3(
						(input.Position.x * texmul1[facetype]) + (input.Position.z * texmul2[facetype]), 
						(input.Position.y * texmul3[facetype]) + (input.Position.z * texmul4[facetype]),
						input.Position.w );

	output.UVWOverlay = float3(
						(input.Position.x * texmul1[facetype]) + (input.Position.z * texmul2[facetype]), 
						(input.Position.y * texmul3[facetype]) + (input.Position.z * texmul4[facetype]),
						input.VertexInfo.z );
						

	

	output.fogPower = 0; //clamp( ((length(worldPosition.xyz) - fogdist) / foglength), 0, 1);

	float3 normal = float3(normalsX[facetype],normalsY[facetype],normalsZ[facetype]);
	output.normal = normal;
	float3 lightDirection = float3(0,1,1);
	
	//emmissiveLight from terran.hlsl : bug, removes the color when a = 1 
	//float3 emmissiveLight = saturate(input.Col.rgb +  SunColor * input.Col.a);
	
	float3 emmissiveLight =input.Col.rgb;

	float DiffuseIntensity =0.4;
	float3 DiffuseColor = float3( 1, 1, 1);
	//float3 DiffuseColor = input.Col.rgb;

	float3 diffuse = dot( normal, lightDirection ) * DiffuseIntensity * DiffuseColor;

	output.EmissiveLight=saturate(emmissiveLight+diffuse);
    return output;
}	

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
//[PS ENTRY POINT]
float4 PS(PS_IN input) : SV_Target
{
	float4 color = TerraTexture.Sample(SamplerDiffuse, input.UVW) * float4(input.EmissiveLight, 1);
	float4 colorOverlay = TerraTexture.Sample(SamplerDiffuse, input.UVWOverlay) * float4(input.EmissiveLight, 1);
	
	//maybe i just want to take color if coloroverlay is transparent(alpha=0) instead of lerping
	color = lerp(color,colorOverlay,0.25);

	//float4 Finalfogcolor = {SunColor / 1.5, color.a};
	
	// Apply fog on output color
	//color = lerp(color, Finalfogcolor, input.fogPower);
		
    return color;
}

