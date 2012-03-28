//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerDraw
{
	matrix World;
}

cbuffer PerFrame
{
	matrix View;
	matrix Projection;
	float3 DiffuseLightDirection;
}

static const float4 DiffuseColor = {1.0f, 1.0f, 1.0f, 1.0f };
static const float4 AmbientColor = {0.4f, 0.4f, 0.4f, 1.0f };

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray DiffuseTexture;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	float3 Pos : POSITION;
	float3 Normal : NORMAL;
	float3 UVW  : TEXCOORD;
};

//Pixel shader Input
struct PS_IN
{
	float4 Pos : SV_POSITION;
	float3 Normal : NORMAL;
	float3 UVW  : TEXCOORD;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
PS_IN VS( VS_IN input )
{
	PS_IN output;
	
	output.Pos = float4(input.Pos.xyz, 1);
	output.Pos = mul( output.Pos, World );
	output.Pos = mul( output.Pos, View );
	output.Pos = mul( output.Pos, Projection );
	output.UVW = input.UVW;
	output.Normal = mul( float4(input.Normal,1), World ).xyz; 
	return output;
}

float4 PSTexturedCube( PS_IN input ) : SV_Target
{
	// Sample our texture at the specified texture coordinates to get the texture color
	float4 texColor = DiffuseTexture.Sample(SamplerDiffuse, input.UVW);

	float3 lightdir = normalize( DiffuseLightDirection );
	float3 norm = normalize( input.Normal );
	float4 diffuse = dot( lightdir, norm ) * DiffuseColor;

	float4 color = texColor * ( diffuse + AmbientColor );
	color.a = texColor.a;

	return color;

	//// Sample our texture at the specified texture coordinates to get the texture color
	//float4 texColor = tex2D( textureSampler, input.TexCoords );

	//// Calculate our specular component
	//float3 lightdir = normalize( DiffuseLightDirection );
	//float3 norm = normalize( input.Normal );
	//float3 halfAngle = normalize( lightdir + input.CameraView );
	//float specular = pow( saturate( dot( norm, halfAngle ) ), Shinniness ) * SpecularColor * SpecularIntensity;

	//// Calculate our diffuse component
	//float4 diffuse = dot( lightdir, input.Normal ) * DiffuseIntensity * DiffuseColor;
	//// Calculate our ambient component
	//float4 ambient = AmbientIntensity * AmbientColor;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PSWhiteCube( PS_IN input ) : SV_Target
{
	return float4(1,1,1,1);
}
