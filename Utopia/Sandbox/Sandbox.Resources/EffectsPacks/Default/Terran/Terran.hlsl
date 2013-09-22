//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	matrix World;
	matrix LightViewProjection;
	float popUpYOffset;
	float Opaque;
	float3 LightDirection;
};

cbuffer PerFrame
{
	matrix ViewProjection;
	float3 SunColor;			  // Diffuse lighting color
	float fogdist;
	float2 BackBufferSize;
	float2 Various;               //.x = 1 if head under water
};

static const float SHADOW_EPSILON = 0.0002f;
static const float SMAP_SIZE = 4096.0f;
static const float SMAP_DX = 1.0f / SMAP_SIZE;

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
static const float2 poissonDisk[16] = { {-0.613392, 0.617481}, {0.170019, -0.040254}, {-0.299417, 0.791925}, {0.645680, 0.493210}, {-0.651784, 0.717887}, {0.421003, 0.027070}, {-0.817194, -0.271096}, {-0.705374, -0.668203}, {0.977050, -0.108615}, {0.063326, 0.142369}, {0.203528, 0.214331}, {-0.667531, 0.326090}, {-0.098422, -0.295755}, {-0.885922, 0.215369}, {0.566637, 0.605213}, {0.039766, -0.396100} };

static const float4 faceSpecialOffset[6] = { {0.0f,0.0f,0.0625f,0.0f} , {0.0f,0.0f,-0.0625f,0.0f}, {0.0f,0.0f,0.0f,0.0f}, {0.0f,0.0f,0.0f,0.0f}, {0.0625f,0.0f,0.0f,0.0f}, {-0.0625f,0.0f,0.0f,0.0f} };

//	cube face						ba	F	Bo	T	L   R
static const float normalsX[6] = {  0,  0,  0,  0, -1,  1};
static const float normalsY[6] = {  0,  0, -1,  1,  0,  0};
static const float normalsZ[6] = { -1,  1,  0,  0,  0,  0};	

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray TerraTexture;
Texture2D SkyBackBuffer;
Texture2DArray BiomesColors;
Texture2D ShadowMap;
SamplerState SamplerBackBuffer;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	uint4 Position		 : POSITION;
	float4 Col			 : COLOR;
	uint4 VertexInfo	 : INFO;   // (bool)x = is Upper vertex, y = facetype, z = AOPower factor 255 = Factor of 3, w = Offset
	float2 BiomeData     : BIOMEINFO; //X = Temperature, Y = Moisture
	uint2 Various		 : VARIOUS;   //X = ArrayTextureID for Biome, Y SideOffset multiplier
};

struct PS_IN
{
	float4 Position				: SV_POSITION;
	float3 UVW					: TEXCOORD0;
	float fogPower				: VARIOUS0;
	float3 EmissiveLight		: Light0;
	float2 BiomeData			: BIOMEDATA0;
	uint2 Various				: BIOMEDATAVARIOUS0;  
	float4 projTexC			    : TEXCOORD1;
	float Bias					: VARIOUS1;
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
	
	float4 newPosition = {input.Position.xyz, 1.0f};
	if(input.Various.y > 0)
	{
		newPosition += (faceSpecialOffset[input.VertexInfo.y] * input.Various.y);
	}

	float YOffset = 0;
	if(input.VertexInfo.x == 1) YOffset = (input.VertexInfo.w/255.0f);
	newPosition.y -= YOffset;

    float4 worldPosition = mul(newPosition, World);
	output.Position = mul(worldPosition, ViewProjection);

	// Generate projective tex-coords to project shadow map onto scene.
	output.projTexC = mul(worldPosition, LightViewProjection);

	int facetype = input.VertexInfo.y;
	//Compute the texture mapping
	output.UVW = float3(
						(input.Position.x * texmul1[facetype]) + (input.Position.z * texmul2[facetype]), 
						((input.Position.y * texmul3[facetype]) + YOffset) + (input.Position.z * texmul4[facetype]),
						input.Position.w );

	//VertexInfo.z/85 => Will transform the Z into a range from 0 to 3
	output.EmissiveLight = input.VertexInfo.z/85 * saturate(input.Col.rgb +  SunColor * input.Col.a);
	output.EmissiveLight *= faceshades[facetype];

	output.fogPower = 0;//clamp( ((length(worldPosition.xyz) - fogdist) / foglength), 0, 1);
	output.BiomeData = input.BiomeData;
	output.Various = input.Various;

	// compute variable bias for the shadow map
	float3 normal = normalize(float3(normalsX[facetype], normalsY[facetype], normalsZ[facetype]));
	float3 sunlight = normalize(LightDirection);

	float cosTheta = clamp(dot(normal, sunlight), 0, 1);

	float bias = tan(acos(cosTheta));
	bias = clamp(bias, 0.0002, 0.0007);

	output.Bias = bias;

    return output;
}

float rand(float4 pos)
{
	float dot_product = dot(pos, float4(12.9898,78.233,45.164,94.673));
    return frac(sin(dot_product) * 43758.5453);
}

uint rndInd(uint max, float4 pos)
{
	return max * rand(pos) % max;
}

// ============================================================================
// Shadow Map Creation ==> not used ATM moment, stability problems, and too much impact on the GPU ! (Need to render the scene twice !)
// ============================================================================
 float CalcShadowFactor(float4 projTexC, float2 worldPos, float bias)
 {
 	// Complete projection by doing division by w.

 	projTexC.xyz /= projTexC.w;

	// Points outside the light volume are lit.
	if( projTexC.x < -1.0f || projTexC.x > 1.0f || projTexC.y < -1.0f || projTexC.y > 1.0f || projTexC.z < 0.0f) return 1.0f;
 	
 	// Transform from NDC space to texture space.
 	projTexC.x = +0.5f*projTexC.x + 0.5f;
 	projTexC.y = -0.5f*projTexC.y + 0.5f;
 	
 	// Depth in NDC space.
 	float depth = projTexC.z;
 	
	float visibility = 1.0f;

	// strange, it does not compile :(
	//for (uint i = 0; i < 16; i++)
	//{
	//	if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, i)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	{
	//		visibility -= 0.06f;
	//	}
	//}

	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rndInd(16, float4(worldPos.xyy, 0))]/3500.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.2f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rndInd(16, float4(worldPos.xyy, 1))]/3500.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.2f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rndInd(16, float4(worldPos.xyy, 2))]/3500.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.2f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rndInd(16, float4(worldPos.xyy, 3))]/3500.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.2f;

	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 0)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 1)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 2)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 3)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 4)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 5)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 6)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 7)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 8)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 9)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 10)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 11)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 12)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 13)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 14)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;
	//if (ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + poissonDisk[rand(16, worldPos, 15)]/2000.0f).r + SHADOW_EPSILON <= depth)
	//	visibility -= 0.06f;

	
 	// Sample shadow map to get nearest depth to light.
 	float s0 = ShadowMap.Sample(SamplerBackBuffer, projTexC.xy).r;
	float s1 = ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + float2(SMAP_DX, 0) ).r; 
	float s2 = ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + float2(0, SMAP_DX)).r; 
	float s3 = ShadowMap.Sample(SamplerBackBuffer, projTexC.xy + float2(SMAP_DX, SMAP_DX)).r; 

	// Is the pixel depth <= shadow map value?
	float result0 = depth <= s0 + bias;
	float result1 = depth <= s1 + bias;
	float result2 = depth <= s2 + bias;
	float result3 = depth <= s3 + bias;
 	
	
	// Transform to texel space.
	float2 texelPos = SMAP_SIZE*projTexC.xy;
 
	// Determine the interpolation amounts.
	float2 t = frac( texelPos );

 	// Interpolate results.
	return depth - bias < s0; // lerp(lerp(result0, result1, t.x), lerp(result2, result3, t.x), t.y); // depth - SHADOW_EPSILON < s0; //
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
PS_OUT PS(PS_IN input)
{
	PS_OUT output;

	float fogvalue = min( Opaque, 1 - input.fogPower);
	clip(fogvalue <= 0.001 ? -1:1); 

	float4 color = TerraTexture.Sample(SamplerDiffuse, input.UVW);
	
	clip(color.a < 0.1f ? -1:1 );    //Remove the pixel if alpha < 0.1

	//Apply Biome Color if the Alpha is < 1
	if(color.a < 1.0)
	{
		float3 samplingBiomeColor = {input.BiomeData.xy, input.Various.x };
	    float4 biomeColor =  BiomesColors.Sample(SamplerBackBuffer, samplingBiomeColor);
		color.r = color.r * biomeColor.r;
		color.g = color.g * biomeColor.g;
		color.b = color.b * biomeColor.b;
	}

	color = color * float4(input.EmissiveLight, 1);

	float4 finalColor = color;
	//To execute only when Fog is present !
	if(fogvalue < 1){
		float4 backBufferColor = { 0.0f, 0.0f, 0.2f, 1.0f }; //Defaulted Set to underWaterColor
		if(Various.x != 1) //Not Under Water and Fog present
		{
			//Get sky Color
			float2 backBufferSampling = {input.Position.x / BackBufferSize.x , input.Position.y / BackBufferSize.y};
			backBufferColor = SkyBackBuffer.Sample(SamplerBackBuffer, backBufferSampling);
		}

		//Compute Transparency, and blend current color with sky color in a blended way
		finalColor.rgb = (color.rgb * fogvalue) + (backBufferColor.rgb * (1 - fogvalue));
		finalColor.a = fogvalue;
	}
	
    float shadowFactor = CalcShadowFactor(input.projTexC, input.Position.xy / input.Position.w, input.Bias);
    finalColor.rbg *= clamp(shadowFactor, 0.5, 1);

	// Apply fog on output color
	output.Color = finalColor;

    return output;
}




