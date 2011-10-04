//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerFrame
{
	matrix WorldFocus;
	matrix ViewProjection;
	float3 SunColor;			  // Diffuse lighting color
	float fogdist;
    float3 WindPower;
};

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray DiffuseTexture;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	float3 Position				: POSITION;
	float4 Color				: COLOR;
	float3 Textcoord     		: TEXCOORD;
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
	float3 UVW					: TEXCOORD0;
	float fogPower				: VARIOUS0;
	float3 EmissiveLight		: Light0;
};

//--------------------------------------------------------------------------------------

static const float foglength = 45;
static float3 Dayfogcolor = {0.7, 0.7, 0.7 };
static float3 Nightfogcolor = {0, 0, 0 };

// offsets for the 4 vertices
static const float3 p[4] = {
							{-0.5, 0.0f, 0.0f},
							{-0.5, 1.0f, 0.0f},
							{0.5, 0.0f, 0.0f},
							{0.5, 1.0f, 0.0f}
						};

static const float texcoordU[4] = { 0.0f, 0.0f, 1.0f, 1.0f};
static const float texcoordV[4] = { 1.0f, 0.0f, 1.0f, 0.0f};	

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
PSInput VS (VSInput input)
{
	PSInput output;

	float4 worldPosition = {input.Position.xyz, 1.0f};
	worldPosition = mul(worldPosition, WorldFocus);

	if (input.Textcoord.y <= 0.1)
	{
	      //worldPosition.x += WindPower.x;
		  //worldPosition.z += WindPower.z;
		  float sine = sin(input.Position.z / 20) * 1.5; // * Time variable to make it move !
		  worldPosition.xyz += sine * WindPower;
	}

	output.Position = mul(worldPosition, ViewProjection);
	output.UVW = input.Textcoord;

	output.fogPower = clamp( ((length(worldPosition.xyz) - fogdist) / foglength), 0, 1);
	output.EmissiveLight = saturate(input.Color.rgb +  SunColor * input.Color.a);

	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(PSInput IN) : SV_Target
{	
	//Texture Sampling
	float4 color = DiffuseTexture.Sample(SamplerDiffuse, IN.UVW) * float4(IN.EmissiveLight, 1);;
	
	clip( color.a < 0.1f ? -1:1 ); //Remove the pixel if alpha < 0.1

	float4 Finalfogcolor = {SunColor / 1.5, color.a};
	color = lerp(color, Finalfogcolor, IN.fogPower);

	return color;	
}

//--------------------------------------------------------------------------------------
