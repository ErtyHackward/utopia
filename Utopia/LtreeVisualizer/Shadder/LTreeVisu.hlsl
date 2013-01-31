//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerDraw
{
	matrix World;
}

cbuffer PerFrame
{
	matrix ViewProjection;
}

static const float faceshades[6] = { 0.6, 0.6, 0.8, 1.0, 0.7, 0.8 };

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	float4 Position				: POSITION;
	float4 Color				: COLOR;
};

struct GSInput {
	float4 Position	 			: POSITION;
	float4 Color				: COLOR;
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
	float4 Color				: COLOR;
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------

GSInput VS (VSInput input)
{
	GSInput output;
	
	float4 newPosition = {input.Position.x, input.Position.y, input.Position.z, 1.0 };

	output.Position = mul(newPosition, World);				  
	output.Position = mul( output.Position, ViewProjection ); 
	output.Color = input.Color * input.Position.w;

	return output;
}


[maxvertexcount(3)]
void GS(triangle GSInput Inputs[3], uint primID : SV_PrimitiveID, inout TriangleStream<PSInput> TriStream)
{
	//Compute the Face ID.
	// Modulo 12 give back the triangle ID (cube being composed of 12 triangles)
	// /2 Give back the face Id ! Easy !
	Inputs[0].Color *= faceshades[(primID % 12) / 2];
	Inputs[1].Color = Inputs[0].Color;
	Inputs[2].Color = Inputs[0].Color;

	TriStream.Append(Inputs[0]);
	TriStream.Append(Inputs[1]);
	TriStream.Append(Inputs[2]);
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

float4 PS(PSInput IN) : SV_Target
{	
	//Texture Sampling
	float4 color = IN.Color;

	return color;
}

//--------------------------------------------------------------------------------------
