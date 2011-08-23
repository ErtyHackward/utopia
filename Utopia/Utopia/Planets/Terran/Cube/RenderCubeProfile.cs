
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Utopia.Planets.Terran.Cube;
using S33M3Engines.Struct;
using S33M3Engines.Struct.Vertex;
using Utopia.Planets.Terran.World;
using System.Data;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Landscaping;
using Utopia.Worlds.Chunks.Enums;

namespace Utopia.Planets.Terran.Cube
{
    public class RenderCubeProfile
    {
        public static RenderCubeProfile[] CubesProfile;

        //Create the various Cubes
        public static void InitCubeProfiles()
        {
            DataSet CubeProfileDS = new DataSet();
            CubeProfileDS.ReadXml(@"Models\CubesProfile.xml", XmlReadMode.Auto);

            DataTable dt = CubeProfileDS.Tables["Cube"];
            CubesProfile = new RenderCubeProfile[dt.Rows.Count];

            byte Id;
            RenderCubeProfile profile;
            string[] emissiveColor;
            foreach (DataRow cubeProfil in dt.Rows)
            {
                Id = byte.Parse(cubeProfil.ItemArray[dt.Columns["Id"].Ordinal].ToString());
                profile = new RenderCubeProfile();
                profile.Id = Id;
                profile.Name = cubeProfil.ItemArray[dt.Columns["Name"].Ordinal].ToString();
                profile.Tex_Top = byte.Parse(cubeProfil.ItemArray[dt.Columns["Tex_Top"].Ordinal].ToString());
                profile.Tex_Bottom = byte.Parse(cubeProfil.ItemArray[dt.Columns["Tex_Bottom"].Ordinal].ToString());
                profile.Tex_Front = byte.Parse(cubeProfil.ItemArray[dt.Columns["Tex_Front"].Ordinal].ToString());
                profile.Tex_Back = byte.Parse(cubeProfil.ItemArray[dt.Columns["Tex_Back"].Ordinal].ToString());
                profile.Tex_Left = byte.Parse(cubeProfil.ItemArray[dt.Columns["Tex_Left"].Ordinal].ToString());
                profile.Tex_Right = byte.Parse(cubeProfil.ItemArray[dt.Columns["Tex_Right"].Ordinal].ToString());
                profile.IsBlockingLight = cubeProfil.ItemArray[dt.Columns["IsBlockingLight"].Ordinal].ToString() == "true";
                profile.IsPickable = cubeProfil.ItemArray[dt.Columns["IsPickable"].Ordinal].ToString() == "true";
                profile.IsSolidToEntity = cubeProfil.ItemArray[dt.Columns["IsSolidToEntity"].Ordinal].ToString() == "true";
                profile.IsBlockingWater = cubeProfil.ItemArray[dt.Columns["IsBlockingWater"].Ordinal].ToString() == "true";
                profile.IsFlooding = cubeProfil.ItemArray[dt.Columns["IsFlooding"].Ordinal].ToString() == "true";
                profile.IsFloodPropagation = cubeProfil.ItemArray[dt.Columns["IsFloodPropagation"].Ordinal].ToString() == "true";
                profile.FloodingPropagationPower = cubeProfil.ItemArray[dt.Columns["FloodingPropagationPower"].Ordinal].ToString() != "" ? int.Parse(cubeProfil.ItemArray[dt.Columns["FloodingPropagationPower"].Ordinal].ToString()) : 0;

                if (cubeProfil.ItemArray[dt.Columns["IsEmissiveColorLightSource"].Ordinal].ToString() == "true")
                {
                    profile.IsEmissiveColorLightSource = true;
                    emissiveColor = cubeProfil.ItemArray[dt.Columns["EmissiveColor"].Ordinal].ToString().Split(new char[] { ',' });
                    profile.EmissiveColor = new Color(int.Parse(emissiveColor[0]), int.Parse(emissiveColor[1]), int.Parse(emissiveColor[2]), int.Parse(emissiveColor[3]));
                }

                profile.CubeFamilly = (enuCubeFamilly)Enum.Parse(typeof(enuCubeFamilly), cubeProfil.ItemArray[dt.Columns["CubeFamilly"].Ordinal].ToString());
                profile.LiquidType = cubeProfil.ItemArray[dt.Columns["LiquidType"].Ordinal].ToString() != "" ? (enuLiquidType)Enum.Parse(typeof(enuLiquidType), cubeProfil.ItemArray[dt.Columns["LiquidType"].Ordinal].ToString()) : enuLiquidType.None;

                if (profile.CubeFamilly == enuCubeFamilly.Liquid)
                {
                    profile.CanGenerateCubeFace = RenderCubeProfile.WaterFaceGenerationCheck;
                    profile.CreateLiquidCubeMesh = CubeMeshFactoryOLD.GenLiquidCubeFace;
                }
                if (profile.CubeFamilly == enuCubeFamilly.Solid)
                {
                    profile.CanGenerateCubeFace = RenderCubeProfile.FaceGenerationCheck;
                    profile.CreateSolidCubeMesh = CubeMeshFactoryOLD.GenSolidCubeFace;
                }

                CubesProfile[Id] = profile;
            }
        }

        //Default Face Generation Checks !
        public static bool FaceGenerationCheck(ref TerraCube cube, ref Location3<int> cubePosiInWorld, CubeFace cubeFace, ref TerraCube neightboorFaceCube)
        {
            //By default I don't need to trace the cubeFace of my cube if the face neightboor cube is blocking light ! (Not see-through)
            if (RenderCubeProfile.CubesProfile[neightboorFaceCube.Id].IsBlockingLight) return false;
            //Else draw the face
            return true;
        }

        public static bool WaterFaceGenerationCheck(ref TerraCube cube, ref Location3<int> cubePosiInWorld, CubeFace cubeFace, ref TerraCube neightboorFaceCube)
        {
            if (cubeFace != CubeFace.Bottom && cubeFace != CubeFace.Top) //Never display a bottom Water face !
            {
                if ((!RenderCubeProfile.CubesProfile[neightboorFaceCube.Id].IsBlockingLight && !RenderCubeProfile.CubesProfile[neightboorFaceCube.Id].IsFlooding))
                {
                    return true;
                }
            }
            if (cubeFace == CubeFace.Top)
            {
                if (cubePosiInWorld.Y == LandscapeBuilder.SeaLevel || neightboorFaceCube.Id == CubeId.Air)
                {
                    return true;
                }
            }
            return false;
        }

        public delegate bool CanGenerateCubeFaceDelegate(ref TerraCube cube, ref Location3<int> cubelocation, CubeFace cubeFace, ref TerraCube neightboorFaceCube);
        public CanGenerateCubeFaceDelegate CanGenerateCubeFace;

        public delegate void GenerateSolidMesh(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, ref List<VertexCubeSolid> cubeVertices, ref List<ushort> cubeIndices, ref Dictionary<string, int> cubeVerticeDico);
        public GenerateSolidMesh CreateSolidCubeMesh;

        public delegate void GenerateLiquidMesh(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Location3<int> cubePosiInWorld, ref List<VertexCubeLiquid> cubeVertices, ref List<ushort> cubeIndices, ref Dictionary<string, int> cubeVerticeDico);
        public GenerateLiquidMesh CreateLiquidCubeMesh;

        public string Name;
        public byte Id; //Represent the ID of the cube and it's linked texture in the array
        public bool IsPickable;
        public bool IsBlockingLight;
        public bool IsBlockingWater;
        public bool IsFloodPropagation;
        public bool IsSolidToEntity;
        public bool IsEmissiveColorLightSource;
        public bool IsFlooding;
        public int FloodingPropagationPower;
        public Color EmissiveColor;
        public enuCubeFamilly CubeFamilly;
        public enuLiquidType LiquidType;
        //Texture id foreach face
        public byte Tex_Front, Tex_Back, Tex_Left, Tex_Right, Tex_Top, Tex_Bottom;
    }

}

