//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerFrameLocal
{
	matrix WorldFocus;
	matrix View;
    float3 WindPower;
	float keyFrameAnimation;
};

cbuffer PerFrame
{
	matrix ViewProjection;
	float3 SunColor;			  // Diffuse lighting color
	float fogdist;
};

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray DiffuseTexture;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	float4 Position				: POSITION;
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

//Billboard corners, 0 being no billboards
static const float3 billboardCorners[5] = {
											{0, 0.0f, 0.0f},
											{-0.5, 0.5f, 0.0f},
											{0.5, 0.5f, 0.0f},
											{0.5, -0.5f, 0.0f},
											{-0.5, -0.5f, 0.0f}
									};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
PSInput VS (VSInput input)
{
	PSInput output;

	float4 worldPosition = {input.Position.xyz, 1.0f};
	
	//Rotating the Position if Billboard
	float3 billboardVertexPosition = billboardCorners[input.Position.w];

	//Rotating it
	billboardVertexPosition = float4(mul(billboardVertexPosition, (float3x3)View),0); //Rotate the billboard

	//Translating/scaling it back to world position
	worldPosition.x = billboardVertexPosition.x + (input.Position.x - billboardCorners[input.Position.w].x);
	worldPosition.y = billboardVertexPosition.y + (input.Position.y - billboardCorners[input.Position.w].y);
	worldPosition.z = billboardVertexPosition.z + (input.Position.z - billboardCorners[input.Position.w].z);




	//Billboard rotation ?
	//if(input.Position.w == 1)
	//{
	//	worldPosition = float4(mul(worldPosition, (float3x3)View),0); //Rotate the vertex
	//}

	worldPosition = mul(worldPosition, WorldFocus); //Translate to vertex to the correct location

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
