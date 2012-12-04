using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpDX.D3DCompiler;
using Utopia.Shared.Settings;

namespace UtopiaContent.Effects
{
    public class DefaultIncludeHandler : Include
    {
        private IDisposable _shadowCallBack = null;

        public void Close(Stream stream)
        {
            stream.Close();
        }

        public DefaultIncludeHandler()
        {
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            string IncludeFilePath;

            //Look inside the activated custom Effect pack directory
            IncludeFilePath = Path.Combine(ClientSettings.EffectPack, fileName);
            if (File.Exists(IncludeFilePath))
            {
                return File.Open(IncludeFilePath, FileMode.Open);
            }

            //Look inside Default Directory
            IncludeFilePath = @"Effects\" + fileName;
            if (File.Exists(IncludeFilePath))
            {
                return File.Open(IncludeFilePath, FileMode.Open);
            }

            //Impossible to locate the Include file !!
            throw new FileNotFoundException("Cannot find the effect include file", fileName);
        }

        public IDisposable Shadow
        {
            get
            {
                return _shadowCallBack;
            }
            set
            {
                _shadowCallBack = value;
            }
        }

        public void Dispose()
        {
            if (_shadowCallBack != null ) _shadowCallBack.Dispose();
        }
    }
}
