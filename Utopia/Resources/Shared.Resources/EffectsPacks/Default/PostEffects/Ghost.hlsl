//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	float4 Params;				// x : Fading value. y : Fade mode (1.0f = FadeIn, 2.0f = FadeOut)
}

static const float2 Center = { 0.5, 0.5 }; ///center of the screen (could be any place)
static const float BlurStart = 1.0f; /// blur offset
static const float BlurWidth = -0.1; ///how big it should be
static const int nsamples = 10;
static const float sampleStrength = 2.2f;
static const float4 startupColor = { 0.0f, 0.0f, 0.0f, 1.0f };

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

	float4 backBufferColor = PostEffectBackBuffer.Sample(SamplerPostEffectBackBuffer, input.TexCoord);
	float4 color = backBufferColor;

	//Radial bluur effect
	if (Params.y == 1.0f)
	{
		float4 c = 0;
		for (int i = 0; i < nsamples; i++) {
			float scale = BlurStart + BlurWidth*(i / (float)(nsamples - 1));
			c += PostEffectBackBuffer.Sample(SamplerPostEffectBackBuffer, UV * scale + Center);
		}
		c /= nsamples;
		color = lerp(color, c, dist);
	}

	//Grayscale transform
	color.rgb = dot(color.rgb, float3(0.3, 0.59, 0.11));

	//Fading effect
	if (Params.y == 1.0f)
	{
		color = lerp(startupColor, color, Params.x);
	}
	else
	{
		color = lerp(color, backBufferColor, Params.x);
	}

	return color;
}
