//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerDraw  
{
    matrix OrthoProjection;
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
    float3 Position : POSITION;
    float3 TexCoord : TEXCOORD;
	float4 Color    : COLOR;
};

//Pixel Shader input
struct PSInput
{
    float4 Position : SV_Position;
    float3 TexCoord : TEXCOORD;
    float4 Color : COLOR;
};


//======================================================================================
// Vertex Shader, non-instanced
//======================================================================================
PSInput SpriteVS(in VSInput input)
{
	PSInput output;

	float4 Posi = float4(input.Position.xyz, 1);
	
	output.Position = mul(Posi, OrthoProjection);
	output.TexCoord = input.TexCoord.xyz;
	output.Color = input.Color;

	return output;
}

//======================================================================================
// Pixel Shader
//======================================================================================
float4 SpritePS(in PSInput input) : SV_Target
{
    float4 texColor = SpriteTexture.Sample(SpriteSampler, input.TexCoord);    
	clip(texColor.a < 0.001f ? -1:1 );
    texColor = texColor * input.Color;    
    texColor.rgb *= texColor.a;
	return texColor;
	//return float4(1,1,1,1);
}
