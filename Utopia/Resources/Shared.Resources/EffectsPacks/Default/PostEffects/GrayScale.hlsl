//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------


static const float2 Center = { 0.5, 0.5 }; ///center of the screen (could be any place)
static const float BlurStart = 1.0f; /// blur offset
static const float BlurWidth = -0.1; ///how big it should be
static const int nsamples = 10;
static const float sampleStrength = 2.2f;

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
	float2 UV = input.TexCoord - Center;

	float dist = clamp(length(UV) * sampleStrength, 0.0f, 1.0f);

	float4 color = PostEffectBackBuffer.Sample(SamplerPostEffectBackBuffer, input.TexCoord);

	float4 c = 0;
	for (int i = 0; i < nsamples; i++) {
		float scale = BlurStart + BlurWidth*(i / (float)(nsamples - 1));
		c += PostEffectBackBuffer.Sample(SamplerPostEffectBackBuffer, UV * scale + Center);
	}
	c /= nsamples;

	c = lerp(color, c, dist);

	//float4 backBufferColor = PostEffectBackBuffer.Sample(SamplerPostEffectBackBuffer, input.TexCoord);
	//backBufferColor.rgb = dot(backBufferColor.rgb, float3(0.3, 0.59, 0.11));
	c.rgb = dot(c.rgb, float3(0.3, 0.59, 0.11));
	return c;
}
