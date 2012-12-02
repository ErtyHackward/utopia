//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------
cbuffer PerFrameLocal
{
	matrix ViewProjection;
	float3 CameraWorldPosition;
	float3 LookAt;
};

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray DiffuseTexture;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	matrix WorldTransform		: TRANSFORM;
	float4 Position				: POSITION;   //XYZ world location, W = texture array indice
	float4 textCoordU			: TEXC0;
	float4 textCoordV			: TEXC1;
	float4 Color				: COLOR;
	float2 Size					: SIZE;       //XY : Size, Z = Geometry Type (0 = Position Facing Billboard, 1 = View Facing Billboard)
};

//--------------------------------------------------------------------------------------
//Geometry shader Input
struct GSInput {
	matrix WorldTransform		: TRANSFORM;
	float4 Position				: POSITION;   //XYZ world location, W = texture array indice
	float4 textCoordU			: TEXC0;
	float4 textCoordV			: TEXC1;
	float4 Color				: COLOR;
	float2 Size					: SIZE;       //XY : Size, Z = Geometry Type (0 = Position Facing Billboard, 1 = View Facing Billboard)
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
	float4 Color				: COLOR;
	float3 UVW					: TEXCOORD0;
};


//Billboard corners, 0 being no billboards
static const float4 billboardCorners[4] = {
											{0.5, 0.0f, 0.0f, 1.0f},  //Botom right corner
											{-0.5, 0.0f, 0.0f, 1.0f}, //Botom left corner
											{0.5, 1.0f, 0.0f, 1.0f},  //Top right corner
											{-0.5, 1.0f, 0.0f, 1.0f}  //Top left corner
										  };

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
GSInput VS (VSInput input)
{
	return input;
}

[maxvertexcount(4)]
void GS(point GSInput Inputs[1]: POSITION0, inout TriangleStream<PSInput> TriStream)
{
	PSInput Output;
	GSInput Input = Inputs[0];
	
	float texcoordU[4];
	texcoordU[0] = Input.textCoordU.x;
	texcoordU[1] = Input.textCoordU.y;
	texcoordU[2] = Input.textCoordU.z;
	texcoordU[3] = Input.textCoordU.w;

	float texcoordV[4];
	texcoordV[0] = Input.textCoordV.x;
	texcoordV[1] = Input.textCoordV.y;
	texcoordV[2] = Input.textCoordV.z;
	texcoordV[3] = Input.textCoordV.w;

	// *****************************************************
	// generate the 4 vertices to make two triangles
	for( uint i = 0 ; i < 4 ; i++ )
	{
		//Get the billboard template corner
		float4 billboardPosition = billboardCorners[i];

		//Scale to billboard local size
		billboardPosition.xy *= Input.Size; 

		billboardPosition.xy += Input.Position.xy; 

		//Tranform billboardPosition from Local => World 
		float4 WorldPosition = mul(mul(billboardPosition, Input.WorldTransform), ViewProjection);

		Output.Position = WorldPosition;
		Output.Color = Input.Color;
		Output.UVW = float3( texcoordU[i], 
							 texcoordV[i],
							 Input.Position.w);

		//Transform point in screen space
		TriStream.Append( Output );
	}
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(PSInput IN) : SV_Target
{	
	//Texture Sampling
	float alpha = DiffuseTexture.Sample(SamplerDiffuse, IN.UVW).a;

	clip( alpha < 0.1f ? -1:1 ); //Remove the pixel if alpha < 0.1

	IN.Color.a = alpha;

	return IN.Color;	
}

//--------------------------------------------------------------------------------------
