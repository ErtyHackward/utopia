
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
    float3 TexCoord : TEXCOORD;
};

//======================================================================================
// Vertex Shader, non-instanced
//======================================================================================
PSInput VS(in VSInput input)
{
	PSInput output;
	float4 position = float4(input.Position, 0.0f, 1.0f);
    
	output.Position = position;

    return output;
}

//======================================================================================
// Pixel Shader
//======================================================================================
float4 PS(in PSInput input) : SV_Target
{
	return float4(1,1,1,1);
}
