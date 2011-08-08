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
	float3 SunColor;			 
	float dayTime;
	float WaveGlobalOffset;
	float LiquidOffset;
	float2 BackBufferSize;
	float fogdist;
};


static const float foglength = 30;
static float3 Dayfogcolor = {0.7, 0.7, 0.7 };
static float3 Nightfogcolor = {0, 0, 0 };

static const float texmul1[6] = { -1,  1, -1,  1,  0,  0};
static const float texmul2[6] = {  0,  0,  0,  0, -1,  1};
static const float texmul3[6] = { -1, -1,  0,  0, -1, -1};
static const float texmul4[6] = {  0,  0,  1,  1,  0,  0};

//3D Flow
static const float LiquidTypeWaveEnabler[4] = { 
												0,  //None
												1,  //WaterSource
												0,  //Water
												0   //Lava
												};


static const float LiquidTypeOffsetStaticEnabler[4] = { 
														0,	//None
														0,  //WaterSource
														0,  //Water
														0	//Lava
														};

//Texture Flow
static const float2 texflow[11] = {	
									{ 0.0,   0.0},			// Still
									{ 1.0,	 0.0},			// Right
									{-1.0,	 0.0},			// Left
									{ 0.0,   1.0},			// Front
									{ 0.0,  -1.0},			// Back
									{ 1.0,   1.0},			// FrontRight
									{-1.0,   1.0},			// FrontLeft
									{ 1.0,  -1.0},			// BackRight
									{-1.0,  -1.0},			// BackLeft
									{ 0.0,   1.0},			// falling
									{ 1.0,  -1.0},			// Raising
								  };

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

Texture2D SolidBackBuffer;
SamplerState SamplerBackBuffer
{
	Filter = MIN_MAG_MIP_POINT;
	AddressU = CLAMP ; 
	AddressV = CLAMP ;
};

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	uint4 Position		 : POSITION;
	float4 Col			 : COLOR;
	uint4 VertexInfo1	 : INFO0; // x = FaceType, y = LiquidType, z = FlowDirection
	float4 VertexInfo2	 : INFO1; // x = YOffset, 
};

//Pixel shader Input
struct PS_IN
{
	float4 Position				: SV_POSITION;
	float3 UVW					: TEXCOORD;
	float fogPower				: VARIOUS0;
	float3 EmissiveLight		: Light0;
};

//--------------------------------------------------------------------------------------
// Vertex Shaders
//--------------------------------------------------------------------------------------

PS_IN VS(VS_IN input)
{
    PS_IN output;
	
	float4 newPosition = {input.Position.xyz, 1.0f};
	
	float YModifier = (clamp((sin(input.VertexInfo2.x + WaveGlobalOffset) + 1) / 2,  0.05 , 0.95) * input.VertexInfo2.y) * LiquidTypeWaveEnabler[input.VertexInfo1.y]; //Offseting the Y
	YModifier += ((1 - input.VertexInfo2.x) * input.VertexInfo2.y) * LiquidTypeOffsetStaticEnabler[input.VertexInfo1.y]; //Offseting the Y

	newPosition.y -= YModifier - popUpYOffset;

    float4 worldPosition = mul(newPosition, World);
	output.Position = mul(worldPosition, ViewProjection);

	uint facetype = input.VertexInfo1.x;
	//Compute the texture mapping
	output.UVW = float3(
						(input.Position.x * texmul1[facetype]) + (input.Position.z * texmul2[facetype]), 
						(input.Position.y * texmul3[facetype]) + (input.Position.z * texmul4[facetype]) + YModifier,
						input.Position.w );

	//Offset Texture to simulate 2D flow
	output.UVW.xy += texflow[input.VertexInfo1.z] * LiquidOffset;

	output.fogPower = 0; //clamp( (length(worldPosition.xyz) - fogdist) / foglength, 0, 1);

	output.EmissiveLight = saturate(input.Col.rgb +  SunColor * input.Col.a);

    return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(PS_IN input) : SV_Target
{
	float4 colorInput = TerraTexture.Sample(SamplerDiffuse, input.UVW) * float4(input.EmissiveLight, 1);
	float2 backBufferSampling = {input.Position.x / BackBufferSize.x , input.Position.y / BackBufferSize.y};
	float4 backBufferColor = SolidBackBuffer.Sample(SamplerBackBuffer, backBufferSampling);
	
	//Manual Blending of Water with the passed in BackBuffer!
	float4 color = {(colorInput.rgb * colorInput.a) + (backBufferColor.rgb * (1 - colorInput.a)), colorInput.a};

	float4 Finalfogcolor = {SunColor / 1.5, color.a};
	color = lerp(color, Finalfogcolor, input.fogPower);
    return color;
}
