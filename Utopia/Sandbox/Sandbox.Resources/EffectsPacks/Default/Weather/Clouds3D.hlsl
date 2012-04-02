//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerDraw
{
	matrix World;
}

cbuffer PerFrame
{
	matrix ViewProjection;
	float3 SunColor;			  // Diffuse lighting color
	float fogdist;
	float2 BackBufferSize;
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
	float3 Pos : POSITION;
	float4 Col : COLOR;
};

//Pixel shader Input
struct PS_IN
{
	float4 Pos : SV_POSITION;
	float4 Col : COLOR;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
PS_IN VS( VS_IN input )
{
	PS_IN output;
	
	output.Pos = float4(input.Pos.xyz, 1);
	output.Pos = mul( output.Pos, World );
	output.Pos = mul( output.Pos, ViewProjection );
	output.Col = input.Col;
	
	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS( PS_IN input ) : SV_Target
{
    float2 backBufferSampling = {input.Pos.x / BackBufferSize.x , input.Pos.y / BackBufferSize.y};
    float4 backBufferColor = SolidBackBuffer.Sample(SamplerBackBuffer, backBufferSampling);
	//Manual Blending with SolidBackBuffer color received
	float4 color = {(input.Col.rgb * input.Col.a) + (backBufferColor.rgb * (1 - input.Col.a)), input.Col.a};
	return color;
}
