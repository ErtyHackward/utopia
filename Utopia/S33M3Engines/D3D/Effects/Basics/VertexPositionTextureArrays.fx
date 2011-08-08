//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
matrix World;
matrix View;
matrix Projection;

int TexInd=0;
//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray DiffuseTexture;
SamplerState SamplerDiffuse
{
	Filter = MIN_MAG_MIP_POINT;
	AddressU = CLAMP; 
	AddressV = CLAMP;
	MipLODBias = 0;
};

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VS_IN
{
	float3 Pos : POSITION;
	float2 UV  : TEXCOORD;
};

//Geometry shader Input
struct GS_IN
{
	float3 Pos	: POSITION;
	float2 UV	: TEXCOORD;
};

//Pixel shader Input
struct PS_IN
{
	float4 Pos : SV_POSITION;
	float2 UV  : TEXCOORD;
};
//--------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
GS_IN VS( VS_IN input )
{
	GS_IN output;
	
	output.Pos = float4(input.Pos.xyz, 1);
	output.UV = input.UV;
	
	return output;
}


//--------------------------------------------------------------------------------------
// Geometry Shader
//--------------------------------------------------------------------------------------
[maxvertexcount(4)]
void GS( point GS_IN input[1], inout TriangleStream<PS_IN> outputStream )
{
	PS_IN v;
	//create sprite quad
	//--------------------------------------------

	//bottom left
	v.Pos = float4(input[0].Pos.x - 5.5f,input[0].Pos.y - 5.5f,input[0].Pos.z,1);
	v.UV = float2(0,1);

	v.Pos = mul( v.Pos, World );
	v.Pos = mul( v.Pos, View );
	v.Pos = mul( v.Pos, Projection );

	outputStream.Append(v);

	//top left
	v.Pos = float4(input[0].Pos.x - 5.5f,input[0].Pos.y + 5.5f,input[0].Pos.z,1);
	v.UV = float2(0,0);

	v.Pos = mul( v.Pos, World );
	v.Pos = mul( v.Pos, View );
	v.Pos = mul( v.Pos, Projection );

	outputStream.Append(v);

	//bottom right
	v.Pos = float4(input[0].Pos.x + 5.5f,input[0].Pos.y - 5.5f,input[0].Pos.z,1);
	v.UV = float2(1,1);

	v.Pos = mul( v.Pos, World );
	v.Pos = mul( v.Pos, View );
	v.Pos = mul( v.Pos, Projection );

	outputStream.Append(v);

	//top right
	v.Pos = float4(input[0].Pos.x + 5.5f,input[0].Pos.y + 5.5f,input[0].Pos.z,1);
	v.UV = float2(1,0);

	v.Pos = mul( v.Pos, World );
	v.Pos = mul( v.Pos, View );
	v.Pos = mul( v.Pos, Projection );

	outputStream.Append(v);
}


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS( PS_IN input ) : SV_Target
{
	float3 uvw = float3(input.UV, TexInd);
	float4 color = DiffuseTexture.Sample(SamplerDiffuse, uvw);

	return color;
}

//--------------------------------------------------------------------------------------
technique10 Render
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VS() ) );
		SetGeometryShader( CompileShader( gs_4_0, GS() ) );
		SetPixelShader( CompileShader( ps_4_0, PS() ) );
	}
}