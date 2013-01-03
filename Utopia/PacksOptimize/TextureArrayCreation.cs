﻿using System;
using System.Collections.Generic;
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
        #region Private Variables
        private List<string> PackTextureArrayDirectories = new List<string>() 
        {
            @"AnimatedTextures\*.png",
            @"BiomesColors\*.png",
            @"Particules\*.png",
            @"Terran\*.png"
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
            foreach (var directory in PackTextureArrayDirectories)
            {
                string[] split = directory.Split('\\');
                CreateTextureArray(split[0], Path.Combine(PackPath, split[0]), split[1]);
            }
        }

        private void CreateTextureArray(string directoryName, string path, string fileFilters)
        {
            //Get File names from Directory
            Texture2D textureArray = ArrayTexture.CreateImageArrayFromFiles(_engine.ImmediateContext, Directory.GetFiles(path, fileFilters), FilterFlags.Point);

            Texture2D.ToFile(_engine.ImmediateContext, textureArray, ImageFileFormat.Dds, path + @"\Array" + directoryName + ".dds");

            textureArray.Dispose();
        }
        #endregion





    }
}
