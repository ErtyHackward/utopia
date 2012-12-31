//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerDraw
{
    float4 Color;
};

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput
{
    float2 Position : POSITION;
};

//Pixel Shader input
struct VSOutput
{
    float4 Position : SV_Position;	//The position.X and .Y MUST be in screen space directly. It means from -1;-1 (Below left) to 1;1 (Top Right)
    float4 Color : COLOR;
};

//======================================================================================
// Vertex Shader, non-instanced
//======================================================================================
//[VS ENTRY POINT]
VSOutput VS(in VSInput input)
{
	VSOutput output;
	float4 position = float4(input.Position, 0.0f, 1.0f);
    
	output.Position = position;
	output.Color = Color;

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
