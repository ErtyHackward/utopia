//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerDraw
{
    int Size;
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
	return SpriteTexture.Sample(SpriteSampler, input.TexCoord);
}

float4 PS_BlurHorizontal( in PSInput input ) : SV_Target
{
    float4 Color = float4(0, 0, 0, 0);

	float step = 1.0f / Size;

	Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x - 5.0*step, input.TexCoord.y)) * 0.04f;
	Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x - 4.0*step, input.TexCoord.y)) * 0.05f;
    Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x - 3.0*step, input.TexCoord.y)) * 0.09f;
    Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x - 2.0*step, input.TexCoord.y)) * 0.12f;
    Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x - step, input.TexCoord.y)) * 0.15f;
    Color += SpriteTexture.Sample(SpriteSampler, input.TexCoord) * 0.16f;
    Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x + step, input.TexCoord.y)) * 0.15f;
    Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x + 2.0*step, input.TexCoord.y)) * 0.12f;
    Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x + 3.0*step, input.TexCoord.y)) * 0.09f;
	Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x + 4.0*step, input.TexCoord.y)) * 0.05f;
	Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x - 5.0*step, input.TexCoord.y)) * 0.04f;

    return Color;
}

float4 PS_BlurVertical( in PSInput input ) : SV_Target
{
    float4 Color = float4(0, 0, 0, 0);

	float step = 1.0f / Size;

	Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x, input.TexCoord.y - 5.0*step)) * 0.04f;
	Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x, input.TexCoord.y - 4.0*step)) * 0.05f;
    Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x, input.TexCoord.y - 3.0*step)) * 0.09f;
    Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x, input.TexCoord.y - 2.0*step)) * 0.12f;
    Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x, input.TexCoord.y - step)) * 0.15f;
    Color += SpriteTexture.Sample(SpriteSampler, input.TexCoord) * 0.16f;
    Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x, input.TexCoord.y + step)) * 0.15f;
    Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x, input.TexCoord.y + 2.0*step)) * 0.12f;
    Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x, input.TexCoord.y + 3.0*step)) * 0.09f;
	Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x, input.TexCoord.y + 4.0*step)) * 0.05f;
	Color += SpriteTexture.Sample(SpriteSampler, float2(input.TexCoord.x, input.TexCoord.y - 5.0*step)) * 0.04f;

    return Color;
}