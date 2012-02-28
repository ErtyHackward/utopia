//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	matrix World;
	float popUpYOffset;
	float Opaque;
};

cbuffer PerFrame
{
	matrix ViewProjection;
	float3 SunColor;			  // Diffuse lighting color
	float fogdist;
};


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
	uint4 VertexInfo	 : INFO;   // (bool)x = is Upper vertex, y = facetype, z = AOPower factor 255 = Factor of 3, w = Offset
};

struct PS_IN
{
	float4 Position				: SV_POSITION;
	float3 UVW					: TEXCOORD0;
	float fogPower				: VARIOUS0;
	float3 EmissiveLight		: Light0;
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
	
	float4 newPosition = {input.Position.xyz, 1.0f};
	float YOffset = 0;
	if(input.VertexInfo.x == 1) YOffset = (input.VertexInfo.w/255.0f);
	newPosition.y -= YOffset;

    float4 worldPosition = mul(newPosition, World);
	output.Position = mul(worldPosition, ViewProjection);

	int facetype = input.VertexInfo.y;
	//Compute the texture mapping
	output.UVW = float3(
						(input.Position.x * texmul1[facetype]) + (input.Position.z * texmul2[facetype]), 
						((input.Position.y * texmul3[facetype]) + YOffset) + (input.Position.z * texmul4[facetype]),
						input.Position.w );

	//VertexInfo.z/85 => Will transform the Z into a range from 0 to 3
	output.EmissiveLight = input.VertexInfo.z/85 * saturate(input.Col.rgb +  SunColor * input.Col.a);
	output.EmissiveLight *= faceshades[facetype];

	output.fogPower = clamp( ((length(worldPosition.xyz) - fogdist) / foglength), 0, 1);

    return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(PS_IN input) : SV_Target
{
	float4 color = TerraTexture.Sample(SamplerDiffuse, input.UVW);
	
	clip( color.a < 0.1f ? -1:1 ); //Remove the pixel if alpha < 0.1

	color = color * float4(input.EmissiveLight, 1);

	float4 Finalfogcolor = {SunColor / 1.5, color.a};
	color = lerp(color, Finalfogcolor, input.fogPower);

	//color.a *= Opaque;

	// Apply fog on output color
    return color;
}

