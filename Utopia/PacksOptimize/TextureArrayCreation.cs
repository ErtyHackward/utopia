using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using S33M3DXEngine;
using S33M3DXEngine.Textures;
using SharpDX.Direct3D11;

namespace PacksOptimize
{
    public class TextureArrayCreation : IDisposable
    {
        private struct TextureArrayMetaData
        {
            public string DirectoryPath;
            public string FileFilters;
            public SharpDX.DXGI.Format ArrayTextureCompressionMode;
        }

        #region Private Variables
        private List<TextureArrayMetaData> PackTextureArrayDirectoriesInfo = new List<TextureArrayMetaData>() 
        {
            new TextureArrayMetaData() { DirectoryPath = "AnimatedTextures", FileFilters = "*.png", ArrayTextureCompressionMode = SharpDX.DXGI.Format.BC4_UNorm },
            new TextureArrayMetaData() { DirectoryPath = "BiomesColors", FileFilters = "*.png", ArrayTextureCompressionMode = SharpDX.DXGI.Format.BC1_UNorm },
            new TextureArrayMetaData() { DirectoryPath = "Particules", FileFilters = "*.png", ArrayTextureCompressionMode = SharpDX.DXGI.Format.R8G8B8A8_UNorm },
            new TextureArrayMetaData() { DirectoryPath = "Terran", FileFilters = "*.png", ArrayTextureCompressionMode = SharpDX.DXGI.Format.R8G8B8A8_UNorm }
        };
        private string _texturePackPath;
        private D3DEngine _engine;
        #endregion

        #region Public Properties
        #endregion

        public TextureArrayCreation(string texturePackPath)
        {
            _texturePackPath = texturePackPath;
            _engine = new D3DEngine();
        }

        public void Dispose()
        {
            _engine.Dispose();
        }

        #region Public Methods
        public void CreateTextureArrays()
        {
            //ForEach Pack Installed
            foreach (var dir in Directory.GetDirectories(_texturePackPath))
            {
                PackArrayCreation(dir);
            }
        }
        #endregion

        #region Private Methods
        private void PackArrayCreation(string PackPath)
        {
            foreach (var data in PackTextureArrayDirectoriesInfo)
            {
                if (Directory.Exists(Path.Combine(PackPath, data.DirectoryPath)))
                    CreateTextureArray(data.DirectoryPath, Path.Combine(PackPath, data.DirectoryPath), data.FileFilters, data.ArrayTextureCompressionMode);
            }
        }

        private void CreateTextureArray(string directoryName, string path, string fileFilters, SharpDX.DXGI.Format ArrayTextureCompressionMode)
        {
            //Get File names from Directory
            Texture2D textureArray = ArrayTexture.CreateImageArrayFromFiles(_engine.ImmediateContext, Directory.GetFiles(path, fileFilters), FilterFlags.Point, ArrayTextureCompressionMode);
            Texture2D.ToFile(_engine.ImmediateContext, textureArray, ImageFileFormat.Dds, path + @"\Array" + directoryName + ".dds");

            textureArray.Dispose();
        }
        #endregion

    }
}
