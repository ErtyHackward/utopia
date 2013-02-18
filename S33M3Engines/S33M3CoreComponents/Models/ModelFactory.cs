using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assimp;
using S33M3CoreComponents.Models.ModelMesh;
using S33M3DXEngine.Buffers;
using S33M3Resources.VertexFormats.Interfaces;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace S33M3CoreComponents.Models
{
    public static class ModelFactory
    {
        #region Private Variables
        #endregion

        #region Public Properties
        #endregion

        #region Public Methods
        public static IModel GenerateModel<VertexFormat, IndexFormat>(string ModelFilePath, string modelName, Device device)
            where VertexFormat : IModelMeshComponents, new()
            where IndexFormat : struct
        {
            if (ModelFilePath.Substring(0, 1) == @"\")
            {
                ModelFilePath = Environment.CurrentDirectory + ModelFilePath;
            }

            //Check file existance
            FileInfo fi = new FileInfo(ModelFilePath);
            if (fi.Exists == false) return null;        //File is not existing, return nothing

            IModel model = LoadModelFromAssimp<VertexFormat, IndexFormat>(fi, modelName, device);

            return model;
        }
        #endregion

        #region Private Methods
        private static IModel LoadModelFromAssimp<VertexFormat, IndexFormat>(FileInfo fi, string modelName, Device device)
            where VertexFormat : IModelMeshComponents, new()
            where IndexFormat : struct
        {
            //Get Assimp Scene
            Scene scene;
            using (AssimpImporter importer = new AssimpImporter())
            {
                //is imported, loaded into managed memory. Then the unmanaged memory is released, and everything is reset.
                scene = importer.ImportFile(fi.FullName, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessPreset.ConvertToLeftHanded);
            }

            //Get components
            var components = ((IModelMeshComponents)new VertexFormat()).Components;

            //Build custom buffer format from passed in components
            bool isP = (components & ModelMesh.ModelMesh.ModelMeshComponents.P) == ModelMesh.ModelMesh.ModelMeshComponents.P;
            bool isN = (components & ModelMesh.ModelMesh.ModelMeshComponents.N) == ModelMesh.ModelMesh.ModelMeshComponents.N;
            bool isT = (components & ModelMesh.ModelMesh.ModelMeshComponents.T) == ModelMesh.ModelMesh.ModelMeshComponents.T;
            bool isC = (components & ModelMesh.ModelMesh.ModelMeshComponents.C) == ModelMesh.ModelMesh.ModelMeshComponents.C;

            //index Format
            TypeCode typeCode = Type.GetTypeCode(typeof(IndexFormat));
            IModel model = null;

            if (isP && !(isN || isT || isC))
            {
                if (typeCode == TypeCode.UInt16) model = Generate_P_UINT16(scene, device);
                if (typeCode == TypeCode.UInt32) model = Generate_P_UINT32(scene, device);
            }

            if (isP && isC &&!(isN || isT))
            {
                if (typeCode == TypeCode.UInt16) model = Generate_PC_UINT16(scene, device);
                if (typeCode == TypeCode.UInt32) model = Generate_PC_UINT32(scene, device);
            }

            if (isP && isN && !(isT || isC))
            {
                if (typeCode == TypeCode.UInt16) model = Generate_PN_UINT16(scene, device);
                if (typeCode == TypeCode.UInt32) model = Generate_PN_UINT32(scene, device);
            }

            if (isP && isN && isC && !(isT))
            {
                if (typeCode == TypeCode.UInt16) model = Generate_PNC_UINT16(scene, device);
                if (typeCode == TypeCode.UInt32) model = Generate_PNC_UINT32(scene, device);
            }

            if (isP && isN && isT && isC)
            {
                if (typeCode == TypeCode.UInt16) model = Generate_PNTC_UINT16(scene, device);
                if (typeCode == TypeCode.UInt32) model = Generate_PNTC_UINT32(scene, device);
            }

            if (model == null) return null;

            model.ModelFilePath = fi.FullName;
            model.Name = modelName;
            return model;
        }

        #region Vertex format P 
        /// <summary>
        /// Create Vertex bufer of format P
        /// </summary>
        /// <param name="m">Mesh containing the vertices</param>
        /// <returns></returns>
        private static ModelMesh_P[] CreateVB_P(Mesh m)
        {
            ModelMesh_P[] vertices = new ModelMesh_P[m.VertexCount];
            for (int v = 0; v < m.VertexCount; v++)
            {
                vertices[v] = new ModelMesh_P()
                {
                    Position = new SharpDX.Vector3(m.Vertices[v].X, m.Vertices[v].Y, m.Vertices[v].Z)
                };
            }
            return vertices;
        }

        /// <summary>
        /// P vertex Format generation 
        /// 16 bit ushort index buffer
        /// </summary>
        /// <param name="scene">Assimp Scene class</param>
        /// <param name="device">Dx11 device used to create Vertex/index Buffer</param>
        private static Model<ModelMesh_P, UInt16> Generate_P_UINT16(Scene scene, Device device)
        {
            //Create the model
            Model<ModelMesh_P, UInt16> model = new Model<ModelMesh_P, UInt16>();

            //Init mesh collection
            model.Meshes = new ModelMesh<ModelMesh_P, UInt16>[scene.MeshCount];

            for (int m = 0; m < scene.MeshCount; m++)
            {
                //Create new mesh
                ModelMesh<ModelMesh_P, UInt16> mm = new ModelMesh<ModelMesh_P, UInt16>();

                //AddMesh Texture information
                mm.Texture = new ModelMeshTexture()
                {
                    FilePath = scene.Materials[scene.Meshes[m].MaterialIndex].GetTexture(TextureType.Diffuse, 0).FilePath,
                    TextureArrayId = -1
                };

                //VERTEX BUFFER CREATION =============================================================
                ModelMesh_P[] vertices = CreateVB_P(scene.Meshes[m]);
                mm.VertexBuffer = new VertexBuffer<ModelMesh_P>(device, vertices.Length, GetPrimitiveTopology(scene.Meshes[m]), "Mesh VB");
                mm.VertexBuffer.SetData(device.ImmediateContext, vertices);

                //INDEX BUFFER CREATION =============================================================
                UInt16[] indices = GetUShortIndices(scene.Meshes[m]);
                mm.IndexBuffer = new IndexBuffer<UInt16>(device, indices.Length, "Mesh IB");
                mm.IndexBuffer.SetData(device.ImmediateContext, indices);

                model.Meshes[m] = mm;
            }

            return model;
        }

        /// <summary>
        /// P vertex Format generation 
        /// 32 bit ushort index buffer
        /// </summary>
        /// <param name="scene">Assimp Scene class</param>
        /// <param name="device">Dx11 device used to create Vertex/index Buffer</param>
        private static Model<ModelMesh_P, UInt32> Generate_P_UINT32(Scene scene, Device device)
        {
            //Create the model
            Model<ModelMesh_P, UInt32> model = new Model<ModelMesh_P, UInt32>();

            //Init mesh collection
            model.Meshes = new ModelMesh<ModelMesh_P, UInt32>[scene.MeshCount];

            for (int m = 0; m < scene.MeshCount; m++)
            {
                //Create new mesh
                ModelMesh<ModelMesh_P, UInt32> mm = new ModelMesh<ModelMesh_P, UInt32>();

                mm.Texture = new ModelMeshTexture()
                {
                    FilePath = scene.Materials[scene.Meshes[m].MaterialIndex].GetTexture(TextureType.Diffuse, 0).FilePath,
                    TextureArrayId = -1
                };

                //VERTEX BUFFER CREATION =============================================================
                ModelMesh_P[] vertices = CreateVB_P(scene.Meshes[m]);
                mm.VertexBuffer = new VertexBuffer<ModelMesh_P>(device, vertices.Length, GetPrimitiveTopology(scene.Meshes[m]), "Mesh VB");
                mm.VertexBuffer.SetData(device.ImmediateContext, vertices);

                //INDEX BUFFER CREATION =============================================================
                UInt32[] indices = scene.Meshes[m].GetIndices();
                mm.IndexBuffer = new IndexBuffer<UInt32>(device, indices.Length, "Mesh IB");
                mm.IndexBuffer.SetData(device.ImmediateContext, indices);

                model.Meshes[m] = mm;
            }

            return model;
        }
        #endregion

        #region Vertex format PC
        /// <summary>
        /// Create Vertex bufer of format PC2
        /// </summary>
        /// <param name="m">Mesh containing the vertices</param>
        /// <returns></returns>
        private static ModelMesh_PC[] CreateVB_PC(Mesh m)
        {
            ModelMesh_PC[] vertices = new ModelMesh_PC[m.VertexCount];

            var texCoord = m.GetTextureCoords(0);
            for (int v = 0; v < m.VertexCount; v++)
            {
                vertices[v] = new ModelMesh_PC()
                {
                    Position = new Vector3(m.Vertices[v].X, m.Vertices[v].Y, m.Vertices[v].Z),
                    TexCoord = new Vector2(texCoord[v].X, texCoord[v].Y),
                };
            }
            return vertices;
        }

        /// <summary>
        /// P vertex Format generation 
        /// 16 bit ushort index buffer
        /// </summary>
        /// <param name="scene">Assimp Scene class</param>
        /// <param name="device">Dx11 device used to create Vertex/index Buffer</param>
        private static Model<ModelMesh_PC, UInt16> Generate_PC_UINT16(Scene scene, Device device)
        {
            //Create the model
            Model<ModelMesh_PC, UInt16> model = new Model<ModelMesh_PC, UInt16>();

            //Init mesh collection
            model.Meshes = new ModelMesh<ModelMesh_PC, UInt16>[scene.MeshCount];

            for (int m = 0; m < scene.MeshCount; m++)
            {
                //Create new mesh
                ModelMesh<ModelMesh_PC, UInt16> mm = new ModelMesh<ModelMesh_PC, UInt16>();

                mm.Texture = new ModelMeshTexture()
                {
                    FilePath = scene.Materials[scene.Meshes[m].MaterialIndex].GetTexture(TextureType.Diffuse, 0).FilePath,
                    TextureArrayId = -1
                };

                //VERTEX BUFFER CREATION =============================================================
                ModelMesh_PC[] vertices = CreateVB_PC(scene.Meshes[m]);
                mm.VertexBuffer = new VertexBuffer<ModelMesh_PC>(device, vertices.Length, GetPrimitiveTopology(scene.Meshes[m]), "Mesh VB");
                mm.VertexBuffer.SetData(device.ImmediateContext, vertices);

                //INDEX BUFFER CREATION =============================================================
                UInt16[] indices = GetUShortIndices(scene.Meshes[m]);
                mm.IndexBuffer = new IndexBuffer<UInt16>(device, indices.Length, "Mesh IB");
                mm.IndexBuffer.SetData(device.ImmediateContext, indices);

                model.Meshes[m] = mm;
            }

            return model;
        }

        /// <summary>
        /// P vertex Format generation 
        /// 32 bit ushort index buffer
        /// </summary>
        /// <param name="scene">Assimp Scene class</param>
        /// <param name="device">Dx11 device used to create Vertex/index Buffer</param>
        private static Model<ModelMesh_PC, UInt32> Generate_PC_UINT32(Scene scene, Device device)
        {
            //Create the model
            Model<ModelMesh_PC, UInt32> model = new Model<ModelMesh_PC, UInt32>();

            //Init mesh collection
            model.Meshes = new ModelMesh<ModelMesh_PC, UInt32>[scene.MeshCount];

            for (int m = 0; m < scene.MeshCount; m++)
            {
                //Create new mesh
                ModelMesh<ModelMesh_PC, UInt32> mm = new ModelMesh<ModelMesh_PC, UInt32>();

                mm.Texture = new ModelMeshTexture()
                {
                    FilePath = scene.Materials[scene.Meshes[m].MaterialIndex].GetTexture(TextureType.Diffuse, 0).FilePath,
                    TextureArrayId = -1
                };

                //VERTEX BUFFER CREATION =============================================================
                ModelMesh_PC[] vertices = CreateVB_PC(scene.Meshes[m]);
                mm.VertexBuffer = new VertexBuffer<ModelMesh_PC>(device, vertices.Length, GetPrimitiveTopology(scene.Meshes[m]), "Mesh VB");
                mm.VertexBuffer.SetData(device.ImmediateContext, vertices);

                //INDEX BUFFER CREATION =============================================================
                UInt32[] indices = scene.Meshes[m].GetIndices();
                mm.IndexBuffer = new IndexBuffer<UInt32>(device, indices.Length, "Mesh IB");
                mm.IndexBuffer.SetData(device.ImmediateContext, indices);

                model.Meshes[m] = mm;
            }

            return model;
        }
        #endregion

        #region Vertex format PN 
        /// <summary>
        /// Create Vertex buffer of format PN
        /// </summary>
        /// <param name="m">Mesh containing the vertices</param>
        /// <returns></returns>
        private static ModelMesh_PN[] CreateVB_PN(Mesh m)
        {
            ModelMesh_PN[] vertices = new ModelMesh_PN[m.VertexCount];

            for (int v = 0; v < m.VertexCount; v++)
            {
                vertices[v] = new ModelMesh_PN()
                {
                    Position = new Vector3(m.Vertices[v].X, m.Vertices[v].Y, m.Vertices[v].Z),
                    Normal = new Vector3(m.Normals[v].X, m.Normals[v].Y, m.Normals[v].Z),
                };
            }
            return vertices;
        }

        /// <summary>
        /// P vertex Format generation 
        /// 16 bit ushort index buffer
        /// </summary>
        /// <param name="scene">Assimp Scene class</param>
        /// <param name="device">Dx11 device used to create Vertex/index Buffer</param>
        private static Model<ModelMesh_PN, UInt16> Generate_PN_UINT16(Scene scene, Device device)
        {
            //Create the model
            Model<ModelMesh_PN, UInt16> model = new Model<ModelMesh_PN, UInt16>();

            //Init mesh collection
            model.Meshes = new ModelMesh<ModelMesh_PN, UInt16>[scene.MeshCount];

            for (int m = 0; m < scene.MeshCount; m++)
            {
                //Create new mesh
                ModelMesh<ModelMesh_PN, UInt16> mm = new ModelMesh<ModelMesh_PN, UInt16>();

                mm.Texture = new ModelMeshTexture()
                {
                    FilePath = scene.Materials[scene.Meshes[m].MaterialIndex].GetTexture(TextureType.Diffuse, 0).FilePath,
                    TextureArrayId = -1
                };

                //VERTEX BUFFER CREATION =============================================================
                ModelMesh_PN[] vertices = CreateVB_PN(scene.Meshes[m]);
                mm.VertexBuffer = new VertexBuffer<ModelMesh_PN>(device, vertices.Length, GetPrimitiveTopology(scene.Meshes[m]), "Mesh VB");
                mm.VertexBuffer.SetData(device.ImmediateContext, vertices);

                //INDEX BUFFER CREATION =============================================================
                UInt16[] indices = GetUShortIndices(scene.Meshes[m]);
                mm.IndexBuffer = new IndexBuffer<UInt16>(device, indices.Length, "Mesh IB");
                mm.IndexBuffer.SetData(device.ImmediateContext, indices);

                model.Meshes[m] = mm;
            }

            return model;
        }

        /// <summary>
        /// P vertex Format generation 
        /// 32 bit ushort index buffer
        /// </summary>
        /// <param name="scene">Assimp Scene class</param>
        /// <param name="device">Dx11 device used to create Vertex/index Buffer</param>
        private static Model<ModelMesh_PN, UInt32> Generate_PN_UINT32(Scene scene, Device device)
        {
            //Create the model
            Model<ModelMesh_PN, UInt32> model = new Model<ModelMesh_PN, UInt32>();

            //Init mesh collection
            model.Meshes = new ModelMesh<ModelMesh_PN, UInt32>[scene.MeshCount];

            for (int m = 0; m < scene.MeshCount; m++)
            {
                //Create new mesh
                ModelMesh<ModelMesh_PN, UInt32> mm = new ModelMesh<ModelMesh_PN, UInt32>();

                mm.Texture = new ModelMeshTexture()
                {
                    FilePath = scene.Materials[scene.Meshes[m].MaterialIndex].GetTexture(TextureType.Diffuse, 0).FilePath,
                    TextureArrayId = -1
                };

                //VERTEX BUFFER CREATION =============================================================
                ModelMesh_PN[] vertices = CreateVB_PN(scene.Meshes[m]);
                mm.VertexBuffer = new VertexBuffer<ModelMesh_PN>(device, vertices.Length, GetPrimitiveTopology(scene.Meshes[m]), "Mesh VB");
                mm.VertexBuffer.SetData(device.ImmediateContext, vertices);

                //INDEX BUFFER CREATION =============================================================
                UInt32[] indices = scene.Meshes[m].GetIndices();
                mm.IndexBuffer = new IndexBuffer<UInt32>(device, indices.Length, "Mesh IB");
                mm.IndexBuffer.SetData(device.ImmediateContext, indices);

                model.Meshes[m] = mm;
            }

            return model;
        }
        #endregion

        #region Vertex format PNC
        /// <summary>
        /// Create Vertex buffer of format PNC
        /// </summary>
        /// <param name="m">Mesh containing the vertices</param>
        /// <returns></returns>
        private static ModelMesh_PNC[] CreateVB_PNC(Mesh m)
        {
            ModelMesh_PNC[] vertices = new ModelMesh_PNC[m.VertexCount];

            var texCoord = m.GetTextureCoords(0);
            for (int v = 0; v < m.VertexCount; v++)
            {
                vertices[v] = new ModelMesh_PNC()
                {
                    Position = new Vector3(m.Vertices[v].X, m.Vertices[v].Y, m.Vertices[v].Z),
                    Normal = new Vector3(m.Normals[v].X, m.Normals[v].Y, m.Normals[v].Z),
                    TexCoord = new Vector2(texCoord[v].X, texCoord[v].Y)
                };
            }
            return vertices;
        }

        /// <summary>
        /// P vertex Format generation 
        /// 16 bit ushort index buffer
        /// </summary>
        /// <param name="scene">Assimp Scene class</param>
        /// <param name="device">Dx11 device used to create Vertex/index Buffer</param>
        private static Model<ModelMesh_PNC, UInt16> Generate_PNC_UINT16(Scene scene, Device device)
        {
            //Create the model
            Model<ModelMesh_PNC, UInt16> model = new Model<ModelMesh_PNC, UInt16>();

            //Init mesh collection
            model.Meshes = new ModelMesh<ModelMesh_PNC, UInt16>[scene.MeshCount];

            for (int m = 0; m < scene.MeshCount; m++)
            {
                //Create new mesh
                ModelMesh<ModelMesh_PNC, UInt16> mm = new ModelMesh<ModelMesh_PNC, UInt16>();

                mm.Texture = new ModelMeshTexture()
                {
                    FilePath = scene.Materials[scene.Meshes[m].MaterialIndex].GetTexture(TextureType.Diffuse, 0).FilePath,
                    TextureArrayId = -1
                };

                //VERTEX BUFFER CREATION =============================================================
                ModelMesh_PNC[] vertices = CreateVB_PNC(scene.Meshes[m]);
                mm.VertexBuffer = new VertexBuffer<ModelMesh_PNC>(device, vertices.Length, GetPrimitiveTopology(scene.Meshes[m]), "Mesh VB");
                mm.VertexBuffer.SetData(device.ImmediateContext, vertices);

                //INDEX BUFFER CREATION =============================================================
                UInt16[] indices = GetUShortIndices(scene.Meshes[m]);
                mm.IndexBuffer = new IndexBuffer<UInt16>(device, indices.Length, "Mesh IB");
                mm.IndexBuffer.SetData(device.ImmediateContext, indices);

                model.Meshes[m] = mm;
            }

            return model;
        }

        /// <summary>
        /// P vertex Format generation 
        /// 32 bit ushort index buffer
        /// </summary>
        /// <param name="scene">Assimp Scene class</param>
        /// <param name="device">Dx11 device used to create Vertex/index Buffer</param>
        private static Model<ModelMesh_PNC, UInt32> Generate_PNC_UINT32(Scene scene, Device device)
        {
            //Create the model
            Model<ModelMesh_PNC, UInt32> model = new Model<ModelMesh_PNC, UInt32>();

            //Init mesh collection
            model.Meshes = new ModelMesh<ModelMesh_PNC, UInt32>[scene.MeshCount];

            for (int m = 0; m < scene.MeshCount; m++)
            {
                //Create new mesh
                ModelMesh<ModelMesh_PNC, UInt32> mm = new ModelMesh<ModelMesh_PNC, UInt32>();

                mm.Texture = new ModelMeshTexture()
                {
                    FilePath = scene.Materials[scene.Meshes[m].MaterialIndex].GetTexture(TextureType.Diffuse, 0).FilePath,
                    TextureArrayId = -1
                };

                //VERTEX BUFFER CREATION =============================================================
                ModelMesh_PNC[] vertices = CreateVB_PNC(scene.Meshes[m]);
                mm.VertexBuffer = new VertexBuffer<ModelMesh_PNC>(device, vertices.Length, GetPrimitiveTopology(scene.Meshes[m]), "Mesh VB");
                mm.VertexBuffer.SetData(device.ImmediateContext, vertices);

                //INDEX BUFFER CREATION =============================================================
                UInt32[] indices = scene.Meshes[m].GetIndices();
                mm.IndexBuffer = new IndexBuffer<UInt32>(device, indices.Length, "Mesh IB");
                mm.IndexBuffer.SetData(device.ImmediateContext, indices);

                model.Meshes[m] = mm;
            }

            return model;
        }
        #endregion

        #region Vertex format PNTC
        /// <summary>
        /// Create Vertex buffer of format PNTC
        /// </summary>
        /// <param name="m">Mesh containing the vertices</param>
        /// <returns></returns>
        private static ModelMesh_PNTC[] CreateVB_PNTC(Mesh m)
        {
            ModelMesh_PNTC[] vertices = new ModelMesh_PNTC[m.VertexCount];

            var texCoord = m.GetTextureCoords(0);
            for (int v = 0; v < m.VertexCount; v++)
            {
                vertices[v] = new ModelMesh_PNTC()
                {
                    Position = new Vector3(m.Vertices[v].X, m.Vertices[v].Y, m.Vertices[v].Z),
                    Normal = new Vector3(m.Normals[v].X, m.Normals[v].Y, m.Normals[v].Z),
                    TexCoord = new Vector2(texCoord[v].X, texCoord[v].Y),
                    Tangent = new Vector3(m.Tangents[v].X, m.Tangents[v].Y, m.Tangents[v].Z),
                };
            }
            return vertices;
        }

        /// <summary>
        /// P vertex Format generation 
        /// 16 bit ushort index buffer
        /// </summary>
        /// <param name="scene">Assimp Scene class</param>
        /// <param name="device">Dx11 device used to create Vertex/index Buffer</param>
        private static Model<ModelMesh_PNTC, UInt16> Generate_PNTC_UINT16(Scene scene, Device device)
        {
            //Create the model
            Model<ModelMesh_PNTC, UInt16> model = new Model<ModelMesh_PNTC, UInt16>();

            //Init mesh collection
            model.Meshes = new ModelMesh<ModelMesh_PNTC, UInt16>[scene.MeshCount];

            for (int m = 0; m < scene.MeshCount; m++)
            {
                //Create new mesh
                ModelMesh<ModelMesh_PNTC, UInt16> mm = new ModelMesh<ModelMesh_PNTC, UInt16>();

                mm.Texture = new ModelMeshTexture()
                {
                    FilePath = scene.Materials[scene.Meshes[m].MaterialIndex].GetTexture(TextureType.Diffuse, 0).FilePath,
                    TextureArrayId = -1
                };

                //VERTEX BUFFER CREATION =============================================================
                ModelMesh_PNTC[] vertices = CreateVB_PNTC(scene.Meshes[m]);
                mm.VertexBuffer = new VertexBuffer<ModelMesh_PNTC>(device, vertices.Length, GetPrimitiveTopology(scene.Meshes[m]), "Mesh VB");
                mm.VertexBuffer.SetData(device.ImmediateContext, vertices);

                //INDEX BUFFER CREATION =============================================================
                UInt16[] indices = GetUShortIndices(scene.Meshes[m]);
                mm.IndexBuffer = new IndexBuffer<UInt16>(device, indices.Length, "Mesh IB");
                mm.IndexBuffer.SetData(device.ImmediateContext, indices);

                model.Meshes[m] = mm;
            }

            return model;
        }

        /// <summary>
        /// P vertex Format generation 
        /// 32 bit ushort index buffer
        /// </summary>
        /// <param name="scene">Assimp Scene class</param>
        /// <param name="device">Dx11 device used to create Vertex/index Buffer</param>
        private static Model<ModelMesh_PNTC, UInt32> Generate_PNTC_UINT32(Scene scene, Device device)
        {
            //Create the model
            Model<ModelMesh_PNTC, UInt32> model = new Model<ModelMesh_PNTC, UInt32>();

            //Init mesh collection
            model.Meshes = new ModelMesh<ModelMesh_PNTC, UInt32>[scene.MeshCount];

            for (int m = 0; m < scene.MeshCount; m++)
            {
                //Create new mesh
                ModelMesh<ModelMesh_PNTC, UInt32> mm = new ModelMesh<ModelMesh_PNTC, UInt32>();

                mm.Texture = new ModelMeshTexture()
                {
                    FilePath = scene.Materials[scene.Meshes[m].MaterialIndex].GetTexture(TextureType.Diffuse, 0).FilePath,
                    TextureArrayId = -1
                };

                //VERTEX BUFFER CREATION =============================================================
                ModelMesh_PNTC[] vertices = CreateVB_PNTC(scene.Meshes[m]);
                mm.VertexBuffer = new VertexBuffer<ModelMesh_PNTC>(device, vertices.Length, GetPrimitiveTopology(scene.Meshes[m]), "Mesh VB");
                mm.VertexBuffer.SetData(device.ImmediateContext, vertices);

                //INDEX BUFFER CREATION =============================================================
                UInt32[] indices = scene.Meshes[m].GetIndices();
                mm.IndexBuffer = new IndexBuffer<UInt32>(device, indices.Length, "Mesh IB");
                mm.IndexBuffer.SetData(device.ImmediateContext, indices);

                model.Meshes[m] = mm;
            }

            return model;
        }
        #endregion

        private static UInt16[] GetUShortIndices(Mesh mesh)
        {
            if (mesh.HasFaces)
            {
                List<UInt16> indices = new List<UInt16>();
                foreach (Face face in mesh.Faces)
                {
                    if (face.IndexCount > 0 && face.Indices != null)
                    {
                        foreach (uint index in face.Indices)
                        {
                            indices.Add((UInt16)index);
                        }
                    }
                }
                return indices.ToArray();
            }
            return null;
        }

        private static PrimitiveTopology GetPrimitiveTopology(Mesh mesh)
        {
            switch (mesh.PrimitiveType)
            {
                case PrimitiveType.Line:
                    return PrimitiveTopology.LineList;
                case PrimitiveType.Point:
                    return PrimitiveTopology.PointList;
                case PrimitiveType.Triangle:
                    return PrimitiveTopology.TriangleList;
                default:
                    throw new Exception("Not supported Primitive type : " + mesh.PrimitiveType.ToString());
            }
        }
        #endregion

    }
}
