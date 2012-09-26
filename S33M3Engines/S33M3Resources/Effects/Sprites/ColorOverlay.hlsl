
//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerDraw
{
    float4 OverlayColor;
};

//======================================================================================
// Samplers
//======================================================================================
Texture2D SpriteTexture;
SamplerState SpriteSampler;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput
{
    float2 Position : POSITION;
    float2 TexCoord : TEXCOORD;
};

//Pixel Shader input
struct PSInput
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD;
};

//======================================================================================
// Vertex Shader, non-instanced
//======================================================================================
PSInput VS(in VSInput input)
{
	PSInput output;
	float4 position = float4(input.Position, 0.0f, 1.0f);
    
	output.Position = position;
	output.TexCoord = input.TexCoord;

    return output;
}

//======================================================================================
// Pixel Shader
//======================================================================================
float4 PS(in PSInput input) : SV_Target
{
	float4 spriteSampledColor = SpriteTexture.Sample(SpriteSampler, input.TexCoord);

	clip(spriteSampledColor.a < 0.1f ? -1:1);

	return OverlayColor;
}
