//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerDraw
{
	matrix World;
	float Brightness;
}

#include <SharedFrameCB.hlsl>

static const float foglength = 20;

Texture2D SkyBackBuffer;
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
//[VS ENTRY POINT]
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
	output.Pos = mul( output.Pos, ViewProjection_focused );
	output.Col = float4(input.Col.rgb * Brightness, input.Col.a);
	
	output.fogPower = clamp(((length(output.Pos.xyz) - 900) / foglength), 0, 1);

	return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//-------------------------------------------------------------------------------------
//[PS ENTRY POINT]
float4 PS( PS_IN input ) : SV_Target
{
	clip(FogType != 2.0 && input.fogPower >= 0.998 ? -1:1);

	float4 backBufferColor;

	if(FogType == 0.0)
	{
		//Get sky Color
		float2 backBufferSampling = {input.Pos.x / BackBufferSize.x , input.Pos.y / BackBufferSize.y};
		backBufferColor = SkyBackBuffer.Sample(SamplerBackBuffer, backBufferSampling);
	}else{
		if(FogType == 1.0)
		{
			backBufferColor.xyz = SunColor / 1.5;
			backBufferColor.w = input.Col.a;
		}else{
			backBufferColor = float4(0.0,0.0,0.0,0.0);
		}
	}

	float4 finalColor;
	//Compute Transparency, and blend current color with sky color in a blended way
	if(FogType != 2.0)
	{
		input.Col.a = lerp(input.Col.a, 0, input.fogPower);
		finalColor.rgb = (input.Col.rgb * input.Col.a) + (backBufferColor.rgb * (1 - input.Col.a));
		finalColor.a = input.Col.a;
	}else{
		finalColor = input.Col;
	}
	//Manual Blending with SolidBackBuffer color received

	return finalColor;
}
