//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerDraw
{
	matrix OrthoProjection;
};

cbuffer TextureTransform
{
	matrix TexMatrix;
};

//======================================================================================
// Samplers
//======================================================================================
Texture2DArray SpriteTexture;
SamplerState SpriteSampler;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput
{
	float3 Position : POSITION;
	float3 TexCoord : TEXCOORD;
	float4 Color    : COLOR;
	float4 Wrap     : VARIOUS;
};

//Pixel Shader input
struct PSInput
{
	float4 Position : SV_Position;
	float3 TexCoord : TEXCOORD;
	float4 Color    : COLOR;
	float4 Wrap     : VARIOUS;
};


//======================================================================================
// Vertex Shader, non-instanced
//======================================================================================
//[VS ENTRY POINT]
PSInput SpriteVS(in VSInput input)
{
	PSInput output;

	float4 Posi = float4(input.Position.xyz, 1);

	output.Position = mul(Posi, OrthoProjection);
	output.TexCoord.xyz = mul(float4(input.TexCoord.xy, 0.0f, 1.0f), TexMatrix).xyz;
	output.TexCoord.z = input.TexCoord.z;
	output.Color = input.Color;
	output.Wrap = input.Wrap;

	return output;
}

//======================================================================================
// Pixel Shader
//======================================================================================
//[PS ENTRY POINT]
float4 SpritePS(in PSInput input) : SV_Target
{
	float4 texColor;

	if (input.Wrap.x != 1 && input.Wrap.y != 0 && input.Wrap.z != 1 && input.Wrap.w != 0)
	{
		float3 texCoord = float3(input.TexCoord.x % input.Wrap.x + input.Wrap.y, input.TexCoord.y % input.Wrap.z + input.Wrap.w, input.TexCoord.z);
		texColor = SpriteTexture.Sample(SpriteSampler, texCoord);
	}
	else
		texColor = SpriteTexture.Sample(SpriteSampler, input.TexCoord);

	clip(texColor.a < 0.01f ? -1 : 1);
	texColor = texColor * input.Color;
	return texColor;
}

//[PS ENTRY POINT]
float4 SpritePSNoClip(in PSInput input) : SV_Target
{
	float4 texColor;

	if (input.Wrap.x != 1 && input.Wrap.y != 0 && input.Wrap.z != 1 && input.Wrap.w != 0)
	{
		float3 texCoord = float3(input.TexCoord.x % input.Wrap.x + input.Wrap.y, input.TexCoord.y % input.Wrap.z + input.Wrap.w, input.TexCoord.z);
		texColor = SpriteTexture.Sample(SpriteSampler, texCoord);
	}
	else
		texColor = SpriteTexture.Sample(SpriteSampler, input.TexCoord);

	texColor = texColor * input.Color;
	return texColor;
}
