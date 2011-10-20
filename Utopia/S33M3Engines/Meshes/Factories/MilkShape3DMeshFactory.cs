using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using S33M3Engines.Meshes.Factories.Helpers;
using S33M3Engines.Struct.Vertex;
using SharpDX;

namespace S33M3Engines.Meshes.Factories
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
        }

        public struct MS3Normals
        {
            public float X;
            public float Y;
            public float Z;
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
            List<MS3Normals> sourceNormals = new List<MS3Normals>(100);

            Dictionary<long, ushort> generatedVertices = new Dictionary<long, ushort>();
            List<VertexMesh> vertices = new List<VertexMesh>(100);
            List<ushort> indices = new List<ushort>(150);

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

                    //For each Meshes in the Files -- Limited to the first one here
                    for (int nbrMesh = 0; nbrMesh < 1; nbrMesh++)
                    {
                        // mesh: name, flags, material index
                        line = sr.ReadLine();
                        _strScan.Scan(line, "{0} {1} {2}", Mesh_Holder);
                        Mesh_Holder[0] = ((string)Mesh_Holder[0]).Replace("\"", "");
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
                                Z = (float)Vertices_Holder[3],
                                U = (float)Vertices_Holder[4],
                                V = (float)Vertices_Holder[5],
                                BoneIndex = (int)Vertices_Holder[6]
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
                            sourceNormals.Add(new MS3Normals()
                            {
                                X = (float)Normals_Holder[0],
                                Y = (float)Normals_Holder[1],
                                Z = (float)Normals_Holder[2],
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
                            
                            //Check these 3 vertex if already generated ?
                            AddNewVertex((int)Triangles_Holder[1], (int)Triangles_Holder[4], generatedVertices, sourceVertices, sourceNormals, vertices, indices, indiceOffset);
                            AddNewVertex((int)Triangles_Holder[2], (int)Triangles_Holder[5], generatedVertices, sourceVertices, sourceNormals, vertices, indices, indiceOffset);
                            AddNewVertex((int)Triangles_Holder[3], (int)Triangles_Holder[6], generatedVertices, sourceVertices, sourceNormals, vertices, indices, indiceOffset);
                        }

                    }

                }

                mesh.Name = (string)Mesh_Holder[0];
                mesh.Vertices = vertices.ToArray();
                mesh.Indices = indices.ToArray();

            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        private void AddNewVertex(int triangleID, 
                                  int normalID, 
                                  Dictionary<long, ushort> generatedVertices, 
                                  List<MS3Vertex> sourceVertices,
                                  List<MS3Normals> sourceNormals,
                                  List<VertexMesh> vertices,
                                  List<ushort> indices,
                                  int indiceOffset)
        {
            long vertexID = (((Int64)(triangleID) << 32) + (int)normalID);

            ushort vertexIndice;
            if (!generatedVertices.TryGetValue(vertexID, out vertexIndice))
            {
                //Not existing, create it.
                VertexMesh vertex = new VertexMesh()
                {
                    Normal = new Vector3(sourceNormals[normalID].X, sourceNormals[normalID].Y, sourceNormals[normalID].Z),
                    Position = new Vector3(sourceVertices[triangleID].X, sourceVertices[triangleID].Y, sourceVertices[triangleID].Z),
                    TextureCoordinate = new Vector2(sourceVertices[triangleID].U, sourceVertices[triangleID].V)
                };
                vertices.Add(vertex);
                vertexIndice = (ushort)(vertices.Count - 1);
                generatedVertices.Add(vertexID, vertexIndice);
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
