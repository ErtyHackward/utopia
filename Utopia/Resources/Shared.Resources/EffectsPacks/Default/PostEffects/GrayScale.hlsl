//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput
{
    float2 Position : POSITION;
	float2 TexCoord : TEXCOORD;
};

//Pixel Shader input
struct VSOutput
{
    float4 Position : SV_Position;	//The position.X and .Y MUST be in screen space directly. It means from -1;-1 (Below left) to 1;1 (Top Right)
	float2 TexCoord : TEXCOORD;
};

//--------------------------------------------------------------------------------------
// Textures
//--------------------------------------------------------------------------------------
Texture2D PostEffectBackBuffer;
SamplerState SamplerPostEffectBackBuffer;


//======================================================================================
// Vertex Shader, non-instanced
//======================================================================================
//[VS ENTRY POINT]
VSOutput VS(in VSInput input)
{
	VSOutput output;
	float4 position = float4(input.Position, 0.0f, 1.0f);
	output.Position = position;
	output.TexCoord = input.TexCoord;
    return output;
}

//======================================================================================
// Pixel Shader
//======================================================================================
//[PS ENTRY POINT]
float4 PS(in VSOutput input) : SV_Target
{
	float4 backBufferColor = PostEffectBackBuffer.Sample(SamplerPostEffectBackBuffer, input.TexCoord);
	backBufferColor.rgb = dot(backBufferColor.rgb, float3(0.3, 0.59, 0.11));
	return backBufferColor;
}
