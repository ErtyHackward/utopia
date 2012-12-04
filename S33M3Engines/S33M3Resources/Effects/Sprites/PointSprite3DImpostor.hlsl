//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

#include <SharedFrameCB.hlsl>

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2DArray DiffuseTexture;
SamplerState SamplerDiffuse;

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct VSInput {
	float4 Position				: POSITION;   //XYZ world location, W = texture array indice
	float4 Color				: COLOR;
	float2 Size					: SIZE;       //XY : Size
};

//--------------------------------------------------------------------------------------
//Geometry shader Input
struct GSInput {
	float4 Position				: POSITION;
	float4 Color				: COLOR;
	float2 Size					: SIZE;
};

//Pixel shader Input
struct PSInput {
	float4 Position	 			: SV_POSITION;
	float4 Color				: COLOR;
	float3 UVW					: TEXCOORD0;
};

static const float texcoordU[4] = { 0.0f, 1.0f, 0.0f, 1.0f};
static const float texcoordV[4] = { 1.0f, 1.0f, 0.0f, 0.0f};	
static const float3 upVector = {0.0f, 1.0f, 0.0f };

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
	
	float halfWidth = Input.Size.x / 2.0f;

	float3 spriteNormal;

	//The billboard will face the "Player", no matter the view vector.
	spriteNormal = Input.Position.xyz - CameraWorldPosition.xyz;
	spriteNormal.y = 0.0f; //By removing Y from the vector, we assure that the rotation can only be made around XZ axis (Don't want to see the billboard rotating in the Y axis)
	spriteNormal = normalize(spriteNormal);

	float3 rightVector = normalize(cross(spriteNormal, upVector)); //Get the vector "Right" vector = X axis vector
	rightVector = rightVector * halfWidth;			    //Apply the scalling to the vector

	// Create the billboards quad
	float3 vert[4];

	// We get the points by using the billboards right vector and the billboards height
	vert[0] = Input.Position.xyz - rightVector; // Get bottom left vertex
	vert[1] = Input.Position.xyz + rightVector; // Get bottom right vertex
	vert[2] = vert[0]; // Get top left vertex
	vert[2].y += Input.Size.y;
	vert[3] = vert[1]; // Get top right vertex
	vert[3].y += Input.Size.y;

	// *****************************************************
	// generate the 4 vertices to make two triangles
	for( uint i = 0 ; i < 4 ; i++ )
	{
		Output.Position =  mul(float4(vert[i], 1.0f), ViewProjection);
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
	float4 color = DiffuseTexture.Sample(SamplerDiffuse, IN.UVW);

	clip( color.a < 0.01f ? -1:1 ); //Remove the pixel if alpha < 0.1

	return color;
}

//--------------------------------------------------------------------------------------
