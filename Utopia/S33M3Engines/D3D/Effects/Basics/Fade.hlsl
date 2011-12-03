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
    float4 Position : SV_Position;
    float4 Color : COLOR;
};

//======================================================================================
// Vertex Shader, non-instanced
//======================================================================================
VSOutput FadeVS(in VSInput input)
{
	VSOutput output;
	float4 position = float4(input.Position, 0.0f, 1.0f);
    
    // Apply transforms in screen space
    //position = mul(position, Transform); // == Simple translation

	output.Position = position;
	output.Color = Color;

    return output;
}

//======================================================================================
// Pixel Shader
//======================================================================================
float4 FadePS(in VSOutput	 input) : SV_Target
{
	return input.Color;
}
