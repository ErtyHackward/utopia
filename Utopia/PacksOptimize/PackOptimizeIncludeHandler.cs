using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S33m3Engines.Effects;
using SharpDX.D3DCompiler;

namespace PacksOptimize
{
    public class PackOptimizeIncludeHandler : DefaultIncludeHandler
    {
        private string _includePath;

        public PackOptimizeIncludeHandler(string IncludePath = null)
        {
            _includePath = IncludePath;
        }

        public override Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            if (_includePath != null)
            {
                string IncludeFilePath;

                //Look inside the activated custom Effect pack directory
                IncludeFilePath = Path.Combine(_includePath, fileName);
                if (File.Exists(IncludeFilePath))
                {
                    return File.Open(IncludeFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
            }

            return base.Open(type, fileName, parentStream);
        }
    }
}
