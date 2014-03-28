//--------------------------------------------------------------------------------------
// Shared Frame Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer PerFrame
{
	matrix ViewProjection_focused; //Focused (World 0;0;0 = Player Location) view projection matrix
	float3 SunColor;			   //Diffuse lighting color, global light emmited by sun
	float fogdist;                 //The Fog distance 
	float2 BackBufferSize;         //The BackBuffer size
	float2 Various;                //x = 1 if head under water
	matrix ViewProjection;         //Normal ViewProjection
	float FogType;                 //Fog Type : 0 skyfog, 1 SimpleFog, 2 NoFog
	float3 CameraWorldPosition;    //The camera in world position coordinate
	matrix InvertedOrientation;	   //The inverted Matrix view
	float2 WeatherGlobalOffset;	   //Global Offset assigned to temp for texture sampling
	float TextureFrameAnimation;    //An animation counter for texture animated texture
};

