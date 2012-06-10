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
	float2 BackBufferSize;
	float2 Various;               //.x = 1 if head under water
};

static const float foglength = 20;
static float3 Dayfogcolor = {0.7, 0.7, 0.7 };
static float3 Nightfogcolor = {0, 0, 0 };

#define E 2.71828

static const float texmul1[6] = { -1,  1, -1,  1,  0,  0};
static const float texmul2[6] = {  0,  0,  0,  0, -1,  1};
static const float texmul3[6] = { -1, -1,  0,  0, -1, -1};
static const float texmul4[6] = {  0,  0,  1,  1,  0,  0};

//face Types
//Back = 0,
//Front = 1,
//Bottom = 2,
//Top = 3,
//Left = 4,
//Right = 5
static const float3 facenormals[6] = {
												{0,0,1},
												{0,0,1},
												{0,1,0},
												{0,1,0},
												{1,0,0},
												{1,0,0}
												};

static const float SHADOW_EPSILON = 0.001f;

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray TerraTexture;
Texture2DArray BiomesColors;
Texture2D SolidBackBuffer;
Texture2D SkyBackBuffer;

SamplerState SamplerDiffuse;
SamplerState SamplerBackBuffer;
//--------------------------------------------------------------------------------------
//Vertex shader Input

struct VS_LIQUID_IN
{
	uint4 Position		 : POSITION;
	float4 Col			 : COLOR;
	uint4 VertexInfo1	 : INFO0; // x = FaceType, (bool)y = is Upper vertex
	float4 VertexInfo2	 : INFO1; // x = Y Modified block Height modificator, Y = Temperature, Z = Moisture
};

struct PS_IN
{
	float4 Position				: SV_POSITION;
	float3 UVW					: TEXCOORD0;
	float fogPower				: VARIOUS0;
	float4 EmissiveLight		: Light0;
	float2 BiomeData			: BIOMEDATA0;
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

PS_IN VS_LIQUID(VS_LIQUID_IN input)
{
    PS_IN output;
	
	float4 newPosition = {input.Position.xyz, 1.0f};
	float YOffset = 0;
	if(input.VertexInfo1.y == 1) YOffset = input.VertexInfo2.x;
	newPosition.y -= YOffset;

    float4 worldPosition = mul(newPosition, World);
	output.Position = mul(worldPosition, ViewProjection);

	int facetype = input.VertexInfo1.x;
	//Compute the texture mapping
	output.UVW = float3(
						(input.Position.x * texmul1[facetype]) + (input.Position.z * texmul2[facetype]), 
						((input.Position.y * texmul3[facetype]) + YOffset) + (input.Position.z * texmul4[facetype]),
						input.Position.w );

	output.EmissiveLight.rgb = saturate(input.Col.rgb +  SunColor * input.Col.a);

	float3 facenorm = facenormals[facetype];
	output.EmissiveLight.a = (1 - abs(dot(normalize(worldPosition.xyz), facenorm))) * 1.3 ;
	output.EmissiveLight.a = clamp(output.EmissiveLight.a, 0.6, 1);

	output.fogPower = clamp( ((length(worldPosition.xyz) - fogdist) / foglength), 0, 1);
	output.BiomeData = input.VertexInfo2.yz;
    return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
PS_OUT PS(PS_IN input)
{
	PS_OUT output;

	float fogvalue = min( Opaque, 1 - input.fogPower);
	clip(fogvalue <= 0.001 ? -1:1); 

	float4 colorInput = float4(TerraTexture.Sample(SamplerDiffuse, input.UVW).rgb, 1) * input.EmissiveLight;
	
	float3 biomeColorSampling = {input.BiomeData.x, input.BiomeData.y, 2};
	float4 biomeColor =  BiomesColors.Sample(SamplerBackBuffer, biomeColorSampling);
	colorInput.r = colorInput.r * biomeColor.r;
	colorInput.g = colorInput.g * biomeColor.g;
	colorInput.b = colorInput.b * biomeColor.b;
	
	//Sample against Solid landscape
	float2 backBufferSampling = {input.Position.x / BackBufferSize.x , input.Position.y / BackBufferSize.y};
	float4 backBufferColor = SolidBackBuffer.Sample(SamplerBackBuffer, backBufferSampling);

	//Manual Blending with SolidBackBuffer color received
	float4 color = {(colorInput.rgb * colorInput.a) + (backBufferColor.rgb * (1 - colorInput.a)), colorInput.a};

	float4 finalColor = color;
	
	//To execute only when Fog is present !
	if(fogvalue < 1){
		//Sample BackGround Sky
		backBufferColor = SkyBackBuffer.Sample(SamplerBackBuffer, backBufferSampling);

		color.a = min( Opaque, 1 - input.fogPower);
		finalColor.rgb = (color.rgb * color.a) + (backBufferColor.rgb * (1 - color.a));
		finalColor.a = color.a;
	}
	output.Color = finalColor;

    return output;
}

