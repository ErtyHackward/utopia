//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerBatch 
{
    float2 TextureSize;
    float2 ViewportSize;
};

cbuffer PerInstance  
{
    matrix Transform;
    float4 Color;
    float4 SourceRect;
	uint TexIndex;
	float Depth;
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
    float2 Position : POSITION;
    float2 TexCoord : TEXCOORD;
};

//Vertex shader Input - Instanced
struct VSInputInstanced
{
    float2 Position : POSITION;
    float2 TexCoord : TEXCOORD;
    float4x4 Transform : TRANSFORM;
    float4 Color : COLOR;
    float4 SourceRect : SOURCERECT;
	uint TexIndex : TEXINDEX;
	float Depth : DEPTH;
};

//Pixel Shader input
struct VSOutput
{
    float4 Position : SV_Position;
    float3 TexCoord : TEXCOORD;
    float4 Color : COLOR;
};

//-------------------------------------------------------------------------------------
// Functionality common to both vertex shaders
//-------------------------------------------------------------------------------------
VSOutput SpriteVSCommon(float2 position, 
                        float2 texCoord, 
                        float4x4 transform, 
                        float4 color, 
                        float4 sourceRect,
						uint texIndex,
						float depth)
{
    // Scale the quad so that it's texture-sized    
    float4 positionSS = float4(position * sourceRect.zw, 0.0f, 1.0f);
    
    // Apply transforms in screen space
    positionSS = mul(positionSS, transform); // == Simple translation

	positionSS.z = depth;
	positionSS.w = 1;

    // Scale by the viewport size, flip Y, then rescale to device coordinates
    float4 positionDS = positionSS;
    positionDS.xy /= ViewportSize;
    positionDS.xy = positionDS.xy * 2 - 1;
    positionDS.y *= -1;

    // Figure out the texture coordinates
    float3 outTexCoord = float3(
						texCoord.x, 
						texCoord.y,
						texIndex
						);

    outTexCoord.xy *= sourceRect.zw / TextureSize;
    outTexCoord.xy += sourceRect.xy / TextureSize;

    VSOutput output;
    output.Position = positionDS;
    output.TexCoord = outTexCoord;
    output.Color = color;

    return output;
}

//======================================================================================
// Vertex Shader, non-instanced
//======================================================================================
VSOutput SpriteVS(in VSInput input)
{
    return SpriteVSCommon(input.Position, input.TexCoord, Transform, Color, SourceRect, TexIndex, Depth);
}

//======================================================================================
// Vertex Shader, instanced
//======================================================================================
VSOutput SpriteInstancedVS(in VSInputInstanced input)
{
    return SpriteVSCommon(input.Position, input.TexCoord, 
                            transpose(input.Transform), input.Color, input.SourceRect,input.TexIndex,input.Depth); 
}

//======================================================================================
// Pixel Shader
//======================================================================================
float4 SpritePS(in VSOutput input) : SV_Target
{
    float4 texColor = SpriteTexture.Sample(SpriteSampler, input.TexCoord);    
    texColor = texColor * input.Color;    
    texColor.rgb *= texColor.a;
    return texColor;
}
