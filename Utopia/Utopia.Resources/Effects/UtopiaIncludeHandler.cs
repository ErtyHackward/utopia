using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using S33m3Engines.Effects;
using SharpDX.D3DCompiler;
using Utopia.Shared.Settings;

namespace UtopiaContent.Effects
{
    public class UtopiaIncludeHandler : DefaultIncludeHandler
    {
        public override Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            string IncludeFilePath;

            //Look inside the activated custom Effect pack directory
            IncludeFilePath = Path.Combine(ClientSettings.EffectPack, fileName);
            if (File.Exists(IncludeFilePath))
            {
                return File.Open(IncludeFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            return base.Open(type, fileName, parentStream);
        }
    }
}
