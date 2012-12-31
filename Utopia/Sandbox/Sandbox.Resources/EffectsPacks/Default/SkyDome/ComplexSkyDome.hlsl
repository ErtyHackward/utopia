//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer VSGlobal
{
	matrix WorldViewProj;
	matrix World;
	float3 CameraPosition_world;
	bool isSkydome;
	float3 LightDirection = {100.0f, 100.0f, 100.0f};
}

cbuffer GlobelFixeVariable
{
	float4 LightColor = {1.0f, 1.0f, 1.0f, 1.0f};
	float4 LightColorAmbient = {0.0f, 0.0f, 0.0f, 1.0f};
	float4 FogColor = {1.0f, 1.0f, 1.0f, 1.0f};
	float fDensity = 0.000028f;
	float SunLightness = 0.2; 
	float sunRadiusAttenuation = 256;
	float largeSunLightness = 0.2;
	float largeSunRadiusAttenuation = 1;
	float dayToSunsetSharpness = 1.5;
	float hazeTopAltitude = 100; 
}

//--------------------------------------------------------------------------------------
// Texture Samplers
//--------------------------------------------------------------------------------------
Texture2D DiffuseTexture;
SamplerState MirrorSampler
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap ; 
	AddressV = Wrap ;
};

Texture2D SkyTextureNight;
Texture2D SkyTextureDay;
Texture2D SkyTextureSunset;
SamplerState WrapSampler
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = mirror; 
	AddressV = mirror;
};

//--------------------------------------------------------------------------------------
//Vertex shader Input
struct appdata {
	float4 Position				: POSITION;
	float4 Normal				: NORMAL;
	float2 UV0					: TEXCOORD0;
};

//Pixel shader Input
struct vertexOutput {
	float4 HPosition	 		: SV_POSITION;
	float3 WorldLightVec		: TEXCOORD0;
	float3 WorldNormal	    	: TEXCOORD1;
	float3 WorldEyeDirection	: TEXCOORD2;
  	float2 UV					: TEXCOORD5;
  	half Fog 					: TEXCOORD6;
  	half2 Altitudes 			: TEXCOORD7; 
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//-------------------------------------------------------------------------------------
//[VS ENTRY POINT]
vertexOutput mainVS (appdata IN)
{
	vertexOutput OUT;

		// Je fais passer la position de mon vertex de l'object à l'écran (Object => World => View (Camera) => Screen (Projection));
		// Object = La position du vertex est relative à un point 0,0 correspondant à la mesh a laquel appartient le point
		// World = La position du vertex est relative à un point 0,0 appartenent à un espace 3d appelé "Monde" dans lequel appartiennent tous les objects à afficher
		// View = La position du vertex est relative à un point 0,0 qui correspond a la position d'un poitn de vue dans le Monde (= Caméra)
		// Screen = Faire passer le vertex d'un monde 3D à un monde 2D (L'écran) grace à une projection.
	float4 Po = float4(IN.Position.xyz,1);
	OUT.HPosition = mul(Po, WorldViewProj);
	
		// Je dois transformer les vecteurs de mon vertex en coordonée de monde (Et pas gardée la coordée du mmodel)
		// Pour cela il suffi de multiplier mes vecteur par l'inverse transposée de ma matrice monde = worldIT (qu'il faudra ensuite normaliser)
	OUT.WorldNormal = mul(IN.Normal, World).xyz;                                  

		// Direction de la lumière défini par un vecteur (Déjà en coord monde)
	OUT.WorldLightVec = -LightDirection; 

		// Calcul le vecteur entre ma caméra et le vertex a peindre
	float3 Pw = mul(Po, World).xyz;
	OUT.WorldEyeDirection = CameraPosition_world.xyz - Pw;
	
		// OUT.Altitudes.x = Altitude Caméra dans le monde
		// OUT.Altitudes.y = Altitude vertex dans le monde
	OUT.Altitudes.x = CameraPosition_world.y;	
	OUT.Altitudes.y = Pw.y;
	
		//Calcul d'un brouillard en fct de la distance entre ma caméra et le vertex
 	float dist = length(OUT.WorldEyeDirection);
	OUT.Fog = (1.f/exp(pow(dist * fDensity, 2))); // Pas convaincu car pour une densité fixe de 0.000028f fog, quelque soit la distance dist * fDensity = +/- 0, au carré, ca fait tjs 0, et Exp(0) = 1.

	OUT.UV = IN.UV0;
	return OUT;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
//[PS ENTRY POINT]
float4 mainPS(vertexOutput IN) : SV_Target
{	
	float4 colorOutput = float4(0,0,0,1);
	float4 DiffuseColor = DiffuseTexture.Sample(MirrorSampler, float2( IN.UV.x, 1-IN.UV.y));  

	// Calculate light/eye/normal vectors
	float eyeAlt = IN.Altitudes.x;
	float3 eyeVec = normalize(IN.WorldEyeDirection);
	float3 normal = normalize(IN.WorldNormal);
	float3 lightVec = normalize(IN.WorldLightVec);

	// Calculate the amount of direct light	
	float NdotL = max(dot(normal,-lightVec), 0);
	
	float4 colorDiffuse = DiffuseColor * (NdotL * LightColor) + LightColorAmbient * DiffuseColor;
	colorOutput += colorDiffuse;		
	colorOutput.a = 1.0f;
	
	// Calculate sun highlight...	
	// Peut être interpreté comme : SI je regarde en face du soleil ==> Maximum sunHighLigh !! (dot = 1)
	float sunHighlight = pow(max(0.000001f, dot(lightVec, -eyeVec)), sunRadiusAttenuation) * SunLightness;	
	// Calculate a wider sun highlight 
	float largeSunHighlight = pow(max(0.000001f, dot(lightVec, -eyeVec)), largeSunRadiusAttenuation) * largeSunLightness;
	
	// Calculate 2D angle between pixel to eye and sun to eye
	float3 flatLightVec = normalize(float3(lightVec.x, 0, lightVec.z));
	float3 flatEyeVec = normalize(float3(eyeVec.x, 0, eyeVec.z));
	float diff = dot(flatLightVec, -flatEyeVec);	
	
	// Based on camera altitude, the haze will look different and will be lower on the horizon.
	// This is simulated by raising YAngle to a certain power based on the difference between the
	// haze top and camera altitude. 
	// This modification of the angle will show more blue sky above the haze with a sharper separation.
	// Lerp between 0.25 and 1.25
	float val = lerp(0.25, 1, min(1, hazeTopAltitude / max(0.0001, eyeAlt)));
	// Apply the power to sharpen the edge between haze and blue sky
	float YAngle = pow(max(0.000001f, -eyeVec.y), val);	

	// Fetch the 3 colors we need based on YAngle and angle from eye vector to the sun
	// Les texture sont créé de sorte plus on monte sur la texture (Axe Y) moins on a de "brouillard", X sur les texture représente la couleur en fct du temps
	float4 fogColorDay =  SkyTextureDay.Sample(WrapSampler, float2( 1 - (diff + 1) * 0.5, 1-YAngle));			
	float4 fogColorSunset = SkyTextureSunset.Sample(WrapSampler,float2( 1 - (diff + 1) * 0.5, 1-YAngle));   
	float4 fogColorNight = SkyTextureNight.Sample(WrapSampler, float2( 1 - (diff + 1) * 0.5, 1-YAngle));    
	
	float4 fogColor;
	
	// If the light is above the horizon, then interpolate between day and sunset
	// Otherwise between sunset and night
	if (lightVec.y > 0)
	{
		// Transition is sharpened with dayToSunsetSharpness to make a more realistic cut 
		// between day and sunset instead of a linear transition
		fogColor = lerp(fogColorDay, fogColorSunset, min(1, pow(abs(1 - lightVec.y), dayToSunsetSharpness)));
	}
	else
	{
		// Slightly different scheme for sunset/night.
		fogColor = lerp(fogColorSunset, fogColorNight, min(1, -lightVec.y * 4));
	}
	
	// Add sun highlights only if SkyDome !
	if(isSkydome)
		fogColor += sunHighlight + largeSunHighlight;
    
	// Apply fog on output color
	colorOutput = lerp(fogColor, colorOutput, IN.Fog);
	
	// Make sun brighter for the skybox...
	if (isSkydome)
		colorOutput = fogColor + sunHighlight;

	if (isSkydome && lightVec.y < 0)
	{
		//colorOutput = lerp(colorOutput, DiffuseColor, -lightVec.y);
	}

	return colorOutput;
}

