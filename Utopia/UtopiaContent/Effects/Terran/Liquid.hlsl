//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	matrix World;
	float popUpYOffset;
};

cbuffer PerFrame
{
	matrix ViewProjection;
	float3 SunColor;			  // Diffuse lighting color
	float fogdist;
};

static const float foglength = 45;
static float3 Dayfogcolor = {0.7, 0.7, 0.7 };
static float3 Nightfogcolor = {0, 0, 0 };

#define E 2.71828

static const float texmul1[6] = { -1,  1, -1,  1,  0,  0};
static const float texmul2[6] = {  0,  0,  0,  0, -1,  1};
static const float texmul3[6] = { -1, -1,  0,  0, -1, -1};
static const float texmul4[6] = {  0,  0,  1,  1,  0,  0};

static const float SHADOW_EPSILON = 0.001f;

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray TerraTexture;
SamplerState SamplerDiffuse
{
	Filter = MIN_LINEAR_MAG_POINT_MIP_LINEAR;
	AddressU = Wrap ; 
	AddressV = Wrap ;
};

//--------------------------------------------------------------------------------------
//Vertex shader Input

struct VS_LIQUID_IN
{
	uint4 Position		 : POSITION;
	float4 Col			 : COLOR;
	uint4 VertexInfo1	 : INFO0; // x = FaceType, y = LiquidType, z = FlowDirection
	float4 VertexInfo2	 : INFO1; // x = YOffset, 
};

struct PS_IN
{
	float4 Position				: SV_POSITION;
	float3 UVW					: TEXCOORD0;
	float3 EmissiveLight		: Light0;
	float fogPower				: VARIOUS0;
};

//--------------------------------------------------------------------------------------
// Fonctions
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
// Vertex Shaders
//--------------------------------------------------------------------------------------

PS_IN VS_LIQUID(VS_LIQUID_IN input)
{
    PS_IN output;
	
	float4 newPosition = {input.Position.xyz, 1.0f};
	newPosition.y += input.VertexInfo2.x + popUpYOffset; //Offseting the Y

    float4 worldPosition = mul(newPosition, World);
	output.Position = mul(worldPosition, ViewProjection);

	int facetype = input.VertexInfo1.x;
	//Compute the texture mapping
	output.UVW = float3(
						(input.Position.x * texmul1[facetype]) + (input.Position.z * texmul2[facetype]), 
						(input.Position.y * texmul3[facetype]) + (input.Position.z * texmul4[facetype]),
						input.Position.w );

	output.EmissiveLight = saturate(input.Col.rgb +  SunColor * input.Col.a);
	output.fogPower = clamp( ((length(worldPosition.xyz) - fogdist) / foglength), 0, 1);

    return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(PS_IN input) : SV_Target
{
	float4 color = TerraTexture.Sample(SamplerDiffuse, input.UVW) * float4(input.EmissiveLight, 1);

	float4 Finalfogcolor = {SunColor / 1.5, color.a};
	color = lerp(color, Finalfogcolor, input.fogPower);

    return color;
}

