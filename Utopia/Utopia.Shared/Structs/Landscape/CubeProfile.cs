using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Utopia.Shared.Structs.Landscape
{
    /// <summary>
    /// Class that will hold the collection of Cube profiles.
    /// A cube profile contains the informations for a cube type that cannot change.
    /// It needs to be initialized (InitCubeProfiles) !
    /// Client is using a more complete profile collection (CubeProfile) with graphical informations, like Texture mapping, but also delegates that can only be called from the Client side
    /// </summary>
    public class CubeProfileOLD
    {
        public static CubeProfileOLD[] CubesProfile;

        //Create the various Cubes
        public static void InitCubeProfiles(string cubeProfilePath)
        {
            DataSet CubeProfileDS = new DataSet();
            CubeProfileDS.ReadXml(cubeProfilePath, XmlReadMode.Auto);

            DataTable dt = CubeProfileDS.Tables["Cube"];
            CubesProfile = new CubeProfileOLD[dt.Rows.Count];

            ushort Id;
            CubeProfileOLD profile;
            string[] emissiveColor;
            foreach (DataRow cubeProfil in dt.Rows)
            {
                Id = ushort.Parse(cubeProfil.ItemArray[dt.Columns["Id"].Ordinal].ToString());
                profile = new CubeProfileOLD();
                profile.Id = Id;
                profile.Name = cubeProfil.ItemArray[dt.Columns["Name"].Ordinal].ToString();
                profile.IsBlockingLight = cubeProfil.ItemArray[dt.Columns["IsBlockingLight"].Ordinal].ToString() == "true";
                profile.IsPickable = cubeProfil.ItemArray[dt.Columns["IsPickable"].Ordinal].ToString() == "true";
                profile.IsSolidToEntity = cubeProfil.ItemArray[dt.Columns["IsSolidToEntity"].Ordinal].ToString() == "true";
                profile.IsBlockingWater = cubeProfil.ItemArray[dt.Columns["IsBlockingWater"].Ordinal].ToString() == "true";
                profile.IsFlooding = cubeProfil.ItemArray[dt.Columns["IsFlooding"].Ordinal].ToString() == "true";
                profile.IsFloodPropagation = cubeProfil.ItemArray[dt.Columns["IsFloodPropagation"].Ordinal].ToString() == "true";
                profile.FloodingPropagationPower = cubeProfil.ItemArray[dt.Columns["FloodingPropagationPower"].Ordinal].ToString() != "" ? int.Parse(cubeProfil.ItemArray[dt.Columns["FloodingPropagationPower"].Ordinal].ToString()) : 0;
                profile.YBlockOffset = cubeProfil.ItemArray[dt.Columns["YBlockOffset"].Ordinal].ToString() != "" ? byte.Parse(cubeProfil.ItemArray[dt.Columns["YBlockOffset"].Ordinal].ToString())/255.0f : 0.0f;

                if (cubeProfil.ItemArray[dt.Columns["IsEmissiveColorLightSource"].Ordinal].ToString() == "true")
                {
                    profile.IsEmissiveColorLightSource = true;
                    emissiveColor = cubeProfil.ItemArray[dt.Columns["EmissiveColor"].Ordinal].ToString().Split(new char[] { ',' });
                    profile.EmissiveColor = new Color(int.Parse(emissiveColor[0]), int.Parse(emissiveColor[1]), int.Parse(emissiveColor[2]), int.Parse(emissiveColor[3]));
                }

                CubesProfile[Id] = profile;
            }
        }

        public string Name;
        public ushort Id; //Represent the ID of the cube and it's linked texture in the array
        public bool IsPickable;
        public bool IsBlockingLight;
        public bool IsBlockingWater;
        public bool IsFloodPropagation;
        public bool IsSolidToEntity;
        public bool IsEmissiveColorLightSource;
        public bool IsFlooding;
        public int FloodingPropagationPower;
        public float YBlockOffset;
        public Color EmissiveColor;
    }
}
