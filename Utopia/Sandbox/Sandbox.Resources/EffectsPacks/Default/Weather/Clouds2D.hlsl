//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	matrix WorldViewProj;
	float2 UVOffset;
	float nbrLayers;
};

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	float3 Position		 : POSITION;
	float2 UV			 : TEXCOORD0;
	float  LayerNbr      : VARIOUS;
};

struct PS_IN
{
	float4 Position				: SV_POSITION;
	float2 UV					: TEXCOORD0;
	float  LayerNbr				: VARIOUS;
};

//--------------------------------------------------------------------------------------
//Sampler Definition With Texture
//--------------------------------------------------------------------------------------
Texture2D CloudTexture;
SamplerState cloudSampler;

//--------------------------------------------------------------------------------------
// Fonctions
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Vertex Shaders
//--------------------------------------------------------------------------------------

PS_IN VS(VS_IN input)
{
    PS_IN output;
	
	float4 newPosition = {input.Position.xyz, 1.0f};

    output.Position = mul(newPosition, WorldViewProj);
	output.UV = input.UV + UVOffset;
	output.LayerNbr = input.LayerNbr;
    return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(PS_IN input) : SV_Target
{
	float4 color =  CloudTexture.Sample(cloudSampler, input.UV);

	if(color.a < 0.1) discard;

	//color.r = saturate(lerp(color.r, color.r - 0.1, input.LayerNbr / nbrLayers));
	//color.g = saturate(lerp(color.g, color.g - 0.1, input.LayerNbr / nbrLayers));
	//color.b = saturate(lerp(color.b, color.b - 0.1, input.LayerNbr / nbrLayers));
	color.r -= 0.1;
	color.g -= 0.1;
	color.b -= 0.1;
	color.a = color.a / nbrLayers;

	// Apply fog on output color
    return color;
}

//--------------------------------------------------------------------------------------
technique11 Render
{
	//Normal Pass
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( 0 );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}

