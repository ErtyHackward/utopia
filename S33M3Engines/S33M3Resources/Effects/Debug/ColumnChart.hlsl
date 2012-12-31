//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer CBPerDraw 
{
    float2 ViewportSize;
};

//--------------------------------------------------------------------------------------
//Vertex shader Input - Instanced
struct VSInput
{
    float2 Position : POSITION;
    float4 Transform : TRANSFORM;
    float4 Color : COLOR;
};

//Pixel Shader input
struct VSOutput
{
    float4 Position : SV_Position;
    float4 Color : COLOR;
};

//======================================================================================
// Vertex Shader, instanced
//======================================================================================
//[VS ENTRY POINT]
VSOutput VS(in VSInput input)
{
	VSOutput output;

	float4 positionSS;

	[branch] 
	if(input.Transform.w)
	{
		positionSS = float4(input.Position.y * input.Transform.z, input.Position.x * input.Transform.z, 0.0f, 1.0f);
	}else{
		positionSS = float4(input.Position.x * input.Transform.z, input.Position.y * input.Transform.z, 0.0f, 1.0f);
	}

	//Do the scaling
	//float4 positionSS = float4(input.Position.x * input.Transform.z, input.Position.y * input.Transform.z, 0.0f, 1.0f);
	//Do the translation
	positionSS.x += input.Transform.x;
	positionSS.y += input.Transform.y;

	// Scale by the viewport size, flip Y, then rescale to device coordinates
    float4 positionDS = positionSS;
    positionDS.xy /= ViewportSize;
    positionDS.xy = positionDS.xy * 2 - 1;

	output.Position = positionDS;
	output.Color = input.Color;

	return output;
}

//======================================================================================
// Pixel Shader
//======================================================================================
//[PS ENTRY POINT]
float4 PS(in VSOutput input) : SV_Target
{
	return input.Color;
}
