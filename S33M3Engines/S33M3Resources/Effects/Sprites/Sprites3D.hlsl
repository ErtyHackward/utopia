//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerFrameLocal
{
	matrix WorldFocus;
	matrix View;
};

cbuffer PerFrame
{
	matrix ViewProjection;
	float3 SunColor;			  // Diffuse lighting color
	float fogdist;
	float2 BackBufferSize;
	float2 Various;               //.x = 1 if head under water
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
	float2 Size					: SIZE;
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
	float3 UVW					: TEXCOORD0;
	float3 EmissiveLight		: Light0;
};

struct PS_OUT
{
	float4 Color				: SV_TARGET0;
};

//--------------------------------------------------------------------------------------

static const float foglength = 45;

//Billboard corners, 0 being no billboards
static const float3 billboardCorners[5] = {
											{0.0f, 0.0f, 0.0f},
											{-0.5, 1.0f, 0.0f},
											{0.5, 1.0f, 0.0f},
											{0.5, 0.0f, 0.0f},
											{-0.5, 0.0f, 0.0f}
										  };

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
//[VS ENTRY POINT]
PSInput VS (VSInput input)
{
	PSInput output;
	float4 worldPosition;

	[branch] if(input.Position.w > 0) //If i'm a billboard
	{
		//Get the billboard template corner
		float3 billboardVertexPosition = billboardCorners[input.Position.w];
		//Multiply the template by the Billboard size (Scale)
		billboardVertexPosition *= float3(input.Size, 1);

		//Rotating it
		billboardVertexPosition = float3(mul(billboardVertexPosition, (float3x3)View)); //Rotate the billboard following Camera position

		//Translating
		worldPosition = float4(billboardVertexPosition.xyz + input.Position.xyz, 1.0f);
	}else{ 
		//I'm not a billboard
		worldPosition = float4(input.Position.xyz, 1.0f);
	}

	worldPosition = mul(worldPosition, WorldFocus); //Translate to vertex to the correct location
	output.Position = mul(worldPosition, ViewProjection);
	output.UVW = input.Textcoord;

	output.EmissiveLight = saturate(input.Color.rgb +  SunColor * input.Color.a);

	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
//[PS ENTRY POINT]
PS_OUT PS(PSInput IN)
{	
	PS_OUT output;
	//Texture Sampling
	float4 color = DiffuseTexture.Sample(SamplerDiffuse, IN.UVW) * float4(IN.EmissiveLight, 1);
	
	clip( color.a < 0.1f ? -1:1 ); //Remove the pixel if alpha < 0.1

	output.Color = color;
    return output;
}

//--------------------------------------------------------------------------------------
