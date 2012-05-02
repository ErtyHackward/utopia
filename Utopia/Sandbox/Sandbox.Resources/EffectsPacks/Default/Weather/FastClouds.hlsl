//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerDraw
{
	matrix World;
	float Brightness;
}

cbuffer PerFrame
{
	matrix ViewProjection;
	float3 SunColor;			  // Diffuse lighting color
	float fogdist;
	float2 BackBufferSize;
};

static const float foglength = 20;

Texture2D SolidBackBuffer;
SamplerState SamplerBackBuffer;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	float3 Pos : POSITION;
	float4 Col : COLOR;
	float2 Offset : POSITION1;
};

//Pixel shader Input
struct PS_IN
{
	float4 Pos : SV_POSITION;
	float fogPower : VARIOUS0;
	float4 Col : COLOR;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
PS_IN VS( VS_IN input )
{
	PS_IN output;
	
	float4x4 offsetMatrix =   
	{  
		{1,0,0,0},  
		{0,1,0,0},  
		{0,0,1,0},  
		{0,0,0,1}  
	}; 

	offsetMatrix._41 = input.Offset.x;
	offsetMatrix._43 = input.Offset.y;

	output.Pos = float4(input.Pos.xyz, 1);
	output.Pos = mul( output.Pos, World );
	output.Pos = mul( output.Pos, offsetMatrix );
	output.Pos = mul( output.Pos, ViewProjection );
	output.Col = float4(input.Col.rgb * Brightness, input.Col.a);
	
	output.fogPower = clamp(((length(output.Pos.xyz) - 900) / foglength), 0, 1);

	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS( PS_IN input ) : SV_Target
{
    float2 backBufferSampling = {input.Pos.x / BackBufferSize.x , input.Pos.y / BackBufferSize.y};
    float4 backBufferColor = SolidBackBuffer.Sample(SamplerBackBuffer, backBufferSampling);

	input.Col.a = lerp(input.Col.a, 0, input.fogPower);

	//Manual Blending with SolidBackBuffer color received
	float4 color = {(input.Col.rgb * input.Col.a) + (backBufferColor.rgb * (1 - input.Col.a)), input.Col.a};
	return color;
}
