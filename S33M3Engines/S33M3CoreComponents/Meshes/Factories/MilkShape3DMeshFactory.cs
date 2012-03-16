using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using System.Globalization;
using S33M3CoreComponents.Meshes.Factories.Helpers;

namespace S33M3CoreComponents.Meshes.Factories
{
    public class MilkShape3DMeshFactory : IMeshFactory
    {
        #region Inner Struct Class
        public struct MS3Vertex
        {
            public int MeshId;
            public float X;
            public float Y;
            public float Z;
            public float U;
            public float V;
            public int BoneIndex;
            public int MaterialId;
        }

        public struct MS3Normal
        {
            public float X;
            public float Y;
            public float Z;
        }

        public struct MS3Material
        {
            public int Id;
            public string Name;
            public Color4 Ambiant;
            public Color4 Diffuse;
            public Color4 Specular;
            public Color4 Emissive;
            public float shininess;
            public float transparency;
            public string ColorTexturePath;
            public string AlphaTexturePath;
        }

        #endregion

        #region Private variable
        private StringScanner _strScan;
        #endregion

        #region Public variables
        #endregion
        public MilkShape3DMeshFactory()
        {
            _strScan = new StringScanner();
        }

        #region Private Methods
        private bool GenerateMesh(string Path, out Mesh mesh, int indiceOffset)
        {
            string strParam = string.Empty;
            int intParam = -1;
            float floatParam = -1;

            // number of meshes => Meshes: 1
            var MeshSection_Holder = _strScan.RefParamHolder(strParam, intParam);
            // mesh: name, flags, material index => "Box01" 0 0
            var Mesh_Holder = _strScan.RefParamHolder(strParam, intParam, intParam);
            // number of vertices => 20
            var NbrVertices_Holder = _strScan.RefParamHolder(intParam);
            // vertex: flags, x, y, z, u, v, bone index => 0 -10.375000 10.500000 13.750000 0.000000 0.000000 -1
            var Vertices_Holder = _strScan.RefParamHolder(intParam, floatParam, floatParam, floatParam, floatParam, floatParam, intParam);
            // number of normals => 6
            var NbrNormals_Holder = _strScan.RefParamHolder(intParam);
            // normal: x, y, z => 0.000000 0.000000 1.000000
            var Normals_Holder = _strScan.RefParamHolder(floatParam, floatParam, floatParam);
            // number of triangles => 12
            var NbrTriangles_Holder = _strScan.RefParamHolder(intParam);
            // triangle: flags, vertex index1, vertex index2, vertex index3, normal index1, normal index 2, normal index 3, smoothing group => 0 0 1 2 0 0 0 1
            var Triangles_Holder = _strScan.RefParamHolder(intParam, intParam, intParam, intParam, intParam, intParam, intParam, intParam);

            List<MS3Vertex> sourceVertices = new List<MS3Vertex>(100);
            List<MS3Normal> sourceNormals = new List<MS3Normal>(100);

            Dictionary<long, ushort> generatedVertices = new Dictionary<long, ushort>();
            List<VertexMesh> vertices = new List<VertexMesh>(100);
            List<ushort> indices = new List<ushort>(150);

            List<MS3Material> Materials = GetModelMaterials(Path);

            mesh = new Mesh();
            try
            {
                using (StreamReader sr = new StreamReader(Path))
                {
                    string line;
                    // Read and display lines from the file until the end of 
                    // the file is reached.

                    //Read File Header to validated the format
                    line = sr.ReadLine();
                    if (line != "// MilkShape 3D ASCII") return false;

                    //Go to the MeshSection
                    while ((line = sr.ReadLine()).Contains("Meshes:") == false) ;

                    _strScan.Scan(line, "{0} {1}", MeshSection_Holder);

                    //For each Meshes in the Files
                    for (int nbrMesh = 0; nbrMesh < (int)MeshSection_Holder[1]; nbrMesh++)
                    {
                        sourceVertices.Clear();
                        sourceNormals.Clear();
                        generatedVertices.Clear();

                        // mesh: name, flags, material index
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0} {1} {2}", Mesh_Holder);
                        Mesh_Holder[0] = ((string)Mesh_Holder[0]).Replace("\"", "");    //Mesh Name
                        // Vertex reading ==================================================
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0}", NbrVertices_Holder);
                        for (int vertice = 0; vertice < (int)NbrVertices_Holder[0]; vertice++)
                        {
                            // vertex: flags, x, y, z, u, v, bone index
                            line = sr.ReadLine();
                            _strScan.Scan(line, "{0} {1} {2} {3} {4} {5} {6}", Vertices_Holder);
                            sourceVertices.Add(new MS3Vertex()
                            {
                                MeshId = (int)Vertices_Holder[0],
                                X = (float)Vertices_Holder[1],
                                Y = (float)Vertices_Holder[2],
                                Z = (float)Vertices_Holder[3] * -1,   // Inverted because MilkShape is Right handed, DirectX left handed
                                U = (float)Vertices_Holder[4],
                                V = (float)Vertices_Holder[5],
                                BoneIndex = (int)Vertices_Holder[6],
                                MaterialId = (int)Mesh_Holder[2]
                            });
                        }

                        // Normals reading ==================================================
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0}", NbrNormals_Holder);
                        for (int normal = 0; normal < (int)NbrNormals_Holder[0]; normal++)
                        {
                            // vertex: flags, x, y, z, u, v, bone index
                            line = sr.ReadLine();
                            _strScan.Scan(line, "{0} {1} {2}", Normals_Holder);
                            sourceNormals.Add(new MS3Normal()
                            {
                                X = (float)Normals_Holder[0],
                                Y = (float)Normals_Holder[1],
                                Z = (float)Normals_Holder[2] * -1, // Inverted because MilkShape is Right handed, DirectX left handed
                            });
                        }

                        // Triangle reading ==================================================
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0}", NbrTriangles_Holder);
                        for (int triangle = 0; triangle < (int)NbrTriangles_Holder[0]; triangle++)
                        {
                            // vertex: flags, x, y, z, u, v, bone index
                            line = sr.ReadLine();
                            _strScan.Scan(line, "{0} {1} {2} {3} {4} {5} {6} {7}", Triangles_Holder);
                            int materialId = (int)Mesh_Holder[2];
                            int materialModelIndex = 0;
                            if (materialId >= 0)
                            {
                                materialModelIndex = Materials[materialId].Id;
                            }

                            //Check these 3 vertex if already generated ?
                            AddNewVertex((int)Triangles_Holder[1], (int)Triangles_Holder[4], generatedVertices, sourceVertices, sourceNormals, vertices, indices, indiceOffset, materialModelIndex);
                            AddNewVertex((int)Triangles_Holder[3], (int)Triangles_Holder[6], generatedVertices, sourceVertices, sourceNormals, vertices, indices, indiceOffset, materialModelIndex);
                            AddNewVertex((int)Triangles_Holder[2], (int)Triangles_Holder[5], generatedVertices, sourceVertices, sourceNormals, vertices, indices, indiceOffset, materialModelIndex);
                        }
                    }
                }

                mesh.Name = (string)Mesh_Holder[0];
                mesh.Vertices = vertices.ToArray();
                mesh.Indices = indices.ToArray();

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private List<MS3Material> GetModelMaterials(string Path)
        {
            string strParam = string.Empty;
            int intParam = -1;
            float floatParam = -1;

            // number of Materials => Materials: 6
            var MaterialsNbr_Holder = _strScan.RefParamHolder(strParam, intParam);
            // number of Materials => "Top"
            var MaterialsName_Holder = _strScan.RefParamHolder(strParam);
            // color data of Materials => 0.200000 0.200000 0.200000 1.000000
            var MaterialsColorData_Holder = _strScan.RefParamHolder(floatParam, floatParam, floatParam, floatParam);
            // coef data of Materials => 0.200000
            var MaterialsColorCoef_Holder = _strScan.RefParamHolder(floatParam);
            // texture path of Materials => "ct00.png"
            var MaterialsTextureFilePath_Holder = _strScan.RefParamHolder(strParam);

            List<MS3Material> Materials = new List<MS3Material>();
            string line;

            try
            {
                using (StreamReader sr = new StreamReader(Path))
                {
                    //Move to "Material file section"
                    while ((line = sr.ReadLine()).Contains("Materials:") == false) ;
                    _strScan.Scan(line, "{0} {1}", MaterialsNbr_Holder);

                    for (int materialId = 0; materialId < (int)MaterialsNbr_Holder[1]; materialId++)
                    {
                        MS3Material newMaterial = new MS3Material();
                        //Read Material name
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0}", MaterialsName_Holder);
                        newMaterial.Name = ((string)MaterialsName_Holder[0]).Replace("\"", "");
                        newMaterial.Id = int.Parse(newMaterial.Name.Split('_')[0], CultureInfo.InvariantCulture);

                        //Read ambient color
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0} {1} {2} {3}", MaterialsColorData_Holder);
                        newMaterial.Ambiant = new Color4((float)MaterialsColorData_Holder[0], (float)MaterialsColorData_Holder[1], (float)MaterialsColorData_Holder[2], (float)MaterialsColorData_Holder[3]);

                        //Read diffuse color
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0} {1} {2} {3}", MaterialsColorData_Holder);
                        newMaterial.Diffuse = new Color4((float)MaterialsColorData_Holder[0], (float)MaterialsColorData_Holder[1], (float)MaterialsColorData_Holder[2], (float)MaterialsColorData_Holder[3]);

                        //Read specular color
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0} {1} {2} {3}", MaterialsColorData_Holder);
                        newMaterial.Specular = new Color4((float)MaterialsColorData_Holder[0], (float)MaterialsColorData_Holder[1], (float)MaterialsColorData_Holder[2], (float)MaterialsColorData_Holder[3]);

                        //Read specular color
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0} {1} {2} {3}", MaterialsColorData_Holder);
                        newMaterial.Emissive = new Color4((float)MaterialsColorData_Holder[0], (float)MaterialsColorData_Holder[1], (float)MaterialsColorData_Holder[2], (float)MaterialsColorData_Holder[3]);

                        //Read Shininess coef (0 => 255 float)
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0}", MaterialsColorCoef_Holder);
                        newMaterial.shininess = (float)MaterialsColorCoef_Holder[0];

                        //Read Transparency coef (0 => 1 float)
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0}", MaterialsColorCoef_Holder);
                        newMaterial.transparency = (float)MaterialsColorCoef_Holder[0];

                        //Read Color texture Path
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0}", MaterialsTextureFilePath_Holder);
                        newMaterial.ColorTexturePath = ((string)MaterialsTextureFilePath_Holder[0]).Replace("\"", "");
                        //Read Alpha texture path
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0}", MaterialsTextureFilePath_Holder);
                        newMaterial.AlphaTexturePath = ((string)MaterialsTextureFilePath_Holder[0]).Replace("\"", "");

                        Materials.Add(newMaterial);
                    }
                }
            }
            catch (Exception)
            {
            }

            return Materials;
        }

        private void AddNewVertex(int vertexID,
                                  int normalID,
                                  Dictionary<long, ushort> generatedVertices,
                                  List<MS3Vertex> sourceVertices,
                                  List<MS3Normal> sourceNormals,
                                  List<VertexMesh> vertices,
                                  List<ushort> indices,
                                  int indiceOffset,
                                  int TextureArrayIndex)
        {
            long ID = (((Int64)(normalID) << 32) + (vertexID << 16) + TextureArrayIndex);

            ushort vertexIndice;
            if (!generatedVertices.TryGetValue(ID, out vertexIndice))
            {
                //Not existing, create it.
                VertexMesh vertex = new VertexMesh()
                {
                    Normal = new Vector3(sourceNormals[normalID].X, sourceNormals[normalID].Y, sourceNormals[normalID].Z),
                    Position = new Vector3(sourceVertices[vertexID].X, sourceVertices[vertexID].Y, sourceVertices[vertexID].Z),
                    TextureCoordinate = new Vector3(sourceVertices[vertexID].U, sourceVertices[vertexID].V, TextureArrayIndex)
                };
                vertices.Add(vertex);
                vertexIndice = (ushort)(vertices.Count - 1);
                generatedVertices.Add(ID, vertexIndice);
            }
            indices.Add((ushort)(vertexIndice + indiceOffset));
        }
        #endregion

        #region Public Methods
        public bool LoadMesh(string Path, out Mesh mesh, int indiceOffset)
        {
            if (Path.Substring(0, 1) == @"\")
            {
                Path = Environment.CurrentDirectory + Path;
            }
            if (!File.Exists(Path))
            {
                throw new Exception("Mesh data file not found : " + Path);
            }

            return GenerateMesh(Path, out mesh, indiceOffset);
        }
        #endregion
    }
}
