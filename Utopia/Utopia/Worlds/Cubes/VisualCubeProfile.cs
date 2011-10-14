using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Utopia.Worlds.Chunks.Enums;
using Utopia.Shared.Structs;
using S33M3Engines.Struct.Vertex;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared.Cubes;

namespace Utopia.Worlds.Cubes
{

    public class VisualCubeProfile
    {
        public static VisualCubeProfile[] CubesProfile;

        //Create the various Cubes
        public static void InitCubeProfiles(ICubeMeshFactory solidCubeMeshFactory, ICubeMeshFactory liquidCubeMeshFactory, string FilePath)
        {
            DataSet CubeProfileDS = new DataSet();
            CubeProfileDS.ReadXml(FilePath, XmlReadMode.Auto);

            DataTable dt = CubeProfileDS.Tables["Cube"];
            CubesProfile = new VisualCubeProfile[dt.Rows.Count];

            byte Id;
            VisualCubeProfile profile;
            string[] emissiveColor;
            foreach (DataRow cubeProfil in dt.Rows)
            {
                Id = byte.Parse(cubeProfil.ItemArray[dt.Columns["Id"].Ordinal].ToString());
                profile = new VisualCubeProfile();
                profile.Id = Id;
                profile.Name = cubeProfil.ItemArray[dt.Columns["Name"].Ordinal].ToString();
                profile.Tex_Top = byte.Parse(cubeProfil.ItemArray[dt.Columns["Tex_Top"].Ordinal].ToString());
                profile.Tex_Bottom = byte.Parse(cubeProfil.ItemArray[dt.Columns["Tex_Bottom"].Ordinal].ToString());
                profile.Tex_Front = byte.Parse(cubeProfil.ItemArray[dt.Columns["Tex_Front"].Ordinal].ToString());
                profile.Tex_Back = byte.Parse(cubeProfil.ItemArray[dt.Columns["Tex_Back"].Ordinal].ToString());
                profile.Tex_Left = byte.Parse(cubeProfil.ItemArray[dt.Columns["Tex_Left"].Ordinal].ToString());
                profile.Tex_Right = byte.Parse(cubeProfil.ItemArray[dt.Columns["Tex_Right"].Ordinal].ToString());

                profile.Textures[(int)CubeFace.Back] = profile.Tex_Back;
                profile.Textures[(int)CubeFace.Front] = profile.Tex_Front;
                profile.Textures[(int)CubeFace.Bottom] = profile.Tex_Bottom;
                profile.Textures[(int)CubeFace.Top] = profile.Tex_Top;
                profile.Textures[(int)CubeFace.Left] = profile.Tex_Left;
                profile.Textures[(int)CubeFace.Right] = profile.Tex_Right;

                profile.IsBlockingLight = cubeProfil.ItemArray[dt.Columns["IsBlockingLight"].Ordinal].ToString() == "true";
                profile.IsSeeThrough = cubeProfil.ItemArray[dt.Columns["IsSeeThrough"].Ordinal].ToString() == "true";
                profile.IsPickable = cubeProfil.ItemArray[dt.Columns["IsPickable"].Ordinal].ToString() == "true";
                profile.IsSolidToEntity = cubeProfil.ItemArray[dt.Columns["IsSolidToEntity"].Ordinal].ToString() == "true";
                profile.IsBlockingWater = cubeProfil.ItemArray[dt.Columns["IsBlockingWater"].Ordinal].ToString() == "true";
                profile.IsFlooding = cubeProfil.ItemArray[dt.Columns["IsFlooding"].Ordinal].ToString() == "true";
                profile.IsFloodPropagation = cubeProfil.ItemArray[dt.Columns["IsFloodPropagation"].Ordinal].ToString() == "true";
                profile.FloodingPropagationPower = cubeProfil.ItemArray[dt.Columns["FloodingPropagationPower"].Ordinal].ToString() != "" ? int.Parse(cubeProfil.ItemArray[dt.Columns["FloodingPropagationPower"].Ordinal].ToString()) : 0;
                profile.YBlockOffset = cubeProfil.ItemArray[dt.Columns["YBlockOffset"].Ordinal].ToString() != "" ? byte.Parse(cubeProfil.ItemArray[dt.Columns["YBlockOffset"].Ordinal].ToString()) : (byte)0;

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
                    profile.CanGenerateCubeFace = VisualCubeProfile.WaterFaceGenerationCheck;
                    profile.CreateLiquidCubeMesh = liquidCubeMeshFactory.GenCubeFace;
                }

                if (profile.CubeFamilly == enuCubeFamilly.Solid)
                {
                    profile.CanGenerateCubeFace = VisualCubeProfile.FaceGenerationCheck;
                    profile.CreateSolidCubeMesh = solidCubeMeshFactory.GenCubeFace;
                }

                CubesProfile[Id] = profile;
            }
        }

        public static void CleanUp()
        {
            CubesProfile = null;
        }

        //Default Face Generation Checks !
        public static bool FaceGenerationCheck(ref TerraCube cube, ref Vector3I cubePosiInWorld, CubeFace cubeFace, ref TerraCube neightboorFaceCube, int seaLevel)
        {
            //By default I don't need to trace the cubeFace of my cube if the face neightboor cube is blocking light ! (Not see-through)
            if (VisualCubeProfile.CubesProfile[neightboorFaceCube.Id].IsSeeThrough) return true;
            //Else draw the face
            return false;
        }

        public static bool WaterFaceGenerationCheck(ref TerraCube cube, ref Vector3I cubePosiInWorld, CubeFace cubeFace, ref TerraCube neightboorFaceCube, int seaLevel)
        {
            if (cubeFace != CubeFace.Bottom && cubeFace != CubeFace.Top) //Never display a bottom Water face !
            {
                if ((!VisualCubeProfile.CubesProfile[neightboorFaceCube.Id].IsBlockingLight && !VisualCubeProfile.CubesProfile[neightboorFaceCube.Id].IsFlooding))
                {
                    return true;
                }
            }
            if (cubeFace == CubeFace.Top)
            {
                if (cubePosiInWorld.Y == seaLevel || neightboorFaceCube.Id == CubeId.Air)
                {
                    return true;
                }
            }
            return false;
        }

        public delegate bool CanGenerateCubeFaceDelegate(ref TerraCube cube, ref Vector3I cubelocation, CubeFace cubeFace, ref TerraCube neightboorFaceCube, int seaLevel);
        public CanGenerateCubeFaceDelegate CanGenerateCubeFace;

        public delegate void GenerateSolidMesh(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Vector3I cubePosiInWorld, VisualChunk chunk);
        public GenerateSolidMesh CreateSolidCubeMesh;

        public delegate void GenerateLiquidMesh(ref TerraCube cube, CubeFace cubeFace, ref ByteVector4 cubePosition, ref Vector3I cubePosiInWorld, VisualChunk chunk);
        public GenerateLiquidMesh CreateLiquidCubeMesh;

        public string Name;
        public byte Id; //Represent the ID of the cube and it's linked texture in the array
        public bool IsPickable;
        public bool IsBlockingLight;
        public bool IsSeeThrough;
        public bool IsBlockingWater;
        public bool IsFloodPropagation;
        public bool IsSolidToEntity;
        public bool IsEmissiveColorLightSource;
        public bool IsFlooding;
        public int FloodingPropagationPower;
        public Color EmissiveColor;
        public enuCubeFamilly CubeFamilly;
        public enuLiquidType LiquidType;
        public byte YBlockOffset;
        //Texture id foreach face
        public byte Tex_Front, Tex_Back, Tex_Left, Tex_Right, Tex_Top, Tex_Bottom;
        public byte[] Textures = new byte[6];
    }
}
