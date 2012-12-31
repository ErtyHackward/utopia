using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacksOptimize
{
    class Program
    {
        private static string rootPath;
        private static ShaderFlags compilationFlag;
         
        static void Main(string[] args)
        {

            //First agument = Pack Root Path
            if (args.Length != 2)
            {
                Console.WriteLine("Utopia Pack Optimizer");
                Console.WriteLine("Syntax : PackOptimize path='rootPath' debugcompil=0");
                return;
            }
            else
            {
                foreach (string param in args)
                {
                    string[] paramData = param.Split('=');
                    if(paramData.Length != 2) return;
                    switch (paramData[0])
                    {
                        case "path":
                            rootPath = paramData[1].Replace("\'", "");
                            break;
                        case "debugcompil":
                            if (paramData[1] == "1")
                            {
                                compilationFlag = ShaderFlags.Debug | ShaderFlags.SkipOptimization;
                            }
                            else
                            {
                                compilationFlag = ShaderFlags.OptimizationLevel3;
                            }
                            break;
                        default:
                            break;
                    }
                }
                StartPackOptimazing();
            }
        }

        private static void StartPackOptimazing()
        {
            //Will Compile all effect files from a given Directory
            EffectCompiler effectCompiler = new EffectCompiler(compilationFlag);
            effectCompiler.ProcessDirectory(rootPath);

        }
    }
}
